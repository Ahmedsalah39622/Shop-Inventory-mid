using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopInventory.Data;
using ShopInventory.Services;
using ShopInventory.Models;
using System.Threading.Tasks;

namespace ShopInventory.Controllers
{
    [Authorize]
    public class ItemsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IInventoryService _inventoryService;

        public ItemsController(ApplicationDbContext context, IInventoryService inventoryService)
        {
            _context = context;
            _inventoryService = inventoryService;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _context.Items
                .Include(i => i.Supplier)
                .Where(i => i.IsActive)
                .Select(i => new ItemWithSalesCount {
                    Id = i.Id,
                    Code = i.Code,
                    Name = i.Name,
                    Category = i.Category,
                    Unit = i.Unit,
                    Quantity = i.Quantity,
                    PurchasePrice = i.PurchasePrice,
                    SalePrice = i.SalePrice,
                    SupplierId = i.SupplierId,
                    Supplier = i.Supplier,
                    ExpiryDate = i.ExpiryDate,
                    IsActive = i.IsActive,
                    ReorderLevel = i.ReorderLevel,
                    Barcode = i.Barcode,
                    PurchaseQuantity = (int)(_context.PurchaseInvoiceItems.Where(pii => pii.ItemId == i.Id).Sum(pii => (decimal?)pii.Quantity) ?? 0)
                })
                .ToListAsync();

            // Get top 10 best-selling items based on purchase quantity
            var bestSelling = items
                .OrderByDescending(x => x.PurchaseQuantity)
                .Take(10)
                .Select(x => new BestSellingItemViewModel {
                    Name = x.Name,
                    Quantity = x.PurchaseQuantity
                })
                .ToList();

            ViewBag.BestSelling = bestSelling;

            return View(items);
        }

        // [Authorize(Roles = "Admin,StoreKeeper")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Suppliers = await _context.Suppliers.ToListAsync();
            return View();
        }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // [Authorize(Roles = "Admin,StoreKeeper")]
        public async Task<IActionResult> Create(Item item)
        {
            if (ModelState.IsValid)
            {
                _context.Add(item);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Suppliers = await _context.Suppliers.ToListAsync();
            return View(item);
        }

    // [Authorize(Roles = "Admin,StoreKeeper")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var item = await _context.Items.FindAsync(id);
            if (item == null)
                return NotFound();

            ViewBag.Suppliers = await _context.Suppliers.ToListAsync();
            return View(item);
        }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // [Authorize(Roles = "Admin,StoreKeeper")]
        public async Task<IActionResult> Edit(int id, Item item)
        {
            if (id != item.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(item);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Items.AnyAsync(e => e.Id == id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Suppliers = await _context.Suppliers.ToListAsync();
            return View(item);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var item = await _context.Items
                .Include(i => i.Supplier)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (item == null)
                return NotFound();

            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item != null)
            {
                item.IsActive = false;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> LowStock()
        {
            var items = await _inventoryService.GetLowStockItemsAsync();
            return View("Index", items);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound("لم يتم العثور على المنتج.");

            var item = await _context.Items
                .Include(i => i.Supplier)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (item == null)
                return NotFound("لم يتم العثور على المنتج.");

            return View(item);
        }

        public async Task<IActionResult> Adjust(int? id)
        {
            if (id == null)
                return NotFound("لم يتم العثور على المنتج.");

            var item = await _context.Items
                .Include(i => i.Supplier)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (item == null)
                return NotFound("لم يتم العثور على المنتج.");

            return View(item);
        }

        public async Task<IActionResult> StockMovement(int? itemId)
        {
            if (itemId == null)
                return NotFound("لم يتم العثور على المنتج.");

            var item = await _context.Items.Include(i => i.Supplier).FirstOrDefaultAsync(i => i.Id == itemId);
            if (item == null)
                return NotFound("لم يتم العثور على المنتج.");

            var movements = await _context.StockMovements
                .Where(m => m.ProductId == itemId)
                .OrderByDescending(m => m.Date)
                .ToListAsync();

            ViewBag.Item = item;
            return View(movements);
        }

        public async Task<IActionResult> Expiring()
        {
            var items = await _inventoryService.GetExpiringItemsAsync();
            return View("Index", items);
        }
    }
}