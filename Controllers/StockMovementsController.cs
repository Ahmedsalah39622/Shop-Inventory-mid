using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopInventory.Data;
using ShopInventory.Models;
using System.Threading.Tasks;

namespace ShopInventory.Controllers
{
    [Authorize]
    public class StockMovementsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public StockMovementsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var movements = await _context.StockMovements
                .Include(m => m.Product)
                .Include(m => m.CreatedByUser)
                .OrderByDescending(m => m.Date)
                .ToListAsync();
            return View(movements);
        }
    }
}
