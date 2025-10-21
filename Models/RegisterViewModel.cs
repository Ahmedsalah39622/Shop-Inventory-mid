using System.ComponentModel.DataAnnotations;

namespace ShopInventory.Models
{
    public class RegisterViewModel
    {
    [Required(ErrorMessage = "يرجى إدخال الاسم الكامل")]
    [Display(Name = "الاسم الكامل")]
    public required string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "يرجى إدخال البريد الإلكتروني")]
    [EmailAddress(ErrorMessage = "يرجى إدخال بريد إلكتروني صحيح")]
    [Display(Name = "البريد الإلكتروني")]
    public required string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "يرجى إدخال كلمة المرور")]
    [StringLength(100, ErrorMessage = "يجب أن تكون كلمة المرور على الأقل 6 أحرف", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "كلمة المرور")]
    public required string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "تأكيد كلمة المرور")]
    [Compare("Password", ErrorMessage = "كلمة المرور وتأكيد كلمة المرور غير متطابقتين")]
    public required string ConfirmPassword { get; set; } = string.Empty;
    }
}