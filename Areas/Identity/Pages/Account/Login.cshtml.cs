using System.ComponentModel.DataAnnotations;
using EddaStore.Models.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

using Microsoft.AspNetCore.RateLimiting;

namespace EddaStore.Areas.Identity.Pages.Account;

[EnableRateLimiting("auth")]
public class LoginModel(
    SignInManager<ApplicationUser> signInManager,
    ILogger<LoginModel> logger) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public bool ShowGoogle =>
        HttpContext.RequestServices
            .GetRequiredService<IAuthenticationSchemeProvider>()
            .GetAllSchemesAsync()
            .GetAwaiter()
            .GetResult()
            .Any(x => x.Name == "Google");

    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required, EmailAddress, Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; } = "";

        [Required, DataType(DataType.Password), Display(Name = "كلمة المرور")]
        public string Password { get; set; } = "";

        [Display(Name = "تذكرني")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }

    public void OnGet(string? returnUrl = null)
    {
        Input.ReturnUrl = returnUrl;
        if (TempData.TryGetValue("ExternalLoginError", out var error) &&
            error is string message)
        {
            ErrorMessage = message;
            ModelState.AddModelError(string.Empty, message);
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await signInManager.PasswordSignInAsync(
            Input.Email, Input.Password, Input.RememberMe, true);

        if (result.Succeeded)
        {
            return LocalRedirect(SafeReturnUrl(Input.ReturnUrl));
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(
                string.Empty,
                "تم قفل الحساب مؤقتًا بسبب محاولات متكررة.");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "بيانات الدخول غير صحيحة.");
        }

        return Page();
    }

    public IActionResult OnPostExternalLogin(string provider, string? returnUrl)
    {
        var redirectUrl = Url.Page(
            "./ExternalLogin",
            pageHandler: "Callback",
            values: new { returnUrl });

        var properties = signInManager.ConfigureExternalAuthenticationProperties(
            provider, redirectUrl);

        logger.LogInformation(
            "Starting external login challenge for {Provider}.", provider);

        return new ChallengeResult(provider, properties);
    }

    private string SafeReturnUrl(string? url) =>
        Url.IsLocalUrl(url) ? url! : Url.Content("~/");
}