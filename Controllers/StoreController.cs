// using EddaStore.Data;
// using EddaStore.Models.Customers;
// using EddaStore.Models.Products;
// using EddaStore.Models.ViewModels.Store;
// using EddaStore.Services.Interfaces;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using System.Security.Claims;

// namespace EddaStore.Controllers;

// public class StoreController(IStoreCatalogService catalogService, ApplicationDbContext context) : Controller
// {
//     // عرض المنتجات مع البحث والتصفية حسب التصنيف.
//     public async Task<IActionResult> Index(
//         string? q,
//         string? category, string? brand, decimal? minPrice, decimal? maxPrice, string? sort, int page = 1, CancellationToken cancellationToken = default)
//     {
//         return View(await catalogService.GetAsync(new StoreIndexViewModel { Query=q,Category=category,Brand=brand,MinPrice=minPrice,MaxPrice=maxPrice,Sort=sort??"newest",Page=page }, cancellationToken));
//     }

//     // عرض تفاصيل المنتج باستخدام Slug.
//     public async Task<IActionResult> Details(string? slug, CancellationToken cancellationToken)
//     {
//         if (string.IsNullOrWhiteSpace(slug))
//         {
//             return RedirectToAction(nameof(Index));
//         }

//         var product = await catalogService.GetDetailsAsync(slug, cancellationToken);

//         if (product is null)
//         {
//             return NotFound();
//         }

//         ViewBag.Reviews = await context.Reviews.AsNoTracking()
//             .Where(r => r.ProductId == product.Id && r.IsApproved)
//             .OrderByDescending(r => r.Id)
//             .ToListAsync(cancellationToken);

//         return View(product);
//     }

//     [Authorize]
//     [HttpPost, ValidateAntiForgeryToken]
//     public async Task<IActionResult> SubmitReview(string slug, int rating, string? comment, CancellationToken cancellationToken)
//     {
//         if (rating < 1 || rating > 5)
//         {
//             TempData["Error"] = "التقييم يجب أن يكون بين 1 و 5.";
//             return RedirectToAction(nameof(Details), new { slug });
//         }

//         var product = await context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive, cancellationToken);
//         if (product is null)
//         {
//             return NotFound();
//         }

//         var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
//         context.Reviews.Add(new Review
//         {
//             CustomerId = userId,
//             ProductId = product.Id,
//             Rating = rating,
//             Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim(),
//             IsApproved = false
//         });
//         await context.SaveChangesAsync(cancellationToken);
//         TempData["Success"] = "تم إرسال مراجعتك وستظهر بعد اعتمادها.";
//         return RedirectToAction(nameof(Details), new { slug });
//     }
// }
using EddaStore.Filters;
using EddaStore.Data;
using EddaStore.Models.Customers;
using EddaStore.Models.Products;
using EddaStore.Models.ViewModels.Store;
using EddaStore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EddaStore.Controllers;

public class StoreController(IStoreCatalogService catalogService, ApplicationDbContext context) : Controller
{
    // عرض المنتجات مع البحث والتصفية حسب التصنيف.
    public async Task<IActionResult> Index(
        string? q,
        string? category, string? brand, decimal? minPrice, decimal? maxPrice, string? sort, int page = 1, CancellationToken cancellationToken = default)
    {
        return View(await catalogService.GetAsync(new StoreIndexViewModel { Query = q, Category = category, Brand = brand, MinPrice = minPrice, MaxPrice = maxPrice, Sort = sort ?? "newest", Page = page }, cancellationToken));
    }

    // عرض تفاصيل المنتج باستخدام Slug.
    public async Task<IActionResult> Details(string? slug, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return RedirectToAction(nameof(Index));
        }

        var product = await catalogService.GetDetailsAsync(slug, cancellationToken);

        if (product is null)
        {
            return NotFound();
        }

ViewData["CanonicalUrl"] = Url.Action(
            nameof(Details), "Store", new { slug = product.Slug }, Request.Scheme);

        ViewBag.Reviews = await context.Reviews.AsNoTracking()
            .Where(r => r.ProductId == product.Id && r.IsApproved)
            .OrderByDescending(r => r.Id)
            .ToListAsync(cancellationToken);

        return View(product);
    }

    [Authorize]
    [ServiceFilter(typeof(RequireCompleteProfileFilter))]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitReview(string slug, int rating, string? comment, CancellationToken cancellationToken)
    {
        if (rating < 1 || rating > 5)
        {
            TempData["Error"] = "التقييم يجب أن يكون بين 1 و 5.";
            return RedirectToAction(nameof(Details), new { slug });
        }

        var product = await context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive, cancellationToken);
        if (product is null)
        {
            return NotFound();
        }

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        context.Reviews.Add(new Review
        {
            CustomerId = userId,
            ProductId = product.Id,
            Rating = rating,
            Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim(),
            IsApproved = false
        });
        await context.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "تم إرسال مراجعتك وستظهر بعد اعتمادها.";
        return RedirectToAction(nameof(Details), new { slug });
    }
}
