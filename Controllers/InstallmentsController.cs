using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ShopInventory.Controllers
{
    public class InstallmentsController : Controller
    {
        private readonly Data.ApplicationDbContext _context;

        public InstallmentsController(Data.ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Installments
        public IActionResult Index()
        {
            var list = _context.Installments
                .Include(i => i.Customer)
                .OrderByDescending(i => i.CreatedAt)
                .ToList();
            return View(list);
        }

        // GET: /Installments/ping
        [HttpGet("/Installments/ping")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Ping()
        {
            return Content("ok");
        }
    }
}
