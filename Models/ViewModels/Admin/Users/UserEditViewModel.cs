using System.ComponentModel.DataAnnotations;

namespace EddaStore.Models.ViewModels.Admin.Users;

public class UserEditViewModel
{
    public Guid Id { get; set; }
    [Required, EmailAddress, Display(Name = "البريد الإلكتروني")]
    public string Email { get; set; } = string.Empty;
    [Display(Name = "رقم الهاتف")]
    public string? PhoneNumber { get; set; }
    [Display(Name = "مدير النظام")]
    public bool IsAdmin { get; set; }
}
