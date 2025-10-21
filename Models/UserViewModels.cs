using System.ComponentModel.DataAnnotations;

namespace ShopInventory.Models
{
    public class CreateUserViewModel
    {
    [Required(ErrorMessage = "يرجى إدخال الاسم الكامل")]
    [Display(Name = "الاسم الكامل")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "يرجى إدخال البريد الإلكتروني")]
    [EmailAddress(ErrorMessage = "يرجى إدخال بريد إلكتروني صحيح")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "يرجى إدخال كلمة المرور")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "يجب أن تكون كلمة المرور على الأقل 6 أحرف")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "كلمة المرور وتأكيد كلمة المرور غير متطابقتين")]
    [Display(Name = "تأكيد كلمة المرور")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Display(Name = "الدور")]
    public string? Role { get; set; }
    }

    public class EditUserViewModel
    {
    [Required(ErrorMessage = "يرجى إدخال المعرف")]
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "يرجى إدخال الاسم الكامل")]
    [Display(Name = "الاسم الكامل")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "يرجى إدخال البريد الإلكتروني")]
    [EmailAddress(ErrorMessage = "يرجى إدخال بريد إلكتروني صحيح")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "الدور")]
    public string? Role { get; set; }
    }
}