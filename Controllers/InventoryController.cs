using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopInventory.Data;
using ShopInventory.Models;
using ShopInventory.Services;
using System.Threading.Tasks;
using System.Linq;

namespace ShopInventory.Controllers
{
    [Authorize]
    public class InventoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IInventoryService _inventoryService;

        public InventoryController(ApplicationDbContext context, IInventoryService inventoryService)
        {
            _context = context;
            _inventoryService = inventoryService;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _context.Items
                .Include(i => i.Supplier)
                .OrderBy(i => i.Name)
                .ToListAsync();
            return View(items);
        }

        public async Task<IActionResult> LowStock()
        {
            var items = await _inventoryService.GetLowStockItemsAsync();
            ViewBag.Title = "Low Stock Items";
            return View("Index", items);
        }

        public async Task<IActionResult> StockMovement(int? itemId)
        {
            IQueryable<StockMovement> query = _context.StockMovements
                .Include(sm => sm.Product)
                .Include(sm => sm.CreatedByUser)
                .OrderByDescending(sm => sm.Date);

            if (itemId.HasValue)
            {
                // Map Item.Id to Product.Id
                var item = await _context.Items.FindAsync(itemId.Value);
                if (item != null)
                {
                    var prod = await _context.Products.FirstOrDefaultAsync(p => p.SKU == item.Code || p.Name == item.Name);
                    var productId = prod != null ? prod.Id : await ShopInventory.Helpers.ProductMapper.GetOrCreateProductIdForItem(_context, item);
                    if (productId != 0)
                        query = query.Where(sm => sm.ProductId == productId);
                    else
                        query = query.Where(sm => false); // no movements if product missing
                }
                else
                {
                    query = query.Where(sm => false);
                }
            }

            var movements = await query.Take(100).ToListAsync();
            return View(movements);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,StoreKeeper")]
        public async Task<IActionResult> Adjust(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null)
                return NotFound();

            return View(item);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,StoreKeeper")]
        public async Task<IActionResult> Adjust(int id, decimal quantity, string reason)
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null)
                return NotFound();

            var userName = User.Identity?.Name ?? "";
            await _inventoryService.UpdateStockAsync(id, Math.Abs(quantity), quantity >= 0 ? MovementType.In : MovementType.Out, $"Adjustment: {reason}", userName);
            return RedirectToAction(nameof(Index));
        }
    }
}