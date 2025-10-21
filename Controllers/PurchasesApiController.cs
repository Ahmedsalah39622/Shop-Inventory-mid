using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopInventory.Data;
using ShopInventory.Models;

[Route("api/purchases")]
[ApiController]
public class PurchasesApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public PurchasesApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PurchaseDto dto)
    {
        if (dto == null || dto.Items == null || !dto.Items.Any())
            return BadRequest("No items provided.");

        // Use a valid user id or null for CreatedByUserId
        // Use a default valid user id if not authenticated
        string? userId = null;
        if (User?.Identity?.IsAuthenticated == true)
        {
            userId = User.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
        }
        if (string.IsNullOrEmpty(userId))
        {
            // Replace with a valid user id from your AspNetUsers table
            userId = "d5f13cc9-045a-47bd-a3f6-e008794858bb";
        }

        // Get the full name of the user
        string createdByUserName = "System";
        if (!string.IsNullOrEmpty(userId))
        {
            var appUser = !string.IsNullOrEmpty(userId) ? 
                await _context.Users.FirstOrDefaultAsync(u => u.Id == userId) : 
                null;
            if (appUser != null && !string.IsNullOrEmpty(appUser.FullName))
                createdByUserName = appUser.FullName;
            else if (appUser != null)
                createdByUserName = appUser.UserName ?? "Unknown";
        }

        // Use a valid supplier id or null
        var firstSupplier = _context.Suppliers.FirstOrDefault();
        if (firstSupplier == null)
        {
            return BadRequest("No supplier found. Please add a supplier before creating a purchase invoice.");
        }
        var invoice = new PurchaseInvoice
        {
            InvoiceNumber = $"PI-{DateTime.Now:yyyyMMddHHmmss}",
            Date = DateTime.Now,
            TotalAmount = dto.TotalAmount,
            PaidAmount = dto.AmountPaid,
            CreatedByUserId = userId,
            SupplierId = firstSupplier.Id,
            Items = new List<PurchaseInvoiceItem>()
        };

        foreach (var x in dto.Items)
        {
            var itemEntity = await _context.Items.FirstOrDefaultAsync(i => i.Id == x.ItemId);
            if (itemEntity != null)
            {
                // Decrease stock
                    itemEntity.Quantity -= (int)x.Quantity;
                if (itemEntity.Quantity < 0) itemEntity.Quantity = 0;
            }
            invoice.Items.Add(new PurchaseInvoiceItem
            {
                ItemId = x.ItemId,
                    Quantity = (int)x.Quantity,
                UnitPrice = x.UnitPrice,
                ItemName = itemEntity?.Name ?? "",
                ProductCode = itemEntity?.Code ?? "",
                Status = (dto.AmountPaid >= dto.TotalAmount) ? "Paid" : (dto.AmountPaid > 0 ? "Partial" : "Unpaid"),
                CreatedByUserId = int.TryParse(userId, out var uid) ? uid : 0,
                CreatedByUserName = createdByUserName,
                Date = DateTime.Now,
                InvoiceNumber = invoice.InvoiceNumber
            });
        }

        _context.PurchaseInvoices.Add(invoice);
        await _context.SaveChangesAsync();
        return Ok(new { invoice.Id });
    }
}

public class PurchaseDto
{
    public List<PurchaseItemDto> Items { get; set; } = new();
    public decimal AmountPaid { get; set; }
    public decimal TotalAmount { get; set; }
}

public class PurchaseItemDto
{
    public int ItemId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
