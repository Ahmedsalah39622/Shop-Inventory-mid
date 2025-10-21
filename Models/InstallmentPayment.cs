using System;
using System.ComponentModel.DataAnnotations;

namespace ShopInventory.Models
{
    public class InstallmentPayment
    {
        public int Id { get; set; }

        [Required]
        public int InstallmentId { get; set; }
        public Installment? Installment { get; set; }

        [Required]
        public decimal Amount { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        public string? PaymentMethod { get; set; }

        public string? Notes { get; set; }
    }
}
