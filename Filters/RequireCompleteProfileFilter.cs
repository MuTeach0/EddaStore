using EddaStore.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
namespace EddaStore.Filters;
public sealed class RequireCompleteProfileFilter(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var principal = context.HttpContext.User;
        if (principal.Identity?.IsAuthenticated != true || principal.IsInRole("Admin")) return;
        var user = await userManager.GetUserAsync(principal);
        if (user is null || !user.IsActive) { await signInManager.SignOutAsync(); context.Result = new ChallengeResult(); return; }
        if (string.IsNullOrWhiteSpace(user.PhoneNumber)) context.Result = new RedirectToActionResult("CompleteProfile", "Account", new { area = "" });
    }
}
