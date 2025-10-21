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

            // Sales: sum PaidAmount (collected money) so down payments are counted instead of full total
            viewModel.TotalSales = await _context.SalesInvoices
                .Where(s => s.Date.Date == today)
                .SumAsync(s => (decimal?)s.PaidAmount) ?? 0m;

            viewModel.TotalOrders = await _context.PurchaseInvoices
                .Where(p => p.Date.Date == today)
                .CountAsync();

            viewModel.LowStockItems = await _context.Items
                .Where(i => i.Quantity <= i.ReorderLevel && i.IsActive)
                .CountAsync();

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

            viewModel.TopSellingItems = todaySales
                .Where(si => si.Item != null)
                .GroupBy(si => new { ItemId = si.Item?.Id ?? 0, ItemName = si.Item?.Name ?? "Unknown Item" })
                .Select(g => new TopSellingItem 
                { 
                    Name = g.Key.ItemName,
                    Quantity = g.Sum(si => si.Quantity)
                })
                .OrderByDescending(x => x.Quantity)
                .Take(5)
                .ToList();

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