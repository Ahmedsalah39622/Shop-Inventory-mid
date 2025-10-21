using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopInventory.Data;
using ShopInventory.Models;
using ShopInventory.Services;

namespace ShopInventory.Controllers
{
    [Authorize(Roles = "Admin,StoreKeeper")]
    public class DailyStockTakingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly StockTakingService _service;
        public DailyStockTakingController(ApplicationDbContext context)
        {
            _context = context;
            _service = new StockTakingService(context);
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var products = await _context.Products.ToListAsync();
            var viewModel = new List<StockTaking>();
            foreach (var product in products)
            {
                var expectedQty = await _service.CalculateExpectedQtyAsync(product.Id, today, today);
                viewModel.Add(new StockTaking
                {
                    ProductId = product.Id,
                    Product = product,
                    ExpectedQty = expectedQty,
                    Type = StockTakingType.Daily,
                    Date = today
                });
            }
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Save(List<StockTaking> takings)
        {
            foreach (var taking in takings)
            {
                taking.Difference = _service.CalculateDifference(taking.ExpectedQty, taking.ActualQty);
                _context.StockTakings.Add(taking);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
