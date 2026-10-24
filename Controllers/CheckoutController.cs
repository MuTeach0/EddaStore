// using System.Security.Claims;
// using EddaStore.Models.ViewModels.Store;
// using EddaStore.Services.Interfaces;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;

// namespace EddaStore.Controllers;

// [Authorize]
// public class CheckoutController(ICartService cartService, ICheckoutService checkoutService) : Controller
// {
//     public async Task<IActionResult> Index(CancellationToken ct)
//     {
//         var cart = await cartService.GetAsync(UserId(), ct);
//         return cart.Items.Any() ? View(new CheckoutPageViewModel { Cart = cart }) : RedirectToAction("Index", "Cart");
//     }

//     [HttpPost, ValidateAntiForgeryToken]
//     public async Task<IActionResult> Index(CheckoutInputViewModel input, CancellationToken ct)
//     {
//         var cart = await cartService.GetAsync(UserId(), ct);
//         if (!ModelState.IsValid) return View(new CheckoutPageViewModel { Input = input, Cart = cart });
//         try { return RedirectToAction(nameof(Confirmation), new { id = await checkoutService.CreateOrderAsync(UserId(), input, ct) }); }
//         catch (InvalidOperationException ex) { ModelState.AddModelError(string.Empty, ex.Message); }
//         return View(new CheckoutPageViewModel { Input = input, Cart = cart });
//     }

//     public IActionResult Confirmation(Guid id) => RedirectToAction("Details", "Account", new { id });
//     private Guid UserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
// }
using EddaStore.Filters;
using System.Security.Claims;
using EddaStore.Models.ViewModels.Store;
using EddaStore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EddaStore.Controllers;

[Authorize]
[ServiceFilter(typeof(RequireCompleteProfileFilter))]
public class CheckoutController(ICartService cartService, ICheckoutService checkoutService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var cart = await cartService.GetAsync(UserId(), ct);
        return cart.Items.Any() ? View(new CheckoutPageViewModel { Cart = cart }) : RedirectToAction("Index", "Cart");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CheckoutInputViewModel input, CancellationToken ct)
    {
        var cart = await cartService.GetAsync(UserId(), ct);
        if (!ModelState.IsValid) return View(new CheckoutPageViewModel { Input = input, Cart = cart });
        try { return RedirectToAction(nameof(Confirmation), new { id = await checkoutService.CreateOrderAsync(UserId(), input, ct) }); }
        catch (InvalidOperationException ex) { ModelState.AddModelError(string.Empty, ex.Message); }
        return View(new CheckoutPageViewModel { Input = input, Cart = cart });
    }

    public IActionResult Confirmation(Guid id) => RedirectToAction("Details", "Account", new { id });
    private Guid UserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
