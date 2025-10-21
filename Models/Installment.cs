using System;
using System.ComponentModel.DataAnnotations;

namespace ShopInventory.Models
{
    public class Installment
    {
        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }

    // Navigation to customer for display
    public virtual Customer? Customer { get; set; }

        // Link to the sales invoice (optional)
        public int? SalesInvoiceId { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        public decimal DownPayment { get; set; }

        public decimal RemainingAmount { get; set; }

        public int NumberOfMonths { get; set; }

        public decimal MonthlyAmount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? NextPaymentDate { get; set; }

        public string? Status { get; set; }

        // نوع التقسيط: مثلا "بنكي" او "كمبيالات"
        public string? InstallmentType { get; set; }
    }
}
