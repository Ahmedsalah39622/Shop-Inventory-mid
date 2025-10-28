using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopInventory.Data;
using ShopInventory.Models;
using ShopInventory.Helpers;

namespace ShopInventory.Controllers;

[Authorize]
public class ReturnsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ReturnsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Returns
    public async Task<IActionResult> Index()
    {
        var returns = await _context.Returns
            .Include(r => r.Customer)
            .Include(r => r.Supplier)
            .Include(r => r.CreatedByUser)
            .Include(r => r.Items)
                .ThenInclude(i => i.Item)
            .OrderByDescending(r => r.Date)
            .ToListAsync();

        return View(returns);
    }

    // GET: Returns/Create
    // Optional parameters: saleId (prefill from sales invoice) or invoiceNumber (prefill from purchase) or itemId (prefill single item)
    public async Task<IActionResult> Create(int? saleId, string? invoiceNumber, int? itemId)
    {
        ViewBag.Customers = _context.Customers.Where(c => c.IsActive).OrderBy(c => c.Name).ToList();
        ViewBag.Suppliers = _context.Suppliers.Where(s => s.IsActive).OrderBy(s => s.Name).ToList();
        ViewBag.Items = _context.Items.Where(i => i.IsActive).OrderBy(i => i.Name).ToList();

        var model = new Return
        {
            Date = DateTime.Now,
            Status = "Pending",
            Type = "Sales"
        };

        if (saleId.HasValue)
        {
            var sale = await _context.SalesInvoices
                .Include(s => s.SalesInvoiceItems)
                    .ThenInclude(si => si.Item)
                .FirstOrDefaultAsync(s => s.Id == saleId.Value);

            if (sale != null)
            {
                model.Type = "Sales";
                model.OriginalInvoiceId = sale.Id;
                model.CustomerId = sale.CustomerId;
                model.Items = sale.SalesInvoiceItems.Select(si => new ReturnItem
                {
                    ItemId = si.ItemId,
                    Quantity = Convert.ToInt32(si.Quantity),
                    UnitPrice = si.UnitPrice,
                    ReturnReason = ""
                }).ToList();
                model.TotalAmount = model.Items.Sum(i => i.Quantity * i.UnitPrice);
            }
        }

        if (!string.IsNullOrEmpty(invoiceNumber))
        {
            var purchase = await _context.PurchaseInvoices
                .Include(p => p.Items)
                    .ThenInclude(pi => pi.Item)
                .FirstOrDefaultAsync(p => p.InvoiceNumber == invoiceNumber);

            if (purchase != null)
            {
                model.Type = "Purchase";
                model.OriginalInvoiceId = purchase.Id;
                model.SupplierId = purchase.SupplierId;
                model.Items = purchase.Items.Select(pi => new ReturnItem
                {
                    ItemId = pi.ItemId,
                    Quantity = Convert.ToInt32(pi.Quantity),
                    UnitPrice = pi.UnitPrice,
                    ReturnReason = ""
                }).ToList();
                model.TotalAmount = model.Items.Sum(i => i.Quantity * i.UnitPrice);
            }
        }

        if (itemId.HasValue)
        {
            var item = await _context.Items.FindAsync(itemId.Value);
            if (item != null)
            {
                // if no items already from sale/purchase, add this
                if (model.Items == null || model.Items.Count == 0)
                {
                    model.Items = new List<ReturnItem>
                    {
                        new ReturnItem
                        {
                            ItemId = item.Id,
                            Quantity = 1,
                            UnitPrice = item.SalePrice,
                            ReturnReason = string.Empty
                        }
                    };
                    model.TotalAmount = model.Items.Sum(i => i.Quantity * i.UnitPrice);
                }
            }
        }

        return View(model);
    }

    // POST: Returns/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create()
    {
        // Parse form data manually to avoid any model binding issues from AJAX/FormData
        var form = await Request.ReadFormAsync();

        var returnModel = new Return();
        returnModel.Type = form["Type"].FirstOrDefault() ?? "Sales";
        if (int.TryParse(form["CustomerId"].FirstOrDefault(), out var custId))
            returnModel.CustomerId = custId;
        if (int.TryParse(form["SupplierId"].FirstOrDefault(), out var suppId))
            returnModel.SupplierId = suppId;
        if (int.TryParse(form["OriginalInvoiceId"].FirstOrDefault(), out var origId))
            returnModel.OriginalInvoiceId = origId;
        returnModel.Notes = form["Notes"].FirstOrDefault();

        // Parse items Items[0].ItemId, Items[0].Quantity, Items[0].UnitPrice, Items[0].ReturnReason
        var items = new List<ReturnItem>();
        for (int i = 0; ; i++)
        {
            var key = $"Items[{i}].ItemId";
            if (!form.ContainsKey(key)) break;

            if (int.TryParse(form[$"Items[{i}].ItemId"].FirstOrDefault(), out var itemId) &&
                int.TryParse(form[$"Items[{i}].Quantity"].FirstOrDefault(), out var qty) &&
                decimal.TryParse(form[$"Items[{i}].UnitPrice"].FirstOrDefault(), out var price))
            {
                var reason = form[$"Items[{i}].ReturnReason"].FirstOrDefault() ?? string.Empty;
                items.Add(new ReturnItem
                {
                    ItemId = itemId,
                    Quantity = qty,
                    UnitPrice = price,
                    ReturnReason = reason
                });
            }
            else
            {
                // skip malformed row
            }
        }
        returnModel.Items = items;

        if (returnModel == null)
        {
            if (Request.Headers.ContainsKey("X-Requested-With") && Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return BadRequest("Invalid return data");
            ModelState.AddModelError(string.Empty, "Invalid return data");
            ViewBag.Customers = _context.Customers.Where(c => c.IsActive).OrderBy(c => c.Name).ToList();
            ViewBag.Suppliers = _context.Suppliers.Where(s => s.IsActive).OrderBy(s => s.Name).ToList();
            ViewBag.Items = _context.Items.Where(i => i.IsActive).OrderBy(i => i.Name).ToList();
            return View(new Return());
        }

        if (ModelState.IsValid)
        {
            var user = await _userManager.GetUserAsync(User);
            returnModel.CreatedById = user?.Id ?? string.Empty;
            returnModel.Date = DateTime.Now;

            // First save the return so it gets an Id
            _context.Returns.Add(returnModel);
            await _context.SaveChangesAsync();

            // The return is saved, but we do NOT apply stock movements or change item quantities here.
            // Movements and quantity updates will be applied when the return status is changed to "Approved"
            // (see UpdateStatus action).
            await _context.SaveChangesAsync();

            // If AJAX request, return 200 OK for client script to redirect
            if (Request.Headers.ContainsKey("X-Requested-With") && Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Ok();
            }

            return RedirectToAction(nameof(Index));
        }

        // If we got here, something failed, redisplay form
        ViewBag.Customers = _context.Customers.Where(c => c.IsActive).OrderBy(c => c.Name).ToList();
        ViewBag.Suppliers = _context.Suppliers.Where(s => s.IsActive).OrderBy(s => s.Name).ToList();
        ViewBag.Items = _context.Items.Where(i => i.IsActive).OrderBy(i => i.Name).ToList();
        return View(returnModel);
    }

    // GET: Returns/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var returnModel = await _context.Returns
            .Include(r => r.Customer)
            .Include(r => r.Supplier)
            .Include(r => r.CreatedByUser)
            .Include(r => r.Items)
                .ThenInclude(i => i.Item)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (returnModel == null)
        {
            return NotFound();
        }

        return View(returnModel);
    }

    // POST: Returns/UpdateStatus/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var returnModel = await _context.Returns
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (returnModel == null)
        {
            return NotFound();
        }

        returnModel.Status = status;
        var user = await _userManager.GetUserAsync(User);

        if (status == "Approved")
        {
            // Create stock movements for approved returns
            foreach (var item in returnModel.Items)
            {
                var stockItem = await _context.Items.FindAsync(item.ItemId);
                var productId2 = 0;
                if (stockItem != null)
                {
                    productId2 = await ProductMapper.GetOrCreateProductIdForItem(_context, stockItem);
                }

                var movement = new StockMovement
                {
                    Date = DateTime.Now,
                    ProductId = productId2,
                    Quantity = returnModel.Type == "Sales" ? item.Quantity : -item.Quantity,
                    MovementType = MovementType.Return,
                    Reference = $"Return #{returnModel.Id} Approved",
                    CreatedByUserId = user?.Id ?? ""
                };
                _context.StockMovements.Add(movement);

                // Update item quantity
                if (stockItem != null)
                {
                    stockItem.Quantity += Convert.ToInt32(movement.Quantity);
                }
            }
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }
}