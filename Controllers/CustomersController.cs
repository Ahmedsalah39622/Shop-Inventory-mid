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
    public class CustomersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var customers = await _context.Customers
                .OrderBy(c => c.Name)
                .ToListAsync();
            return View(customers);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Cashier")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Cashier")]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (ModelState.IsValid)
            {
                customer.IsActive = true;
                _context.Add(customer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        public async Task<IActionResult> Details(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.SalesInvoices)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
                return NotFound();

            return View(customer);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
                return NotFound();

            return View(customer);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Customer customer)
        {
            if (id != customer.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(customer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }
    }
}