using System.ComponentModel.DataAnnotations;

namespace ShopInventory.Models
{
    public class LoginViewModel
    {
    [Required(ErrorMessage = "يرجى إدخال البريد الإلكتروني")]
    [EmailAddress(ErrorMessage = "يرجى إدخال بريد إلكتروني صحيح")]
    public required string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "يرجى إدخال كلمة المرور")]
    [DataType(DataType.Password)]
    public required string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }
}