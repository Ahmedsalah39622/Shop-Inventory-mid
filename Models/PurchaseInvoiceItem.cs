namespace ShopInventory.Models
{
    public class PurchaseInvoiceItem
    {
        public int Id { get; set; }
        public int PurchaseInvoiceId { get; set; }
        public PurchaseInvoice? PurchaseInvoice { get; set; }
        public int ItemId { get; set; }
        public Item? Item { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total => Quantity * UnitPrice;
        public string? ItemName { get; set; }
        public string? ProductCode { get; set; }
        public string? Status { get; set; }
        public int CreatedByUserId { get; set; }
        public string? CreatedByUserName { get; set; }
        public DateTime Date { get; set; }
        public string? InvoiceNumber { get; set; }
        public string? ClientName { get; set; }
    }
}