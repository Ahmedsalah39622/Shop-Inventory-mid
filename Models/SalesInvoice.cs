using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShopInventory.Models
{
    public class SalesInvoice
    {
    // For dashboard display: client name
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? ClientName { get; set; }
        public int Id { get; set; }

    [Required(ErrorMessage = "يرجى إدخال رقم الفاتورة")]
    [MaxLength(50, ErrorMessage = "يجب ألا يزيد رقم الفاتورة عن 50 حرف")]
    public string InvoiceNumber { get; set; } = string.Empty;

        public DateTime Date { get; set; }
        public int CustomerId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal Balance => TotalAmount - PaidAmount;

    // New fields
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public InvoiceType InvoiceType { get; set; } = InvoiceType.Retail;

    [MaxLength(500, ErrorMessage = "يجب ألا تزيد الملاحظات عن 500 حرف")]
    public string? Notes { get; set; }
        public string CreatedByUserId { get; set; } = string.Empty;

        // Navigation properties
        public virtual Customer Customer { get; set; } = null!;
        public virtual ApplicationUser CreatedByUser { get; set; } = null!;
        public virtual ICollection<SalesInvoiceItem> SalesInvoiceItems { get; set; } = new List<SalesInvoiceItem>();
    }
}