using System.ComponentModel.DataAnnotations;
namespace EddaStore.Models.ViewModels.Identity;
public sealed class ForgotPasswordViewModel
{
    [Required(ErrorMessage="من فضلك أدخل البريد الإلكتروني")][EmailAddress(ErrorMessage="صيغة البريد الإلكتروني غير صحيحة")][Display(Name="البريد الإلكتروني")]
    public string Email { get; set; } = string.Empty;
}
