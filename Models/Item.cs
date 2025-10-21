using System.ComponentModel.DataAnnotations;

namespace ShopInventory.Models
{
    public class Item
    {
        public int Id { get; set; }

    [Required(ErrorMessage = "يرجى إدخال كود المنتج")]
    [MaxLength(50, ErrorMessage = "يجب ألا يزيد الكود عن 50 حرفًا")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "يرجى إدخال اسم المنتج")]
    [MaxLength(100, ErrorMessage = "يجب ألا يزيد اسم المنتج عن 100 حرف")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "يرجى إدخال الفئة")]
    [MaxLength(50, ErrorMessage = "يجب ألا تزيد الفئة عن 50 حرفًا")]
    public string Category { get; set; } = string.Empty;

    [Required(ErrorMessage = "يرجى إدخال الوحدة")]
    [MaxLength(20, ErrorMessage = "يجب ألا تزيد الوحدة عن 20 حرفًا")]
    public string Unit { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "يرجى إدخال كمية صحيحة (عدد صحيح فقط)")]
    [RegularExpression(@"^\d+$", ErrorMessage = "يرجى إدخال عدد صحيح بدون كسور")]
    public int Quantity { get; set; }

    [Required(ErrorMessage = "يرجى إدخال سعر الشراء")]
    [Range(0, double.MaxValue, ErrorMessage = "يرجى إدخال سعر شراء صحيح")]
    public decimal PurchasePrice { get; set; }

    [Required(ErrorMessage = "يرجى إدخال سعر البيع")]
    [Range(0, double.MaxValue, ErrorMessage = "يرجى إدخال سعر بيع صحيح")]
    public decimal SalePrice { get; set; }

    public int? SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public bool IsActive { get; set; } = true;

    [Range(0, int.MaxValue, ErrorMessage = "يرجى إدخال مستوى إعادة الطلب بشكل صحيح (عدد صحيح فقط)")]
    [RegularExpression(@"^\d+$", ErrorMessage = "يرجى إدخال عدد صحيح بدون كسور")]
    public int ReorderLevel { get; set; }

    [MaxLength(100, ErrorMessage = "يجب ألا يزيد الباركود عن 100 حرف")]
    public string? Barcode { get; set; }
    }
}