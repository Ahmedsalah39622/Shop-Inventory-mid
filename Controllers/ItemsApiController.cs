using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopInventory.Data;
using System.Threading.Tasks;

namespace ShopInventory.Controllers
{
    [Route("api/items")]
    [ApiController]
    public class ItemsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ItemsApiController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpGet("active-count")]
        public async Task<IActionResult> GetActiveItemsCount()
        {
            var count = await _context.Items.CountAsync(i => i.IsActive);
            return Ok(count);
        }

        [HttpGet("top-selling")]
        public async Task<IActionResult> GetTopSellingItems()
        {
            // Get all items in inventory, with their total quantity sold (zero if unsold)
            var items = await _context.Items
                .Where(i => i.IsActive)
                .OrderByDescending(i => i.Id)
                .Select(i => new {
                    ItemId = i.Id,
                    Name = i.Name,
                    SalesCount = _context.SalesInvoiceItems.Count(sii => sii.ItemId == i.Id)
                })
                .ToListAsync();
            return Ok(items);
        }

            [HttpGet("search")]
            public async Task<IActionResult> SearchItems([FromQuery] string q)
            {
                if (string.IsNullOrWhiteSpace(q)) return Ok(new List<object>());
                var items = await _context.Items
                    .Where(i => i.IsActive && (i.Name.Contains(q) || i.Code.Contains(q)))
                    .Select(i => new {
                        i.Id,
                        i.Name,
                        i.Code,
                        i.Quantity,
                        i.SalePrice
                    })
                    .Take(10)
                    .ToListAsync();
                return Ok(items);
            }
    }
}
