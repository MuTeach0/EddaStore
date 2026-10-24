using System.Security.Claims;
using EddaStore.Models.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace EddaStore.Areas.Identity.Pages.Account;

public class ExternalLoginModel(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    ILogger<ExternalLoginModel> logger) : PageModel
{
    public IActionResult OnGet() => RedirectToPage("./Login");

    public IActionResult OnPost(string provider, string? returnUrl = null)
    {
        var redirectUrl = Url.Page(
            "./ExternalLogin",
            pageHandler: "Callback",
            values: new { returnUrl });

        var properties = signInManager.ConfigureExternalAuthenticationProperties(
            provider, redirectUrl);

        return new ChallengeResult(provider, properties);
    }

    public async Task<IActionResult> OnGetCallbackAsync(
        string? returnUrl = null, string? remoteError = null)
    {
        returnUrl = (Url.IsLocalUrl(returnUrl) ? returnUrl : null)
            ?? Url.Content("~/");

        if (!string.IsNullOrWhiteSpace(remoteError))
        {
            logger.LogWarning(
                "External provider returned an error: {Error}.", remoteError);

            TempData["ExternalLoginError"] =
                "تم إلغاء تسجيل الدخول أو رفض الوصول من Google.";
            return RedirectToPage("./Login", new { returnUrl });
        }

        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info is null)
        {
            logger.LogWarning("GetExternalLoginInfoAsync returned null.");
            TempData["ExternalLoginError"] =
                "تعذر تحميل معلومات تسجيل الدخول externals.";
            return RedirectToPage("./Login", new { returnUrl });
        }

        var signInResult = await signInManager.ExternalLoginSignInAsync(
            info.LoginProvider, info.ProviderKey, isPersistent: false,
            bypassTwoFactor: true);

        if (signInResult.Succeeded)
        {
            logger.LogInformation(
                "User signed in using {Provider}.", info.LoginProvider);
            return LocalRedirect(returnUrl);
        }

        if (signInResult.IsLockedOut)
        {
            TempData["ExternalLoginError"] =
                "تم قفل الحساب مؤقتًا بسبب محاولات متكررة.";
            return RedirectToPage("./Login", new { returnUrl });
        }

        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(email))
        {
            TempData["ExternalLoginError"] =
                "Google لم يوفر بريدًا إلكترونيًا.";
            return RedirectToPage("./Login", new { returnUrl });
        }

        // Google provides "email_verified" in its OpenID Connect userinfo
        // response. The claim may be stored under a few possible names
        // depending on the endpoint version, so check them all.
        var emailVerified = info.Principal.FindFirstValue("email_verified")
            ?? info.Principal.FindFirstValue("verified_email");

        if (!IsEmailVerified(emailVerified))
        {
            logger.LogWarning(
                "Google login rejected: email_verified claim was '{Value}'. Available claims: {Claims}.",
                emailVerified ?? "<null>",
                string.Join(", ", info.Principal.Claims.Select(c => c.Type)));

            TempData["ExternalLoginError"] =
                "البريد الإلكتروني من Google غير مؤكد.";
            return RedirectToPage("./Login", new { returnUrl });
        }

        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser is not null)
        {
            logger.LogInformation(
                "Local account already exists for {Email}; not auto-linking.",
                email);
            TempData["ExternalLoginError"] =
                "يوجد حساب محلي بالبريد نفسه. سجّل الدخول بكلمة المرور أولًا.";
            return RedirectToPage("./Login", new { returnUrl });
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(user);
        if (!createResult.Succeeded)
        {
            logger.LogError(
                "Failed to create user from {Provider}: {Errors}.",
                info.LoginProvider,
                string.Join("; ", createResult.Errors.Select(e => e.Description)));

            TempData["ExternalLoginError"] = "تعذر إنشاء الحساب.";
            return RedirectToPage("./Login", new { returnUrl });
        }

        var addLoginResult = await userManager.AddLoginAsync(user, info);
        if (!addLoginResult.Succeeded)
        {
            logger.LogError(
                "Failed to add {Provider} login: {Errors}.",
                info.LoginProvider,
                string.Join("; ", addLoginResult.Errors.Select(e => e.Description)));

            await userManager.DeleteAsync(user);

            TempData["ExternalLoginError"] = "تعذر ربط حساب Google.";
            return RedirectToPage("./Login", new { returnUrl });
        }

        await signInManager.SignInAsync(user, isPersistent: false);
        logger.LogInformation(
            "Created and signed in new user via {Provider}.", info.LoginProvider);
        return LocalRedirect(returnUrl);
    }

    private static bool IsEmailVerified(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return value.Trim() switch
        {
            "true" or "True" or "TRUE" or "1" => true,
            _ => bool.TryParse(value, out var parsed) && parsed
        };
    }
}
