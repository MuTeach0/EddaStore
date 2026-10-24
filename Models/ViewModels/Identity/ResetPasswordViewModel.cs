using System.ComponentModel.DataAnnotations;
namespace EddaStore.Models.ViewModels.Identity;
public sealed class ResetPasswordViewModel
{
    [Required] public string Email { get; set; } = string.Empty;
    [Required] public string Token { get; set; } = string.Empty;
    [Required(ErrorMessage="كلمة المرور الجديدة مطلوبة")][StringLength(100, MinimumLength=10, ErrorMessage="كلمة المرور يجب ألا تقل عن 10 أحرف")][DataType(DataType.Password)][Display(Name="كلمة المرور الجديدة")]
    public string Password { get; set; } = string.Empty;
    [Required(ErrorMessage="تأكيد كلمة المرور مطلوب")][DataType(DataType.Password)][Compare(nameof(Password), ErrorMessage="كلمتا المرور غير متطابقتين")][Display(Name="تأكيد كلمة المرور")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
