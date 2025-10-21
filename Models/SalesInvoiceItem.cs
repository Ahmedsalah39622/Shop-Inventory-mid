namespace ShopInventory.Models
{
    public class SalesInvoiceItem
    {
        public int Id { get; set; }

        public int SalesInvoiceId { get; set; }
        public SalesInvoice? SalesInvoice { get; set; }

        public int ItemId { get; set; }
        public Item? Item { get; set; }

        public decimal Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Total => Quantity * UnitPrice;
    }
}