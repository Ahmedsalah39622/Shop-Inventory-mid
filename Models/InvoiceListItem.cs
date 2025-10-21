namespace ShopInventory.Models
{
    public class InvoiceListItem
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string PartyName { get; set; } = string.Empty; // Supplier or Customer
        public int ItemsCount { get; set; }
        public decimal TotalAmount { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public bool IsPurchase { get; set; }
        public int? Id { get; set; }
    }
}
