using System.ComponentModel.DataAnnotations;
using System.Text;
namespace EddaStore.Models.ViewModels.Identity;
public sealed class CompleteProfileViewModel
{
    [Required(ErrorMessage="رقم الموبايل مطلوب")][StringLength(30)][EgyptianPhoneNumber][Display(Name="رقم الموبايل")]
    public string PhoneNumber { get; set; } = string.Empty;
}
public sealed class EgyptianPhoneNumberAttribute : ValidationAttribute
{
    public static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var digits = new StringBuilder(value.Length);
        foreach (var c in value)
        {
            if (c is '+' or '-' or '(' or ')' || char.IsWhiteSpace(c)) continue;
            if (c is >= '0' and <= '9') digits.Append(c);
            else if (c is >= '٠' and <= '٩') digits.Append((char)('0' + c - '٠'));
            else if (c is >= '۰' and <= '۹') digits.Append((char)('0' + c - '۰'));
            else return null;
        }
        var number=digits.ToString();
        if(number.Length==11 && number.StartsWith("01",StringComparison.Ordinal) && number[2] is '0' or '1' or '2' or '5') return $"+20{number[1..]}";
        if(number.Length==12 && number.StartsWith("201",StringComparison.Ordinal) && number[3] is '0' or '1' or '2' or '5') return $"+{number}";
        if(number.Length==14 && number.StartsWith("00201",StringComparison.Ordinal) && number[5] is '0' or '1' or '2' or '5') return $"+20{number[4..]}";
        return null;
    }
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) => Normalize(value as string) is not null ? ValidationResult.Success : new ValidationResult("أدخل رقم موبايل مصري صحيحًا.");
}
