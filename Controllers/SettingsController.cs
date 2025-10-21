using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopInventory.Data;
using System.Threading.Tasks;

namespace ShopInventory.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> BackupDatabase()
        {
            // Implementation for database backup
            await Task.CompletedTask; // Placeholder for actual backup implementation
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ActivityLog()
        {
            var logs = await _context.ActivityLogs
                .Include(l => l.User)
                .OrderByDescending(l => l.Timestamp)
                .Take(1000)
                .ToListAsync();

            return View(logs);
        }

        public IActionResult EmailSettings()
        {
            // Email configuration settings
            return View();
        }

        public IActionResult SystemPreferences()
        {
            // System preferences like currency format, date format, etc.
            return View();
        }
    }
}