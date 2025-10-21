
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopInventory.Data;
using ShopInventory.Models;

namespace ShopInventory.Controllers
{
    public class PurchasesController : Controller
    {
        private readonly ApplicationDbContext _context;
        public PurchasesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(DateTime? startDate = null, DateTime? endDate = null, string? searchTerm = null)
        {

            // 1. Today's statistics (combine purchases + sales)
            var today = DateTime.Today;

            var todayPurchaseAmount = await _context.PurchaseInvoices
                .Where(p => p.Date.Date == today)
                .SumAsync(p => (decimal?)p.TotalAmount) ?? 0;
            var todaySalesAmount = await _context.SalesInvoices
                .Where(s => s.Date.Date == today)
                .SumAsync(s => (decimal?)s.PaidAmount) ?? 0;
            var todayAmount = todayPurchaseAmount + todaySalesAmount;
            ViewData["TodayPurchasesAmount"] = todayAmount;

            // Net profit = total sales (PaidAmount) - total purchases (TotalAmount)
            var netProfit = todaySalesAmount - todayPurchaseAmount;
            ViewData["DailyNetProfit"] = netProfit;

            // ...existing code...

            // 2. Filtered statistics (by startDate, endDate, searchTerm)
            var filteredQuery = _context.PurchaseInvoices
                .Include(p => p.Supplier)
                .Include(p => p.CreatedByUser)
                .Include(p => p.Items)
                .AsQueryable();

            if (startDate.HasValue)
                filteredQuery = filteredQuery.Where(p => p.Date.Date >= startDate.Value.Date);
            if (endDate.HasValue)
                filteredQuery = filteredQuery.Where(p => p.Date.Date <= endDate.Value.Date);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                filteredQuery = filteredQuery.Where(p =>
                    (p.InvoiceNumber != null && p.InvoiceNumber.Contains(searchTerm)) ||
                    (p.Supplier != null && p.Supplier.Name != null && p.Supplier.Name.Contains(searchTerm)) ||
                    (p.CreatedByUser != null && p.CreatedByUser.UserName != null && p.CreatedByUser.UserName.Contains(searchTerm))
                );
            }

            var filteredPurchaseAmount = await filteredQuery.SumAsync(p => (decimal?)p.TotalAmount) ?? 0;
            var filteredPurchaseCount = await filteredQuery.CountAsync();

            // Calculate total remaining (Balance) from all invoices (purchases + sales) for the filtered period
            var filteredPurchasesBalance = (await filteredQuery
                .Select(p => new { p.TotalAmount, p.PaidAmount })
                .ToListAsync())
                .Sum(p => p.TotalAmount - p.PaidAmount);

            // compute filtered sales as well
            var filteredSalesQuery = _context.SalesInvoices.AsQueryable();
            if (startDate.HasValue)
                filteredSalesQuery = filteredSalesQuery.Where(s => s.Date.Date >= startDate.Value.Date);
            if (endDate.HasValue)
                filteredSalesQuery = filteredSalesQuery.Where(s => s.Date.Date <= endDate.Value.Date);
            if (!string.IsNullOrWhiteSpace(searchTerm))
                filteredSalesQuery = filteredSalesQuery.Where(s => s.InvoiceNumber != null && s.InvoiceNumber.Contains(searchTerm));

            var filteredSalesAmount = await filteredSalesQuery.SumAsync(s => (decimal?)s.PaidAmount) ?? 0;
            var filteredSalesCount = await filteredSalesQuery.CountAsync();

            var filteredSalesBalance = (await filteredSalesQuery
                .Select(s => new { s.TotalAmount, s.PaidAmount })
                .ToListAsync())
                .Sum(s => s.TotalAmount - s.PaidAmount);
            var totalAllInvoicesBalanceFiltered = filteredPurchasesBalance + filteredSalesBalance;
            ViewData["TotalAllInvoicesBalanceToday"] = totalAllInvoicesBalanceFiltered;

