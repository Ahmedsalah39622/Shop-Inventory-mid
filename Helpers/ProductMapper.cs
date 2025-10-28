using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShopInventory.Data;
using ShopInventory.Models;

namespace ShopInventory.Helpers
{
    public static class ProductMapper
    {
        public static async Task<int> GetOrCreateProductIdForItem(ApplicationDbContext context, Item item)
        {
            if (item == null) return 0;

            // Try match by SKU/Code or Name
            var prod = await context.Products.FirstOrDefaultAsync(p => p.SKU == item.Code || p.Name == item.Name);
            if (prod != null) return prod.Id;

            // Create a new Product to map this Item
            var newProd = new Product
            {
                Name = item.Name ?? item.Code ?? "Item",
                SKU = item.Code ?? item.Id.ToString(),
                Unit = item.Unit ?? string.Empty,
                CurrentStock = item.Quantity
            };
            context.Products.Add(newProd);
            await context.SaveChangesAsync();
            return newProd.Id;
        }
    }
}
