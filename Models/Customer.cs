using System.ComponentModel.DataAnnotations;

namespace ShopInventory.Models
{
    public class Customer
    {
        public int Id { get; set; }

    [Required(ErrorMessage = "يرجى إدخال اسم العميل")]
    [MaxLength(100, ErrorMessage = "يجب ألا يزيد اسم العميل عن 100 حرف")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200, ErrorMessage = "يجب ألا يزيد العنوان عن 200 حرف")]
    public string? Address { get; set; }

    [MaxLength(50, ErrorMessage = "يجب ألا يزيد رقم الهاتف عن 50 حرف")]
    public string? Phone { get; set; }

    [MaxLength(100, ErrorMessage = "يجب ألا يزيد البريد الإلكتروني عن 100 حرف")]
    public string? Email { get; set; }

        public decimal Balance { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<SalesInvoice> SalesInvoices { get; set; } = new List<SalesInvoice>();
    }
}