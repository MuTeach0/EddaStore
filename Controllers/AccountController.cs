using System.Security.Claims;
using System.Text.Encodings.Web;
using EddaStore.Models.Customers;
using EddaStore.Models.Identity;
using EddaStore.Models.ViewModels.Identity;
using EddaStore.Models.ViewModels.Store;
using EddaStore.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace EddaStore.Controllers;

public class AccountController(
    IAccountService accountService,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IConfiguration configuration,
    IEmailSender emailSender,
    ILogger<AccountController> logger,
    IWebHostEnvironment environment) : Controller
{
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct) => View(await accountService.GetOverviewAsync(UserId(), User.Identity?.Name ?? string.Empty, ct));

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken ct)
    {
        var order = await accountService.GetOrderAsync(id, UserId(), ct);
        return order is null ? NotFound() : View(order);
    }

    [Authorize]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAddress([Bind("FullName,PhoneNumber,Country,City,StateOrGovernorate,StreetAddress,Building,Apartment,PostalCode,IsDefault")] Address address, CancellationToken ct)
    {
        if (!ModelState.IsValid) { TempData["Error"] = "راجع بيانات العنوان وحاول مرة أخرى."; return RedirectToAction(nameof(Index)); }
        await accountService.AddAddressAsync(UserId(), address, ct);
        TempData["Success"] = "تم حفظ العنوان.";
        return RedirectToAction(nameof(Index));
    }

    [AllowAnonymous, HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken, EnableRateLimiting("auth")]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = new ApplicationUser { UserName = model.Email.Trim(), Email = model.Email.Trim(), FirstName = model.FirstName.Trim(), LastName = model.LastName.Trim(), IsActive = true };
        var result = await userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded) { AddIdentityErrors(result); return View(model); }
        var roleResult = await userManager.AddToRoleAsync(user, "Customer");
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            logger.LogError("Could not assign Customer role to newly registered user {UserId}.", user.Id);
            ModelState.AddModelError(string.Empty, "تعذر إنشاء الحساب حاليًا. حاول مرة أخرى.");
            return View(model);
        }
        await signInManager.SignInAsync(user, isPersistent: false);
        logger.LogInformation("Customer account {UserId} was registered.", user.Id);
        return RedirectToAction(nameof(CompleteProfile));
    }

    [AllowAnonymous, HttpGet]
    public IActionResult Login(string? returnUrl = null, bool googleError = false)
    {
        ViewBag.GoogleEnabled = IsGoogleConfigured();
        if (googleError) TempData["ErrorMessage"] = "تعذر تسجيل الدخول باستخدام Google. حاول مرة أخرى.";
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken, EnableRateLimiting("auth")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        ViewBag.GoogleEnabled = IsGoogleConfigured();
        if (!ModelState.IsValid) return View(model);
        var user = await userManager.FindByEmailAsync(model.Email.Trim());
        if (user is null || !user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "البريد الإلكتروني أو كلمة المرور غير صحيحة.");
            return View(model);
        }
        var result = await signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            logger.LogInformation("User {UserId} signed in.", user.Id);
            return await RedirectAfterSignInAsync(user, model.ReturnUrl);
        }
        logger.LogWarning("Login failed for user {UserId}; locked out={LockedOut}.", user.Id, result.IsLockedOut);
        ModelState.AddModelError(string.Empty, result.IsLockedOut ? "تم إيقاف تسجيل الدخول مؤقتًا بعد محاولات غير ناجحة. حاول لاحقًا." : "البريد الإلكتروني أو كلمة المرور غير صحيحة.");
        return View(model);
    }

    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken, EnableRateLimiting("auth")]
    public IActionResult ExternalLogin(string provider, string? returnUrl = null)
    {
        if (!string.Equals(provider, "Google", StringComparison.Ordinal) || !IsGoogleConfigured())
        {
            TempData["ErrorMessage"] = "تسجيل الدخول باستخدام Google غير متاح حاليًا.";
            return RedirectToAction(nameof(Login), new { returnUrl });
        }
        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), values: new { returnUrl });
        var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl!);
        return Challenge(properties, provider);
    }

    [AllowAnonymous, HttpGet]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
    {
        if (!string.IsNullOrWhiteSpace(remoteError)) return ExternalLoginFailed(returnUrl, "تعذر تسجيل الدخول باستخدام Google. حاول مرة أخرى.");
        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info is null) return ExternalLoginFailed(returnUrl, "تعذر التحقق من تسجيل الدخول باستخدام Google. حاول مرة أخرى.");

        var signInResult = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
        if (signInResult.Succeeded)
        {
            var linked = await userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (linked is null || !linked.IsActive)
            {
                await signInManager.SignOutAsync();
                return ExternalLoginFailed(returnUrl, "تعذر تسجيل الدخول بهذا الحساب.");
            }
            return await RedirectAfterSignInAsync(linked, returnUrl);
        }

        if (signInResult.IsLockedOut || signInResult.IsNotAllowed || signInResult.RequiresTwoFactor)
            return ExternalLoginFailed(returnUrl, "تعذر تسجيل الدخول بهذا الحساب.");

        var email = info.Principal.FindFirstValue(ClaimTypes.Email) ?? info.Principal.FindFirstValue("email");
        var verified = info.Principal.FindFirstValue("email_verified") ?? info.Principal.FindFirstValue("verified_email");
        if (string.IsNullOrWhiteSpace(email) || !string.Equals(verified, "true", StringComparison.OrdinalIgnoreCase))
            return ExternalLoginFailed(returnUrl, "لم يتمكن Google من تأكيد بريدك الإلكتروني. استخدم حسابًا موثّقًا وحاول مجددًا.");
        if (await userManager.FindByEmailAsync(email) is not null)
            return ExternalLoginFailed(returnUrl, "يوجد حساب بهذا البريد بالفعل. سجّل الدخول بالطريقة الأصلية؛ لا نربط الحسابات تلقائيًا.");

        var givenName = info.Principal.FindFirstValue(ClaimTypes.GivenName) ?? info.Principal.FindFirstValue("given_name");
        var familyName = info.Principal.FindFirstValue(ClaimTypes.Surname) ?? info.Principal.FindFirstValue("family_name");
        var displayParts = (info.Principal.FindFirstValue("name") ?? string.Empty).Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true, FirstName = string.IsNullOrWhiteSpace(givenName) ? displayParts.FirstOrDefault() ?? "عميل" : givenName, LastName = string.IsNullOrWhiteSpace(familyName) ? displayParts.Skip(1).FirstOrDefault() ?? string.Empty : familyName, IsActive = true };
        var create = await userManager.CreateAsync(user);
        if (!create.Succeeded) return ExternalLoginFailed(returnUrl, "تعذر إنشاء الحساب. حاول مرة أخرى.");
        var role = await userManager.AddToRoleAsync(user, "Customer");
        if (!role.Succeeded) { await userManager.DeleteAsync(user); return ExternalLoginFailed(returnUrl, "تعذر إنشاء الحساب حاليًا. حاول مرة أخرى."); }
        var addLogin = await userManager.AddLoginAsync(user, info);
        if (!addLogin.Succeeded) { await userManager.DeleteAsync(user); return ExternalLoginFailed(returnUrl, "تعذر ربط تسجيل الدخول بالحساب. حاول مرة أخرى."); }
        await signInManager.SignInAsync(user, isPersistent: false);
        return RedirectToAction(nameof(CompleteProfile));
    }

    [AllowAnonymous, HttpGet]
    public IActionResult AccessDenied() { Response.StatusCode = StatusCodes.Status403Forbidden; return View(); }

    [AllowAnonymous, HttpGet]
    public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken, EnableRateLimiting("auth")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        const string safeMessage = "إذا كان البريد مسجلًا لدينا، فستصلك رسالة تحتوي على رابط استعادة كلمة المرور.";
        var user = await userManager.FindByEmailAsync(model.Email.Trim());
        if (user is null || !user.IsActive || await userManager.IsInRoleAsync(user, "Admin"))
        { TempData["PasswordResetMessage"] = safeMessage; return RedirectToAction(nameof(ForgotPasswordConfirmation)); }
        var baseUrl = configuration["App:PublicBaseUrl"];
        Uri? publicUri = Uri.TryCreate(baseUrl, UriKind.Absolute, out var configuredUri) && (configuredUri.Scheme == Uri.UriSchemeHttps || (EnvironmentIsDevelopment() && configuredUri.Scheme == Uri.UriSchemeHttp)) ? configuredUri : null;
        if (publicUri is null && Request.IsHttps) publicUri = new Uri($"{Request.Scheme}://{Request.Host}");
        if (publicUri is null) logger.LogWarning("Password reset email skipped because App:PublicBaseUrl must be a valid HTTPS URL.");
        else
        {
            try
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                var resetUrl = Url.Action(nameof(ResetPassword), "Account", new { email = user.Email, token }, publicUri.Scheme, publicUri.Authority);
                if (!string.IsNullOrWhiteSpace(resetUrl))
                {
                    var safeUrl = HtmlEncoder.Default.Encode(resetUrl);
                    var body = $"<p>وصلنا طلب لإعادة تعيين كلمة المرور لحسابك في عِدّة.</p><p><a href=\"{safeUrl}\">إعادة تعيين كلمة المرور</a></p><p>إذا لم تطلب ذلك، يمكنك تجاهل الرسالة.</p>";
                    await emailSender.SendEmailAsync(user.Email!, "إعادة تعيين كلمة المرور - عِدّة", body);
                }
            }
            catch (Exception ex) { logger.LogError(ex, "Password reset email could not be sent for user {UserId}.", user.Id); }
        }
        TempData["PasswordResetMessage"] = safeMessage;
        return RedirectToAction(nameof(ForgotPasswordConfirmation));
    }

    [AllowAnonymous, HttpGet]
    public IActionResult ForgotPasswordConfirmation() { ViewBag.Message = TempData["PasswordResetMessage"] ?? "إذا كان البريد مسجلًا لدينا، فستصلك رسالة تحتوي على رابط استعادة كلمة المرور."; return View(); }

    [AllowAnonymous, HttpGet]
    public IActionResult ResetPassword(string? email, string? token)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token)) { TempData["ErrorMessage"] = "رابط استعادة كلمة المرور غير صالح أو انتهت صلاحيته."; return RedirectToAction(nameof(ForgotPassword)); }
        return View(new ResetPasswordViewModel { Email = email, Token = token });
    }

    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken, EnableRateLimiting("auth")]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await userManager.FindByEmailAsync(model.Email);
        if (user is null || !user.IsActive || await userManager.IsInRoleAsync(user, "Admin"))
        { ModelState.AddModelError(string.Empty, "رابط الاستعادة غير صالح أو انتهت صلاحيته."); return View(model); }
        var result = await userManager.ResetPasswordAsync(user, model.Token, model.Password);
        if (!result.Succeeded) { ModelState.AddModelError(string.Empty, "تعذر تعيين كلمة المرور. تأكد من صلاحية الرابط ومن شروط كلمة المرور."); return View(model); }
        TempData["SuccessMessage"] = "تم تعيين كلمة المرور بنجاح. يمكنك تسجيل الدخول الآن.";
        return RedirectToAction(nameof(Login));
    }

    [Authorize, HttpGet]
    public async Task<IActionResult> CompleteProfile()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null || !user.IsActive) { await signInManager.SignOutAsync(); return RedirectToAction(nameof(Login)); }
        if (!string.IsNullOrWhiteSpace(user.PhoneNumber)) return RedirectToAction("Index", "Store");
        return View(new CompleteProfileViewModel());
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteProfile(CompleteProfileViewModel model, CancellationToken ct)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null || !user.IsActive) { await signInManager.SignOutAsync(); return RedirectToAction(nameof(Login)); }
        if (!ModelState.IsValid) return View(model);
        var phone = EgyptianPhoneNumberAttribute.Normalize(model.PhoneNumber);
        if (phone is null) { ModelState.AddModelError(nameof(model.PhoneNumber), "أدخل رقم موبايل مصري صحيحًا."); return View(model); }
        if (await userManager.Users.AnyAsync(x => x.Id != user.Id && x.PhoneNumber == phone, ct)) { ModelState.AddModelError(nameof(model.PhoneNumber), "رقم الهاتف مستخدم بالفعل."); return View(model); }
        user.PhoneNumber = phone;
        try
        {
            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded) { AddIdentityErrors(result); return View(model); }
        }
        catch (DbUpdateException ex) when (ex.GetBaseException() is SqlException { Number: 2601 or 2627 })
        {
            ModelState.AddModelError(nameof(model.PhoneNumber), "رقم الهاتف مستخدم بالفعل.");
            return View(model);
        }
        await signInManager.RefreshSignInAsync(user);
        return RedirectToAction("Index", "Store");
    }

    [Authorize, HttpGet]
    public async Task<IActionResult> Settings()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null || !user.IsActive) { await signInManager.SignOutAsync(); return RedirectToAction(nameof(Login)); }
        return View(new AccountSettingsViewModel { FirstName = user.FirstName, LastName = user.LastName, Email = user.Email ?? string.Empty });
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(AccountSettingsViewModel model)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null || !user.IsActive) { await signInManager.SignOutAsync(); return RedirectToAction(nameof(Login)); }
        if (string.IsNullOrWhiteSpace(model.FirstName) || string.IsNullOrWhiteSpace(model.LastName) || model.FirstName.Length > 50 || model.LastName.Length > 50)
        { TempData["Error"] = "يرجى إدخال الاسم الأول واسم العائلة بشكل صحيح."; return RedirectToAction(nameof(Settings)); }
        user.FirstName = model.FirstName.Trim(); user.LastName = model.LastName.Trim(); user.UpdatedAt = DateTimeOffset.UtcNow;
        var result = await userManager.UpdateAsync(user);
        if (result.Succeeded) { await signInManager.RefreshSignInAsync(user); TempData["Success"] = "تم تحديث البيانات الشخصية بنجاح."; }
        else { logger.LogWarning("Profile update failed for user {UserId}.", user.Id); TempData["Error"] = "تعذر تحديث البيانات. حاول مرة أخرى."; }
        return RedirectToAction(nameof(Settings));
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) { TempData["Error"] = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)); return RedirectToAction(nameof(Settings)); }
        var user = await userManager.GetUserAsync(User);
        if (user is null || !user.IsActive) { await signInManager.SignOutAsync(); return RedirectToAction(nameof(Login)); }
        var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded) { TempData["Error"] = result.Errors.Any(e => e.Code == "PasswordMismatch") ? "كلمة المرور الحالية غير صحيحة." : string.Join("، ", result.Errors.Select(e => e.Description)); return RedirectToAction(nameof(Settings)); }
        await signInManager.RefreshSignInAsync(user);
        TempData["Success"] = "تم تغيير كلمة المرور بنجاح.";
        return RedirectToAction(nameof(Settings));
    }

    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    private bool IsGoogleConfigured() => !string.IsNullOrWhiteSpace(configuration["Authentication:Google:ClientId"]) && !string.IsNullOrWhiteSpace(configuration["Authentication:Google:ClientSecret"]);
    private bool EnvironmentIsDevelopment() => environment.IsDevelopment();
    private IActionResult ExternalLoginFailed(string? returnUrl, string message) { logger.LogWarning("External Google authentication did not complete successfully."); TempData["ErrorMessage"] = message; return RedirectToAction(nameof(Login), new { returnUrl }); }
    private async Task<IActionResult> RedirectAfterSignInAsync(ApplicationUser user, string? returnUrl)
    {
        if (!user.IsActive) { await signInManager.SignOutAsync(); TempData["ErrorMessage"] = "تعذر تسجيل الدخول بهذا الحساب."; return RedirectToAction(nameof(Login)); }
        if (await userManager.IsInRoleAsync(user, "Admin")) return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : Url.Action("Index", "Dashboard", new { area = "Admin" })!);
        if (string.IsNullOrWhiteSpace(user.PhoneNumber)) return RedirectToAction(nameof(CompleteProfile));
        return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : Url.Action("Index", "Store")!);
    }
    private void AddIdentityErrors(IdentityResult result) { foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description); }
    private Guid UserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
