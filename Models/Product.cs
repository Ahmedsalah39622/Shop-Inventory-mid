using System.ComponentModel.DataAnnotations;

namespace ShopInventory.Models
{
    public class Product
    {
        public int Id { get; set; }
    [Required(ErrorMessage = "يرجى إدخال اسم المنتج")]
    public string Name { get; set; } = string.Empty;
    [Required(ErrorMessage = "يرجى إدخال رمز المنتج")]
    public string SKU { get; set; } = string.Empty;
    [Required(ErrorMessage = "يرجى إدخال الوحدة")]
    public string Unit { get; set; } = string.Empty;
        public decimal CurrentStock { get; set; }
    }
}
