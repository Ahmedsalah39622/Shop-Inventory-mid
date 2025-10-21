using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShopInventory.Data;
using ShopInventory.Models;
using System;
using System.Threading.Tasks;

namespace ShopInventory.Controllers
{
    [Authorize]
    public class SalesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SalesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(DateTime? startDate = null, DateTime? endDate = null, string? searchTerm = null)
        {
            var query = _context.SalesInvoices
                .Include(s => s.Customer)
                .Include(s => s.CreatedByUser)
                .Include(s => s.SalesInvoiceItems)
                .AsQueryable();

            // Apply date filters
            if (startDate.HasValue)
            {
                query = query.Where(s => s.Date.Date >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                query = query.Where(s => s.Date.Date <= endDate.Value.Date);
            }

            // Apply search term
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(s =>
                    (s.InvoiceNumber != null && s.InvoiceNumber.Contains(searchTerm)) ||
                    (s.Customer != null && s.Customer.Name != null && s.Customer.Name.Contains(searchTerm)) ||
                    (s.CreatedByUser != null && s.CreatedByUser.UserName != null && s.CreatedByUser.UserName.Contains(searchTerm))
                );
            }

            var sales = await query.OrderByDescending(s => s.Date).ToListAsync();

            var viewModel = new SalesListViewModel
            {
                Sales = sales,
                StartDate = startDate,
                EndDate = endDate,
                SearchTerm = searchTerm,
                TotalAmount = sales.Sum(s => s.TotalAmount),
                TotalSales = sales.Count
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Admin,Cashier")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Customers = new SelectList(await _context.Customers.Where(c => c.IsActive).ToListAsync(), "Id", "Name");
            ViewBag.Items = await _context.Items.Where(i => i.Quantity > 0).ToListAsync();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Cashier")]
        public async Task<IActionResult> Create(SalesInvoice sale)
        {
            if (ModelState.IsValid)
            {
                // Set the creation date
                sale.Date = DateTime.Now;
                
                // Set the created by user
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    throw new InvalidOperationException("User ID not found");
                }
                sale.CreatedByUserId = userId;

                // Generate invoice number (you might want to implement a more sophisticated system)
                var lastInvoice = await _context.SalesInvoices.OrderByDescending(s => s.Id).FirstOrDefaultAsync();
                int nextNumber = (lastInvoice?.Id ?? 0) + 1;
                sale.InvoiceNumber = $"INV{nextNumber:D5}";

                // Update stock for each item
                foreach (var item in sale.SalesInvoiceItems)
                {
                    var product = await _context.Items.FindAsync(item.ItemId);
                    if (product == null || product.Quantity < item.Quantity)
                    {
                        ModelState.AddModelError("", $"Insufficient stock for item {product?.Name ?? "Unknown"}");
                        ViewBag.Customers = new SelectList(await _context.Customers.Where(c => c.IsActive).ToListAsync(), "Id", "Name");
                        ViewBag.Items = await _context.Items.Where(i => i.Quantity > 0).ToListAsync();
                        return View(sale);
                    }

                    // Update stock
                        product.Quantity -= (int)item.Quantity;
                    _context.Update(product);

                    // Create stock movement record
                    var movement = new StockMovement
                    {
                        ProductId = item.ItemId,
                        MovementType = MovementType.Out,
                            Quantity = (int)item.Quantity,
                        Date = DateTime.Now,
                        Reference = sale.InvoiceNumber,
                        CreatedByUserId = userId
                    };
                    _context.StockMovements.Add(movement);
                }

                _context.Add(sale);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Details), new { id = sale.Id });
            }

            ViewBag.Customers = new SelectList(await _context.Customers.Where(c => c.IsActive).ToListAsync(), "Id", "Name");
            ViewBag.Items = await _context.Items.Where(i => i.Quantity > 0).ToListAsync();
            return View(sale);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var sale = await _context.SalesInvoices
                .Include(s => s.Customer)
                .Include(s => s.CreatedByUser)
                .Include(s => s.SalesInvoiceItems)
                    .ThenInclude(si => si.Item)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale == null)
                return NotFound();

            return View(sale);
        }

        [HttpGet]
        public async Task<IActionResult> Print(int id)
        {
            var sale = await _context.SalesInvoices
                .Include(s => s.Customer)
                .Include(s => s.SalesInvoiceItems)
                    .ThenInclude(si => si.Item)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale == null)
                return NotFound();

            return View(sale);
        }
    }
}