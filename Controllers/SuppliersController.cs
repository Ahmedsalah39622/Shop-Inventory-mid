using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopInventory.Data;
using ShopInventory.Models;
using System.Threading.Tasks;
using System.Linq;

namespace ShopInventory.Controllers
{
    [Authorize]
    public class SuppliersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SuppliersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var suppliers = await _context.Suppliers
                .OrderBy(s => s.Name)
                .ToListAsync();
            return View(suppliers);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,StoreKeeper")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,StoreKeeper")]
        public async Task<IActionResult> Create(Supplier supplier)
        {
            if (ModelState.IsValid)
            {
                supplier.IsActive = true;
                _context.Add(supplier);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(supplier);
        }

        public async Task<IActionResult> Details(int id)
        {
            var supplier = await _context.Suppliers
                .Include(s => s.Items)
                .Include(s => s.PurchaseInvoices)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (supplier == null)
                return NotFound();

            return View(supplier);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
                return NotFound();

            return View(supplier);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Supplier supplier)
        {
            if (id != supplier.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(supplier);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(supplier);
        }
    }
}