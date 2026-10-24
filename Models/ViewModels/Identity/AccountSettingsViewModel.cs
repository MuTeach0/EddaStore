using System.ComponentModel.DataAnnotations;
namespace EddaStore.Models.ViewModels.Identity;
public sealed class AccountSettingsViewModel
{
    [Required(ErrorMessage="الاسم الأول مطلوب")][StringLength(50)][Display(Name="الاسم الأول")] public string FirstName { get; set; } = string.Empty;
    [Required(ErrorMessage="اسم العائلة مطلوب")][StringLength(50)][Display(Name="اسم العائلة")] public string LastName { get; set; } = string.Empty;
    [Display(Name="البريد الإلكتروني")] public string Email { get; set; } = string.Empty;
}
