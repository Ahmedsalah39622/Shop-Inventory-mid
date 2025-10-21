using System.ComponentModel.DataAnnotations;

namespace ShopInventory.Models
{
    public class Supplier
    {
        public int Id { get; set; }

    [Required(ErrorMessage = "يرجى إدخال اسم المورد")]
    [MaxLength(100, ErrorMessage = "يجب ألا يزيد اسم المورد عن 100 حرف")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200, ErrorMessage = "يجب ألا يزيد العنوان عن 200 حرف")]
    public string? Address { get; set; }

    [MaxLength(50, ErrorMessage = "يجب ألا يزيد رقم الهاتف عن 50 حرف")]
    public string? Phone { get; set; }

    [MaxLength(100, ErrorMessage = "يجب ألا يزيد البريد الإلكتروني عن 100 حرف")]
    public string? Email { get; set; }

        public decimal Balance { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<Item> Items { get; set; } = new List<Item>();
        public ICollection<PurchaseInvoice> PurchaseInvoices { get; set; } = new List<PurchaseInvoice>();
    }
}