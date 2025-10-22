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
        // Supports optional GET parameters: search (string) and days (int)
        public IActionResult Index(string? search, int? days)
        {
            var query = _context.Installments
                .Include(i => i.Customer)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                // try to allow searching by customer name or by exact installment id when numeric
                if (int.TryParse(s, out var id))
                {
                    query = query.Where(i => i.Id == id || (i.Customer != null && EF.Functions.Like(i.Customer.Name, $"%{s}%")));
                }
                else
                {
                    query = query.Where(i => i.Customer != null && EF.Functions.Like(i.Customer.Name, $"%{s}%"));
                }
            }

            if (days.HasValue)
            {
                var cutoff = DateTime.Today.AddDays(days.Value);
                query = query.Where(i => i.NextPaymentDate.HasValue && i.NextPaymentDate.Value.Date <= cutoff.Date);
            }

            var list = query
                .OrderByDescending(i => i.CreatedAt)
                .ToList();

            ViewData["Search"] = search ?? string.Empty;
            ViewData["Days"] = days;

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
