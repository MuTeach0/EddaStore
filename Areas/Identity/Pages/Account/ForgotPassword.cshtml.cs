using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Text;
using System.Text.Encodings.Web;
using EddaStore.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

using Microsoft.AspNetCore.RateLimiting;

namespace EddaStore.Areas.Identity.Pages.Account;

[EnableRateLimiting("auth")]
public class ForgotPasswordModel(
    UserManager<ApplicationUser> userManager,
    Microsoft.AspNetCore.Identity.UI.Services.IEmailSender emailSender,
    ILogger<ForgotPasswordModel> logger,
    IWebHostEnvironment environment) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required, EmailAddress, Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await userManager.FindByEmailAsync(Input.Email);
        if (user is null || string.IsNullOrWhiteSpace(user.Email))
        {
            logger.LogInformation(
                "Password reset requested for an email that is not registered or not eligible.");

            return RedirectToPage("ForgotPasswordConfirmation");
        }

        try
        {
            logger.LogInformation(
                "Starting password reset email flow for user {UserId}.", user.Id);

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { area = "Identity", code, email = user.Email },
                protocol: Request.Scheme);

            await emailSender.SendEmailAsync(
                user.Email!,
                "استعادة كلمة المرور - عِدّة",
                $"لإعادة تعيين كلمة المرور، <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}'>اضغط هنا</a>.");

            logger.LogInformation(
                "Password reset email dispatched for user {UserId}.", user.Id);
        }
        catch (Exception ex)
        {
            var smtpStatus = ex is SmtpException smtpEx
                ? smtpEx.StatusCode.ToString()
                : "none";

            logger.LogError(
                ex,
                "Failed to send password reset email for user {UserId}. Exception: {ExceptionType}. SmtpStatusCode: {SmtpStatusCode}.",
                user.Id,
                ex.GetType().FullName,
                smtpStatus);

            if (environment.IsDevelopment())
            {
                ModelState.AddModelError(
                    string.Empty,
                    "تعذر إرسال البريد الإلكتروني. تحقق من إعدادات EmailSettings في User Secrets.");
                return Page();
            }
        }

        return RedirectToPage("ForgotPasswordConfirmation");
    }
}
