using System.ComponentModel.DataAnnotations;
namespace EddaStore.Models.ViewModels.Identity;
public sealed class ChangePasswordViewModel
{
    [Required(ErrorMessage="كلمة المرور الحالية مطلوبة")][DataType(DataType.Password)][Display(Name="كلمة المرور الحالية")] public string CurrentPassword { get; set; } = string.Empty;
    [Required(ErrorMessage="كلمة المرور الجديدة مطلوبة")][StringLength(100,MinimumLength=10,ErrorMessage="كلمة المرور يجب ألا تقل عن 10 أحرف")][DataType(DataType.Password)][Display(Name="كلمة المرور الجديدة")] public string NewPassword { get; set; } = string.Empty;
    [Required(ErrorMessage="تأكيد كلمة المرور مطلوب")][DataType(DataType.Password)][Compare(nameof(NewPassword),ErrorMessage="كلمتا المرور غير متطابقتين")][Display(Name="تأكيد كلمة المرور الجديدة")] public string ConfirmPassword { get; set; } = string.Empty;
}
