using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopInventory.Data;

namespace ShopInventory.Controllers
{
    [ApiController]
    [Route("debug")]
    public class DebugController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public DebugController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("recent-sales")]
        public async Task<IActionResult> RecentSales()
        {
            var list = await _context.SalesInvoices
                .Include(s => s.Customer)
                .OrderByDescending(s => s.Date)
                .Take(20)
                .Select(s => new {
                    s.Id,
                    s.InvoiceNumber,
                    s.Date,
                    s.CustomerId,
                    CustomerName = s.Customer != null ? s.Customer.Name : null,
                    s.TotalAmount,
                    s.PaidAmount,
                    PaymentMethod = s.PaymentMethod.ToString()
                })
                .ToListAsync();

            return Ok(list);
        }
    }
}
