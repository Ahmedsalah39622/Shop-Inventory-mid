using Microsoft.EntityFrameworkCore;
using ShopInventory.Data;
using ShopInventory.Models;

namespace ShopInventory.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly ApplicationDbContext _context;

        public InventoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> UpdateStockAsync(int itemId, decimal quantity, MovementType movementType, string reference, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Map the incoming itemId (Item.Id) to the Products.Id that StockMovements use
                var item = await _context.Items.FindAsync(itemId);
                if (item == null)
                    return false;

                var productId = await ShopInventory.Helpers.ProductMapper.GetOrCreateProductIdForItem(_context, item);
                if (productId == 0)
                    return false;

                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                    return false;

                // Update product quantity
                switch (movementType)
                {
                    case MovementType.In:
                        product.CurrentStock += quantity;
                        break;
                    case MovementType.Out:
                        if (product.CurrentStock < quantity)
                            throw new InvalidOperationException("Insufficient stock");
                        product.CurrentStock -= quantity;
                        break;
                    case MovementType.Return:
                        product.CurrentStock += quantity;
                        break;
                }

                // Create stock movement record using the mapped product id
                var movement = new StockMovement
                {
                    ProductId = productId,
                    Quantity = quantity,
                    MovementType = movementType,
                    Date = DateTime.UtcNow,
                    Reference = reference,
                    CreatedByUserId = userId
                };

                _context.StockMovements.Add(movement);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> ProcessPurchaseInvoiceAsync(PurchaseInvoice invoice)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in invoice.Items)
                {
                    await UpdateStockAsync(
                        item.ItemId,
                        item.Quantity,
                        MovementType.In,
                        $"Purchase Invoice #{invoice.InvoiceNumber}",
                        invoice.CreatedByUserId);
                }

                // Update supplier balance
                var supplier = await _context.Suppliers.FindAsync(invoice.SupplierId);
                if (supplier != null)
                {
                    supplier.Balance += invoice.Balance;
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> ProcessSalesInvoiceAsync(SalesInvoice invoice)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in invoice.SalesInvoiceItems)
                {
                    await UpdateStockAsync(
                        item.ItemId,
                        item.Quantity,
                        MovementType.Out,
                        $"Sales Invoice #{invoice.InvoiceNumber}",
                        invoice.CreatedByUserId);
                }

                // Update customer balance
                var customer = await _context.Customers.FindAsync(invoice.CustomerId);
                if (customer != null)
                {
                    customer.Balance += invoice.Balance;
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<decimal> GetCurrentStockAsync(int itemId)
        {
            var item = await _context.Items.FindAsync(itemId);
            return item?.Quantity ?? 0;
        }

        public async Task<IEnumerable<Item>> GetLowStockItemsAsync()
        {
            return await _context.Items
                .Where(i => i.Quantity <= i.ReorderLevel && i.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<Item>> GetExpiringItemsAsync(int daysThreshold = 30)
        {
            var thresholdDate = DateTime.UtcNow.AddDays(daysThreshold);
            return await _context.Items
                .Where(i => i.ExpiryDate.HasValue && 
                          i.ExpiryDate.Value <= thresholdDate && 
                          i.IsActive && 
                          i.Quantity > 0)
                .ToListAsync();
        }
    }
}