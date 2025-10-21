using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopInventory.Data;
using ShopInventory.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShopInventory.Controllers
{
    [Route("api/sales")]
    [ApiController]
    public class SalesApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        
        public SalesApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        public class QuickSaleRequest
        {
            public required int CustomerId { get; set; }
            public required List<QuickSaleItem> Items { get; set; } = new();
            public required decimal AmountPaid { get; set; }
            public string? PaymentMethod { get; set; }
            public string? InvoiceType { get; set; }
            // نوع التقسيط: "بنكي" او "كمبيالات"
            public string? InstallmentType { get; set; }
            // Installment specific
            public decimal? DownPayment { get; set; }
            public int? NumberOfMonths { get; set; }
        }

        public class QuickSaleItem
        {
            public required int ItemId { get; set; }
            public required int Quantity { get; set; }
            public required decimal UnitPrice { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> CreateSale([FromBody] QuickSaleRequest request)
        {
            if (request.Items == null || !request.Items.Any())
                return BadRequest("No items in cart");

            var totalAmount = request.Items.Sum(i => i.Quantity * i.UnitPrice);

            // Parse optional enums early so we can validate payment rules
            PaymentMethod? parsedPaymentMethod = null;
            InvoiceType? parsedInvoiceType = null;
            if (!string.IsNullOrWhiteSpace(request.PaymentMethod) && Enum.TryParse<PaymentMethod>(request.PaymentMethod, true, out var pm))
                parsedPaymentMethod = pm;
            if (!string.IsNullOrWhiteSpace(request.InvoiceType) && Enum.TryParse<InvoiceType>(request.InvoiceType, true, out var it))
                parsedInvoiceType = it;

            // If this is an installment sale (تقسيط) or a credit invoice (اجل), only require down payment (المقدم)
            var isInstallmentOrCredit = (parsedPaymentMethod == PaymentMethod.Installment) || (parsedInvoiceType == InvoiceType.Credit);
            var requiredPayment = isInstallmentOrCredit ? (request.DownPayment ?? 0m) : totalAmount;

            // Server-side validation: down payment cannot exceed total amount
            if (isInstallmentOrCredit && (request.DownPayment ?? 0m) > totalAmount)
                return BadRequest("Down payment cannot be greater than total amount.");

            if (request.AmountPaid < requiredPayment)
                return BadRequest(isInstallmentOrCredit
                    ? "Amount paid must be at least the down payment for installment/credit invoices."
                    : "Insufficient payment amount");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var prefix = DateTime.Now.ToString("yyyyMMdd");
                var lastInvoice = await _context.SalesInvoices
                    .Where(s => s.InvoiceNumber.StartsWith(prefix))
                    .OrderByDescending(s => s.InvoiceNumber)
                    .FirstOrDefaultAsync();

                int sequence = 1;
                if (lastInvoice != null && int.TryParse(lastInvoice.InvoiceNumber[8..], out int lastSequence))
                {
                    sequence = lastSequence + 1;
                }

                // Get current user
                var cashierUser = await _context.Users.FirstOrDefaultAsync();
                var cashierUserId = cashierUser?.Id ?? "system";

                // determine effective paid amount depending on installment type
                var effectivePaid = request.AmountPaid;
                var requestedInstallmentType = (request.InstallmentType ?? string.Empty).Trim();
                var canonicalType = requestedInstallmentType.ToLowerInvariant();
                if (parsedPaymentMethod == PaymentMethod.Installment)
                {
                    // detect bank variant (بنكي / بنك)
                    if (canonicalType.Contains("بنكي") || canonicalType.Contains("بنك"))
                    {
                        // bank installments: mark invoice as fully paid
                        effectivePaid = totalAmount;
                    }
                    // detect promissory/كمبيالات variant
                    else if (canonicalType.Contains("كمبي") || canonicalType.Contains("كمبيا") || canonicalType.Contains("كمبيالات"))
                    {
                        // كمبيالات: only the down payment is considered paid for sales totals
                        effectivePaid = request.DownPayment ?? 0m;
                    }
                    else
                    {
                        // default for other installment types: treat paid as provided (but ensure not negative)
                        effectivePaid = Math.Max(0, effectivePaid);
                    }
                }

                var newInvoice = new SalesInvoice
                {
                    InvoiceNumber = $"{prefix}{sequence:D4}",
                    CustomerId = request.CustomerId,
                    Date = DateTime.Now,
                    TotalAmount = totalAmount,
                    PaidAmount = effectivePaid,
                    CreatedByUserId = cashierUserId
                };

                // Map optional parsed enums to the invoice
                if (parsedPaymentMethod.HasValue)
                    newInvoice.PaymentMethod = parsedPaymentMethod.Value;
                if (parsedInvoiceType.HasValue)
                    newInvoice.InvoiceType = parsedInvoiceType.Value;

                _context.SalesInvoices.Add(newInvoice);
                await _context.SaveChangesAsync();

                foreach (var item in request.Items)
                {
                    var dbItem = await _context.Items.FindAsync(item.ItemId);
                    if (dbItem == null)
                        throw new InvalidOperationException($"Item {item.ItemId} not found");

                    if (dbItem.Quantity < item.Quantity)
                        throw new InvalidOperationException($"Not enough stock for item {dbItem.Name}");

                    _context.SalesInvoiceItems.Add(new SalesInvoiceItem
                    {
                        SalesInvoiceId = newInvoice.Id,
                        ItemId = item.ItemId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    });

                    dbItem.Quantity -= item.Quantity;
                }

                await _context.SaveChangesAsync();

                // If payment method is Installment, create an Installment record
                if (parsedPaymentMethod == PaymentMethod.Installment)
                {
                    var down = request.DownPayment ?? 0m;
                    var months = request.NumberOfMonths ?? 1;
                    var instType = (request.InstallmentType ?? string.Empty).Trim();

                    // Special handling for bank installments: treat as fully collected
                    if (string.Equals(instType, "بنكي", StringComparison.OrdinalIgnoreCase))
                    {
                        // For bank installments, consider the sale fully collected immediately
                        var inst = new Installment
                        {
                            CustomerId = newInvoice.CustomerId,
                            SalesInvoiceId = newInvoice.Id,
                            TotalAmount = newInvoice.TotalAmount,
                            DownPayment = newInvoice.TotalAmount,
                            RemainingAmount = 0m,
                            NumberOfMonths = 0,
                            MonthlyAmount = 0m,
                            Status = "تم التحصيل كاملا",
                            NextPaymentDate = null,
                            InstallmentType = instType
                        };

                        // Ensure the database has the InstallmentType column (helps when adding this property without a migration)
                        try
                        {
                            var ensureSql = @"IF COL_LENGTH('dbo.Installments', 'InstallmentType') IS NULL 
                                                BEGIN 
                                                    ALTER TABLE dbo.Installments ADD InstallmentType NVARCHAR(100) NULL; 
                                                END";
                            await _context.Database.ExecuteSqlRawAsync(ensureSql);
                        }
                        catch
                        {
                            // ignore
                        }

                        _context.Add(inst);
                        await _context.SaveChangesAsync();

                        // create a payment record representing the full collected amount
                        var payment = new InstallmentPayment
                        {
                            InstallmentId = inst.Id,
                            Amount = inst.DownPayment,
                            PaymentMethod = request.PaymentMethod ?? "Installment",
                            Date = DateTime.Now
                        };
                        _context.InstallmentPayments.Add(payment);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        var remaining = newInvoice.TotalAmount - down;
                        var monthly = months > 0 ? Math.Round(remaining / months, 2) : remaining;

                        var inst = new Installment
                        {
                            CustomerId = newInvoice.CustomerId,
                            SalesInvoiceId = newInvoice.Id,
                            TotalAmount = newInvoice.TotalAmount,
                            DownPayment = down,
                            RemainingAmount = remaining,
                            NumberOfMonths = months,
                            MonthlyAmount = monthly,
                            Status = remaining > 0 ? "Active" : "تم التحصيل كاملا",
                            NextPaymentDate = remaining > 0 ? DateTime.Now.AddMonths(1) : (DateTime?)null,
                            InstallmentType = instType
                        };

                        try
                        {
                            var ensureSql = @"IF COL_LENGTH('dbo.Installments', 'InstallmentType') IS NULL 
                                                BEGIN 
                                                    ALTER TABLE dbo.Installments ADD InstallmentType NVARCHAR(100) NULL; 
                                                END";
                            await _context.Database.ExecuteSqlRawAsync(ensureSql);
                        }
                        catch
                        {
                        }

                        _context.Add(inst);
                        await _context.SaveChangesAsync();
                    }
                }
                await transaction.CommitAsync();

                return Ok(new { Id = newInvoice.Id, InvoiceNumber = newInvoice.InvoiceNumber });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Surface inner exception for easier debugging in the UI (remove/limit in production)
                var inner = ex.InnerException?.Message ?? ex.Message;
                return BadRequest($"{ex.Message} - {inner}");
            }
        }
    }
}
