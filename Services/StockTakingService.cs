using ShopInventory.Data;
using ShopInventory.Models;
using Microsoft.EntityFrameworkCore;

namespace ShopInventory.Services
{
    public class StockTakingService
    {
        private readonly ApplicationDbContext _context;
        public StockTakingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> CalculateExpectedQtyAsync(int productId, DateTime from, DateTime to)
        {
            var openingBalance = await _context.StockMovements
                .Where(m => m.ProductId == productId && m.Date < from)
                .SumAsync(m => m.MovementType == MovementType.In ? m.Quantity : -m.Quantity);

            var purchases = await _context.StockMovements
                .Where(m => m.ProductId == productId && m.MovementType == MovementType.In && m.Date >= from && m.Date <= to)
                .SumAsync(m => m.Quantity);

            var sales = await _context.StockMovements
                .Where(m => m.ProductId == productId && m.MovementType == MovementType.Out && m.Date >= from && m.Date <= to)
                .SumAsync(m => m.Quantity);

            var returns = await _context.StockMovements
                .Where(m => m.ProductId == productId && m.MovementType == MovementType.Return && m.Date >= from && m.Date <= to)
                .SumAsync(m => m.Quantity);

            return openingBalance + purchases - sales + returns;
        }

        public decimal CalculateDifference(decimal expectedQty, decimal actualQty)
        {
            return actualQty - expectedQty;
        }
    }
}