            var filteredAmount = filteredPurchaseAmount + filteredSalesAmount;
            var filteredCount = filteredPurchaseCount + filteredSalesCount;

            ViewData["TotalPurchasesAmount"] = filteredAmount;
            ViewData["TotalPurchasesCount"] = filteredCount;

            var purchases = await filteredQuery.OrderByDescending(p => p.Date).ToListAsync();

            // Also include Sales invoices so the Purchases index shows all invoices
            var salesQuery = _context.SalesInvoices
                .Include(s => s.Customer)
                .Include(s => s.CreatedByUser)
                .Include(s => s.SalesInvoiceItems)
                .AsQueryable();

            if (startDate.HasValue)
                salesQuery = salesQuery.Where(s => s.Date.Date >= startDate.Value.Date);
            if (endDate.HasValue)
                salesQuery = salesQuery.Where(s => s.Date.Date <= endDate.Value.Date);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                salesQuery = salesQuery.Where(s => s.InvoiceNumber != null && s.InvoiceNumber.Contains(searchTerm));
            }

            var sales = await salesQuery.OrderByDescending(s => s.Date).ToListAsync();

            // Map to InvoiceListItem
            var list = new List<InvoiceListItem>();

            list.AddRange(purchases.Select(p => new InvoiceListItem
            {
                InvoiceNumber = p.InvoiceNumber ?? string.Empty,
                Date = p.Date,
                PartyName = p.Supplier?.Name ?? string.Empty,
                ItemsCount = p.Items?.Count ?? 0,
                TotalAmount = p.TotalAmount,
                CreatedBy = p.CreatedByUser?.UserName ?? string.Empty,
                IsPurchase = true,
                Id = p.Id
            }));

            list.AddRange(sales.Select(s => new InvoiceListItem
            {
                InvoiceNumber = s.InvoiceNumber ?? string.Empty,
                Date = s.Date,
                PartyName = s.Customer?.Name ?? string.Empty,
                ItemsCount = s.SalesInvoiceItems?.Count ?? 0,
                TotalAmount = s.TotalAmount,
                CreatedBy = s.CreatedByUser?.UserName ?? string.Empty,
                IsPurchase = false,
                Id = s.Id
            }));

            // Order combined list by date desc
            var combined = list.OrderByDescending(i => i.Date).ToList();

            return View(combined);
        }

        // Accept either invoice number or numeric id in the URL (e.g. /Purchases/Details/PI-20251020173543)
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            // Try to find purchase by InvoiceNumber first
            var purchase = await _context.PurchaseInvoices
                .Include(p => p.Items)
                .Include(p => p.CreatedByUser)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(p => p.InvoiceNumber == id);

            if (purchase != null)
                return View(purchase);

            // If not a purchase, try SalesInvoices by InvoiceNumber and render the Sales details view
            var sale = await _context.SalesInvoices
                .Include(s => s.SalesInvoiceItems)
                .Include(s => s.CreatedByUser)
                .Include(s => s.Customer)
                .FirstOrDefaultAsync(s => s.InvoiceNumber == id);

            if (sale != null)
                return View("~/Views/Sales/Details.cshtml", sale);

            // Fallback: if id is numeric, try by numeric PK on purchases then sales
            if (int.TryParse(id, out var intId))
            {
                purchase = await _context.PurchaseInvoices
                    .Include(p => p.Items)
                    .Include(p => p.CreatedByUser)
                    .Include(p => p.Supplier)
                    .FirstOrDefaultAsync(p => p.Id == intId);

                if (purchase != null)
                    return View(purchase);

                sale = await _context.SalesInvoices
                    .Include(s => s.SalesInvoiceItems)
                    .Include(s => s.CreatedByUser)
                    .Include(s => s.Customer)
                    .FirstOrDefaultAsync(s => s.Id == intId);

                if (sale != null)
                    return View("~/Views/Sales/Details.cshtml", sale);
            }

            return NotFound();
        }
    }
}