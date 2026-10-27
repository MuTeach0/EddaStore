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
using EddaStore.Data;
using EddaStore.Filters;
using System.Security.Claims;
using EddaStore.Models.Orders;
using EddaStore.Models.ViewModels.Store;
using EddaStore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EddaStore.Controllers;

[Authorize]
[ServiceFilter(typeof(RequireCompleteProfileFilter))]
public class CheckoutController(ICartService cartService, ICheckoutService checkoutService, ApplicationDbContext context, IConfiguration configuration) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var cart = await cartService.GetAsync(UserId(), ct);
        if (!cart.Items.Any()) return RedirectToAction("Index", "Cart");
        var addresses = await context.Addresses.AsNoTracking()
            .Where(x => x.CustomerId == UserId())
            .OrderByDescending(x => x.IsDefault)
            .ThenByDescending(x => x.Id)
            .ToListAsync(ct);
        var selected = addresses.FirstOrDefault(x => x.IsDefault) ?? addresses.FirstOrDefault();

        var input = new CheckoutInputViewModel { SelectedAddressId = selected?.Id };
        if (selected is not null)
        {
            input.RecipientName = selected.FullName;
            input.PhoneNumber = selected.PhoneNumber;
            input.City = selected.City;
            input.StreetAddress = selected.StreetAddress;
            input.Building = selected.Building;
            input.Apartment = selected.Apartment;
            input.PostalCode = selected.PostalCode;
        }

        return View(new CheckoutPageViewModel { Input = input, Cart = cart, SavedAddresses = addresses });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CheckoutInputViewModel input, CancellationToken ct)
    {
        var cart = await cartService.GetAsync(UserId(), ct);
        var addresses = await context.Addresses.AsNoTracking()
            .Where(x => x.CustomerId == UserId())
            .OrderByDescending(x => x.IsDefault)
            .ThenByDescending(x => x.Id)
            .ToListAsync(ct);

        if (input.SelectedAddressId.HasValue)
        {
            var selectedAddress = await context.Addresses.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == input.SelectedAddressId.Value && x.CustomerId == UserId(), ct);
            if (selectedAddress is null)
            {
                ModelState.AddModelError("Input.SelectedAddressId", "العنوان المحدد غير متاح.");
            }
            else
            {
                foreach (var key in new[] { "Input.RecipientName", "Input.PhoneNumber", "Input.City", "Input.StreetAddress" })
                    ModelState.Remove(key);
            }
        }

        if (!ModelState.IsValid) return View(new CheckoutPageViewModel { Input = input, Cart = cart, SavedAddresses = addresses });
        try
        {
            var orderId = await checkoutService.CreateOrderAsync(UserId(), input, ct);
            var order = await context.Orders.AsNoTracking()
                .Include(x => x.Items)
                .Include(x => x.ShippingAddress)
                .FirstOrDefaultAsync(x => x.Id == orderId, ct);
            if (order is null) return RedirectToAction("Details", "Account", new { id = orderId });

            var whatsAppNumber = configuration["Store:WhatsAppNumber"]?.Trim().Replace("+", string.Empty).Replace(" ", string.Empty);
            if (string.IsNullOrWhiteSpace(whatsAppNumber))
            {
                TempData["Error"] = "تم حفظ الطلب. لم يتم العثور على رقم واتساب المتجر في الإعدادات.";
                return RedirectToAction("Details", "Account", new { id = orderId });
            }

            var address = order.ShippingAddress;
            var productLines = order.Items.Select(item =>
                $"- {item.ProductName} × {item.Quantity} @ {item.UnitPrice:N2} = {item.LineTotal:N2} ج.م");
            var optionalAddressLines = new List<string>();
            if (!string.IsNullOrWhiteSpace(address?.StateOrGovernorate))
                optionalAddressLines.Add($"المحافظة: {address.StateOrGovernorate}");
            if (!string.IsNullOrWhiteSpace(address?.Building))
                optionalAddressLines.Add($"رقم المبنى: {address.Building}");
            if (!string.IsNullOrWhiteSpace(address?.Apartment))
                optionalAddressLines.Add($"الدور/الشقة: {address.Apartment}");
            if (!string.IsNullOrWhiteSpace(address?.PostalCode))
                optionalAddressLines.Add($"الرمز البريدي: {address.PostalCode}");

            var messageLines = new List<string>
            {
                "مرحباً، أود تأكيد طلبي.",
                $"رقم الطلب: {order.OrderNumber}",
                "",
                "المنتجات:",
                string.Join("\n", productLines),
                "",
                $"إجمالي المنتجات: {order.Subtotal:N2} ج.م"
            };
            if (order.DiscountAmount > 0)
                messageLines.Add($"الخصم: -{order.DiscountAmount:N2} ج.م");
            messageLines.Add("تكلفة الشحن: لم تُحدد بعد.");
            messageLines.Add($"الإجمالي النهائي: سيُحسب بعد إضافة تكلفة الشحن (حالياً {order.TotalAmount:N2} ج.م قبل الشحن).");
            messageLines.Add("");
            messageLines.Add("بيانات المستلم:");
            if (address is not null)
            {
                messageLines.Add($"الاسم: {address.RecipientName}");
                messageLines.Add($"الهاتف: {address.PhoneNumber}");
                messageLines.Add($"المحافظة/المدينة: {(string.IsNullOrWhiteSpace(address.StateOrGovernorate) ? address.City : $"{address.StateOrGovernorate} / {address.City}")}");
                messageLines.Add($"العنوان: {address.StreetAddress}");
                foreach (var line in optionalAddressLines) messageLines.Add(line);
            }
            messageLines.Add("");
            messageLines.Add("يرجى تأكيد ما يلي:");
            messageLines.Add("1. توافر المنتجات في الطلب.");
            messageLines.Add("2. تكلفة الشحن إلى العنوان المذكور.");
            messageLines.Add("3. الإجمالي النهائي شامل الشحن.");
            messageLines.Add("4. ما إذا كان الدفع كامل المبلغ أم عربون.");
            messageLines.Add("5. في حال الدفع الجزئي: قيمة العربون والمبلغ المتبقي.");
            messageLines.Add("6. وسيلة الدفع وبيانات التحويل.");
            messageLines.Add("7. المدة المتوقعة للتجهيز والتوصيل.");
            messageLines.Add("");
            messageLines.Add("سأؤكد طريقة الدفع بعد الاتفاق على المبلغ النهائي. لا أفترض توفر الدفع عند الاستلام أو الشحن المجاني.");
            var message = string.Join("\n", messageLines);
            return Redirect($"https://wa.me/{whatsAppNumber}?text={Uri.EscapeDataString(message)}");
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        return View(new CheckoutPageViewModel { Input = input, Cart = cart, SavedAddresses = addresses });
    }

    private Guid UserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
