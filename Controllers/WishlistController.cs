// using System.Security.Claims;
// using EddaStore.Data;
// using EddaStore.Models.Customers;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;

// namespace EddaStore.Controllers;

// [Authorize]
// public class WishlistController(ApplicationDbContext context) : Controller
// {
//     public async Task<IActionResult> Index(CancellationToken ct) => View(await context.WishlistItems.Where(x => x.CustomerId == UserId()).Include(x => x.Product).ThenInclude(x => x.Images).OrderByDescending(x => x.AddedAt).ToListAsync(ct));
//     [HttpPost, ValidateAntiForgeryToken]
//     public async Task<IActionResult> Add(Guid productId, string? returnUrl, CancellationToken ct)
//     {
//         var customerId = UserId();
//         if (!await context.Products.AnyAsync(x => x.Id == productId && x.IsActive, ct)) return NotFound();
//         if (!await context.WishlistItems.AnyAsync(x => x.CustomerId == customerId && x.ProductId == productId, ct)) { context.WishlistItems.Add(new WishlistItem { CustomerId = customerId, ProductId = productId }); await context.SaveChangesAsync(ct); TempData["Success"] = "تمت إضافة المنتج إلى المفضلة."; }
//         return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction(nameof(Index));
//     }
//     [HttpPost, ValidateAntiForgeryToken]
//     public async Task<IActionResult> Remove(Guid productId, CancellationToken ct)
//     { var item = await context.WishlistItems.FirstOrDefaultAsync(x => x.CustomerId == UserId() && x.ProductId == productId, ct); if (item is not null) { context.WishlistItems.Remove(item); await context.SaveChangesAsync(ct); } return RedirectToAction(nameof(Index)); }
//     private Guid UserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
// }
using EddaStore.Filters;
using System.Security.Claims;
using EddaStore.Data;
using EddaStore.Models.Customers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EddaStore.Controllers;

[Authorize]
[ServiceFilter(typeof(RequireCompleteProfileFilter))]
public class WishlistController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct) => View(await context.WishlistItems.Where(x => x.CustomerId == UserId()).Include(x => x.Product).ThenInclude(x => x.Images).OrderByDescending(x => x.AddedAt).ToListAsync(ct));
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(Guid productId, string? returnUrl, CancellationToken ct)
    {
        var customerId = UserId();
        if (!await context.Products.AnyAsync(x => x.Id == productId && x.IsActive, ct)) return NotFound();
        if (!await context.WishlistItems.AnyAsync(x => x.CustomerId == customerId && x.ProductId == productId, ct)) { context.WishlistItems.Add(new WishlistItem { CustomerId = customerId, ProductId = productId }); await context.SaveChangesAsync(ct); TempData["Success"] = "تمت إضافة المنتج إلى المفضلة."; }
        return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction(nameof(Index));
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(Guid productId, CancellationToken ct)
    { var item = await context.WishlistItems.FirstOrDefaultAsync(x => x.CustomerId == UserId() && x.ProductId == productId, ct); if (item is not null) { context.WishlistItems.Remove(item); await context.SaveChangesAsync(ct); } return RedirectToAction(nameof(Index)); }
    private Guid UserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
