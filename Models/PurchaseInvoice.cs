using System.ComponentModel.DataAnnotations;

namespace ShopInventory.Models
{
    public class PurchaseInvoice
    {
        public int Id { get; set; }

    [Required(ErrorMessage = "يرجى إدخال رقم الفاتورة")]
    [MaxLength(50, ErrorMessage = "يجب ألا يزيد رقم الفاتورة عن 50 حرف")]
    public string InvoiceNumber { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public int SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal PaidAmount { get; set; }

        public decimal Balance => TotalAmount - PaidAmount;

    [MaxLength(500, ErrorMessage = "يجب ألا تزيد الملاحظات عن 500 حرف")]
    public string? Notes { get; set; }

        public string CreatedByUserId { get; set; } = string.Empty;
        public ApplicationUser? CreatedByUser { get; set; }

        public ICollection<PurchaseInvoiceItem> Items { get; set; } = new List<PurchaseInvoiceItem>();

        public string? ClientName { get; set; }
    }
}