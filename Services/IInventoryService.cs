using ShopInventory.Models;

namespace ShopInventory.Services
{
    public interface IInventoryService
    {
        Task<bool> UpdateStockAsync(int itemId, decimal quantity, MovementType movementType, string reference, string userId);
        Task<bool> ProcessPurchaseInvoiceAsync(PurchaseInvoice invoice);
        Task<bool> ProcessSalesInvoiceAsync(SalesInvoice invoice);
        Task<decimal> GetCurrentStockAsync(int itemId);
        Task<IEnumerable<Item>> GetLowStockItemsAsync();
        Task<IEnumerable<Item>> GetExpiringItemsAsync(int daysThreshold = 30);
    }
}