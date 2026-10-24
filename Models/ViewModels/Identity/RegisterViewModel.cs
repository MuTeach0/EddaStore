using System.ComponentModel.DataAnnotations;
namespace EddaStore.Models.ViewModels.Identity;
public sealed class RegisterViewModel
{
    [Required(ErrorMessage = "الاسم الأول مطلوب")][StringLength(50)][Display(Name="الاسم الأول")]
    public string FirstName { get; set; } = string.Empty;
    [Required(ErrorMessage = "اسم العائلة مطلوب")][StringLength(50)][Display(Name="اسم العائلة")]
    public string LastName { get; set; } = string.Empty;
    [Required(ErrorMessage = "البريد الإلكتروني مطلوب")][EmailAddress(ErrorMessage="صيغة البريد الإلكتروني غير صحيحة")][Display(Name="البريد الإلكتروني")]
    public string Email { get; set; } = string.Empty;
    [Required(ErrorMessage="كلمة المرور مطلوبة")][StringLength(100, MinimumLength=10, ErrorMessage="كلمة المرور يجب ألا تقل عن 10 أحرف")][DataType(DataType.Password)][Display(Name="كلمة المرور")]
    public string Password { get; set; } = string.Empty;
    [Required(ErrorMessage="تأكيد كلمة المرور مطلوب")][DataType(DataType.Password)][Compare(nameof(Password), ErrorMessage="كلمتا المرور غير متطابقتين")][Display(Name="تأكيد كلمة المرور")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
