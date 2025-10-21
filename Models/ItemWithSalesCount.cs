namespace ShopInventory.Models
{
    public class ItemWithSalesCount : Item
    {
        public int PurchaseQuantity { get; set; }
        public int SalesQuantity { get; set; }
    }
}