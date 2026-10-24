using EddaStore.Models.Identity;
using EddaStore.Models.ViewModels.Admin.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EddaStore.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class UsersController(UserManager<ApplicationUser> userManager) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct) =>
        View(await userManager.Users.OrderByDescending(x => x.CreatedAt).ToListAsync(ct));

    [HttpGet]
    public IActionResult Create() => View("Form", new UserEditViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserEditViewModel model, string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 10)
            ModelState.AddModelError("password", "كلمة المرور يجب ألا تقل عن 10 أحرف.");
        if (!ModelState.IsValid) return View("Form", model);

        if (!string.IsNullOrWhiteSpace(model.PhoneNumber) &&
            await userManager.Users.AnyAsync(x => x.PhoneNumber == model.PhoneNumber))
        {
            ModelState.AddModelError(nameof(model.PhoneNumber), "رقم الهاتف مستخدم بالفعل.");
            return View("Form", model);
        }
        var user = new ApplicationUser
        {
            UserName = model.Email.Trim(),
            Email = model.Email.Trim(),
            PhoneNumber = model.PhoneNumber,
            FirstName = model.Email.Split('@')[0],
            LastName = string.Empty,
            IsActive = true
        };
        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded) { AddErrors(result); return View("Form", model); }

        var roleName = model.IsAdmin ? "Admin" : "Customer";
        var roleResult = await userManager.AddToRoleAsync(user, roleName);
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            AddErrors(roleResult);
            return View("Form", model);
        }
        TempData["Success"] = "تم إنشاء المستخدم بنجاح.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null) return NotFound();
        return View("Form", new UserEditViewModel
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            IsAdmin = await userManager.IsInRoleAsync(user, "Admin")
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UserEditViewModel model)
    {
        if (id != model.Id) return BadRequest();
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null) return NotFound();
        if (!ModelState.IsValid) return View("Form", model);

        if (!string.IsNullOrWhiteSpace(model.PhoneNumber) &&
            await userManager.Users.AnyAsync(x => x.Id != user.Id && x.PhoneNumber == model.PhoneNumber))
        {
            ModelState.AddModelError(nameof(model.PhoneNumber), "رقم الهاتف مستخدم بالفعل.");
            return View("Form", model);
        }
        user.Email = model.Email.Trim();
        user.UserName = model.Email.Trim();
        user.PhoneNumber = model.PhoneNumber;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded) { AddErrors(result); return View("Form", model); }

        var isAdmin = await userManager.IsInRoleAsync(user, "Admin");
        if (!model.IsAdmin && isAdmin)
        {
            if (CurrentUserId() == user.Id.ToString())
            {
                ModelState.AddModelError(string.Empty, "لا يمكنك إزالة صلاحية المدير من حسابك.");
                return View("Form", model);
            }
            if ((await userManager.GetUsersInRoleAsync("Admin")).Count <= 1)
            {
                ModelState.AddModelError(string.Empty, "لا يمكن إزالة آخر مدير في النظام.");
                return View("Form", model);
            }
            await userManager.RemoveFromRoleAsync(user, "Admin");
            if (!await userManager.IsInRoleAsync(user, "Customer"))
                await userManager.AddToRoleAsync(user, "Customer");
        }
        else if (model.IsAdmin && !isAdmin)
        {
            var roleResult = await userManager.AddToRoleAsync(user, "Admin");
            if (!roleResult.Succeeded) { AddErrors(roleResult); return View("Form", model); }
        }

        TempData["Success"] = "تم تعديل المستخدم بنجاح.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (CurrentUserId() == id.ToString())
        {
            TempData["Error"] = "لا يمكنك حذف حسابك الحالي.";
            return RedirectToAction(nameof(Index));
        }
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null) return NotFound();
        if (await userManager.IsInRoleAsync(user, "Admin") &&
            (await userManager.GetUsersInRoleAsync("Admin")).Count <= 1)
        {
            TempData["Error"] = "لا يمكن حذف آخر مدير في النظام.";
            return RedirectToAction(nameof(Index));
        }
        var result = await userManager.DeleteAsync(user);
        if (!result.Succeeded) { AddErrors(result); TempData["Error"] = "تعذر حذف المستخدم."; }
        else TempData["Success"] = "تم حذف المستخدم بنجاح.";
        return RedirectToAction(nameof(Index));
    }

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
    }
    private string? CurrentUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}
