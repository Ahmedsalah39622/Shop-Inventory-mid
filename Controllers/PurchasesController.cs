
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

            // Compute today's totals. Per the requested behavior, Net Profit is the sum of sales totals only.
            var todayPurchasePaid = await _context.PurchaseInvoices
                .Where(p => p.Date.Date == today)
                .SumAsync(p => (decimal?)p.PaidAmount) ?? 0;

            // Sum of sales total amounts for today (not PaidAmount)
            var todaySalesTotal = await _context.SalesInvoices
                .Where(s => s.Date.Date == today)
                .SumAsync(s => (decimal?)s.TotalAmount) ?? 0;

            // Today's total cash (إجمالي اليوم) remains the sum of paid amounts for purchases+sales
            var todaySalesPaid = await _context.SalesInvoices
                .Where(s => s.Date.Date == today)
                .SumAsync(s => (decimal?)s.PaidAmount) ?? 0;

            var todayTotal = todayPurchasePaid + todaySalesPaid;
            ViewData["TodayPurchasesAmount"] = todayTotal;

            // Calculate today's outstanding balances (باقي النقود) for purchases and sales (unchanged)
            var todayPurchasesBalance = (await _context.PurchaseInvoices
                .Where(p => p.Date.Date == today)
                .Select(p => new { p.TotalAmount, p.PaidAmount })
                .ToListAsync())
                .Sum(p => p.TotalAmount - p.PaidAmount);

            var todaySalesBalance = (await _context.SalesInvoices
                .Where(s => s.Date.Date == today)
                .Select(s => new { s.TotalAmount, s.PaidAmount })
                .ToListAsync())
                .Sum(s => s.TotalAmount - s.PaidAmount);

            // Keep the today's customer outstanding available for the view
            var totalCustomerInvoicesBalanceToday = todaySalesBalance;
            ViewData["TotalAllInvoicesBalanceToday"] = totalCustomerInvoicesBalanceToday;

            // Today's approved sales returns: count and total amount
            var todayReturnsQuery = _context.Returns
                .Where(r => r.Type == "Sales" && r.Status == "Approved" && r.Date.Date == today);
            var todayReturnsCount = await todayReturnsQuery.CountAsync();
            var todayReturnsTotal = await todayReturnsQuery.SumAsync(r => (decimal?)r.TotalAmount) ?? 0m;
            ViewData["TodayReturnsCount"] = todayReturnsCount;
            ViewData["TodayReturnsTotal"] = todayReturnsTotal;

            // Net profit = sum of today's sales totals (TotalAmount) minus approved sales returns
            var dailyNetProfit = todaySalesTotal - todayReturnsTotal;
            ViewData["DailyNetProfit"] = dailyNetProfit;

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

            // Use PaidAmount for filtered purchases as well (cash-out)
            var filteredPurchaseAmount = await filteredQuery.SumAsync(p => (decimal?)p.PaidAmount) ?? 0;
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

            // Use TotalAmount for filtered sales revenue when computing net profit (sum of sales totals as requested)
            var filteredSalesAmount = await filteredSalesQuery.SumAsync(s => (decimal?)s.TotalAmount) ?? 0;
            var filteredSalesCount = await filteredSalesQuery.CountAsync();

            var filteredSalesBalance = (await filteredSalesQuery
                .Select(s => new { s.TotalAmount, s.PaidAmount })
                .ToListAsync())
                .Sum(s => s.TotalAmount - s.PaidAmount);
            // For filtered view, keep the filtered customer outstanding available
            var totalCustomerInvoicesBalanceFiltered = filteredSalesBalance;
            ViewData["TotalAllInvoicesBalanceFiltered"] = totalCustomerInvoicesBalanceFiltered;

            var filteredAmount = filteredPurchaseAmount + filteredSalesAmount;
            var filteredCount = filteredPurchaseCount + filteredSalesCount;

            // Filtered net profit = sum of filtered sales totals (TotalAmount)
            // Filtered approved sales returns: count and total amount (respect filters)
            var filteredReturnsQuery = _context.Returns.AsQueryable();
            if (startDate.HasValue)
                filteredReturnsQuery = filteredReturnsQuery.Where(r => r.Date.Date >= startDate.Value.Date);
            if (endDate.HasValue)
                filteredReturnsQuery = filteredReturnsQuery.Where(r => r.Date.Date <= endDate.Value.Date);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                // If a searchTerm exists, we try to match it against invoice number or supplier/customer names by joining returns' original invoice or supplier - keep simple: skip complex joins here
            }
            filteredReturnsQuery = filteredReturnsQuery.Where(r => r.Type == "Sales" && r.Status == "Approved");
            var filteredReturnsCount = await filteredReturnsQuery.CountAsync();
            var filteredReturnsTotal = await filteredReturnsQuery.SumAsync(r => (decimal?)r.TotalAmount) ?? 0m;
            ViewData["FilteredReturnsCount"] = filteredReturnsCount;
            ViewData["FilteredReturnsTotal"] = filteredReturnsTotal;
            // Filtered net profit = filtered sales totals minus approved returns in the filtered period
            var filteredNetProfit = filteredSalesAmount - filteredReturnsTotal;
            ViewData["FilteredNetProfit"] = filteredNetProfit;

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