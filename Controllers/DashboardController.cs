using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopInventory.Data;
using ShopInventory.Models;

namespace ShopInventory.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var today = DateTime.Today;
        var viewModel = new DashboardViewModel();

        try
        {
            // Purchases
            viewModel.TotalPurchases = await _context.PurchaseInvoices
                .Where(p => p.Date.Date == today)
                .SumAsync(p => p.TotalAmount);

            // Sales: use sum of TotalAmount (full sale value) and subtract approved sales returns so the card reflects net sales
            var totalSalesAmount = await _context.SalesInvoices
                .Where(s => s.Date.Date == today)
                .SumAsync(s => (decimal?)s.TotalAmount) ?? 0m;
            viewModel.TotalSales = totalSalesAmount;

            viewModel.TotalOrders = await _context.PurchaseInvoices
                .Where(p => p.Date.Date == today)
                .CountAsync();

            viewModel.LowStockItems = await _context.Items
                .Where(i => i.Quantity <= i.ReorderLevel && i.IsActive)
                .CountAsync();

            // Count today's sales (number of sales invoices)
            viewModel.SalesCount = await _context.SalesInvoices
                .Where(s => s.Date.Date == today)
                .CountAsync();

            // Compute previous day values for percentage change
            var prevDay = today.AddDays(-1);
            // Previous day sales total (subtract approved returns on that day)
            var prevDaySalesTotal = await _context.SalesInvoices
                .Where(s => s.Date.Date == prevDay)
                .SumAsync(s => (decimal?)s.TotalAmount) ?? 0m;
            var prevDayReturnsTotal = await _context.Returns
                .Where(r => r.Type == "Sales" && r.Status == "Approved" && r.Date.Date == prevDay)
                .SumAsync(r => (decimal?)r.TotalAmount) ?? 0m;
            prevDaySalesTotal = Math.Max(0, prevDaySalesTotal - prevDayReturnsTotal);

            var prevDaySalesCount = await _context.SalesInvoices
                .Where(s => s.Date.Date == prevDay)
                .CountAsync();

            // Helper to compute percent change; when previous is 0, treat as 0% if both 0, otherwise 100%
            decimal computePercent(decimal todayVal, decimal prevVal)
            {
                if (prevVal == 0m)
                {
                    if (todayVal == 0m) return 0m;
                    return 100m;
                }
                return Math.Round((todayVal - prevVal) / prevVal * 100m, 2);
            }

            viewModel.SalesAmountPercentChange = computePercent(viewModel.TotalSales, prevDaySalesTotal);
            viewModel.SalesAmountIncreased = viewModel.TotalSales >= prevDaySalesTotal;
            viewModel.SalesCountPercentChange = computePercent(viewModel.SalesCount, prevDaySalesCount);
            viewModel.SalesCountIncreased = viewModel.SalesCount >= prevDaySalesCount;

            // Get best selling items data
            var items = await _context.Items
                .Where(i => i.IsActive)
                .Select(i => new ItemWithSalesCount {
                    Id = i.Id,
                    Name = i.Name,
                    PurchaseQuantity = (int)(_context.PurchaseInvoiceItems.Where(pii => pii.ItemId == i.Id).Sum(pii => (decimal?)pii.Quantity) ?? 0)
                })
                .ToListAsync();

            var bestSelling = items
                .OrderByDescending(x => x.PurchaseQuantity)
                .Take(10)
                .Select(x => new BestSellingItemViewModel {
                    Name = x.Name,
                    Quantity = x.PurchaseQuantity
                })
                .ToList();

            ViewBag.BestSelling = bestSelling;

            var todaySales = await _context.SalesInvoiceItems
                .Include(si => si.Item)
                .Include(si => si.SalesInvoice)
                    .ThenInclude(s => s!.CreatedByUser)
                .Where(si => si.SalesInvoice != null && si.SalesInvoice.Date.Date == today && si.SalesInvoice.CreatedByUser != null)
                .ToListAsync();
            // Also subtract any approved sales returns from today's sales counts
            var todayReturns = await _context.ReturnItems
                .Include(ri => ri.Return)
                .Include(ri => ri.Item)
                .Where(ri => ri.Return != null && ri.Return.Type == "Sales" && ri.Return.Status == "Approved" && ri.Return.Date.Date == today)
                .ToListAsync();

            // Build a lookup of returned quantities per ItemId
            var returnedQuantities = todayReturns
                .Where(ri => ri.Item != null)
                .GroupBy(ri => ri.ItemId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

            viewModel.TopSellingItems = todaySales
                .Where(si => si.Item != null)
                .GroupBy(si => new { ItemId = si.Item?.Id ?? 0, ItemName = si.Item?.Name ?? "Unknown Item" })
                .Select(g => {
                    var soldQty = g.Sum(si => si.Quantity);
                    var returnedQty = returnedQuantities.ContainsKey(g.Key.ItemId) ? returnedQuantities[g.Key.ItemId] : 0;
                    var netQty = soldQty - returnedQty;
                    if (netQty < 0) netQty = 0;
                    return new TopSellingItem { Name = g.Key.ItemName, Quantity = netQty };
                })
                .OrderByDescending(x => x.Quantity)
                .Take(5)
                .ToList();

            // Adjust total sales money by subtracting approved sales returns amounts for today
            var returnsTotalAmount = await _context.Returns
                .Where(r => r.Type == "Sales" && r.Status == "Approved" && r.Date.Date == today)
                .SumAsync(r => (decimal?)r.TotalAmount) ?? 0m;

            viewModel.TotalSales = Math.Max(0, viewModel.TotalSales - returnsTotalAmount);

            viewModel.Items = await _context.Items
                .Where(i => i.IsActive && i.Quantity > 0)
                .OrderBy(i => i.Name)
                .ToListAsync();

            var activeClients = await _context.Customers
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
            viewModel.Customers = activeClients;
            viewModel.Clients = activeClients;

            viewModel.RecentPurchases = await _context.PurchaseInvoices
                .Include(p => p.Supplier)
                .Include(p => p.CreatedByUser)
                .Include(p => p.Items)
                .OrderByDescending(p => p.Date)
                .Take(10)
                .ToListAsync();

            // Also load recent sales so cash/card/installment sales can be shown on the dashboard
            viewModel.RecentSales = await _context.SalesInvoices
                .Include(s => s.Customer)
                .Include(s => s.CreatedByUser)
                .Include(s => s.SalesInvoiceItems)
                    .ThenInclude(si => si.Item)
                .OrderByDescending(s => s.Date)
                .Take(10)
                .ToListAsync();
        }
        catch (Exception)
        {
            // Log the error here if you have a logging system
        }

        return View(viewModel);
    }
}