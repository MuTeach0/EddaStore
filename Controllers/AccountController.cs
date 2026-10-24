using System.Security.Claims;
using EddaStore.Data;
using EddaStore.Models.Customers;
using EddaStore.Models.ViewModels.Store;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EddaStore.Controllers;

[Authorize]
public class AccountController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var id = UserId();
        return View(new AccountViewModel { Email = User.Identity?.Name ?? string.Empty,
            PhoneNumber = await context.Users.Where(x => x.Id == id).Select(x => x.PhoneNumber).FirstOrDefaultAsync(ct),
            WishlistCount = await context.WishlistItems.CountAsync(x => x.CustomerId == id, ct),
            Orders = await context.Orders.Where(x => x.CustomerId == id).Include(x => x.Items).OrderByDescending(x => x.PlacedAt).ToListAsync(ct),
            Addresses = await context.Addresses.Where(x => x.CustomerId == id).OrderByDescending(x => x.IsDefault).ToListAsync(ct) });
    }
    public async Task<IActionResult> Details(Guid id, CancellationToken ct)
    { var order = await context.Orders.Where(x => x.Id == id && x.CustomerId == UserId()).Include(x => x.Items).Include(x => x.StatusHistory).FirstOrDefaultAsync(ct); return order is null ? NotFound() : View(order); }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAddress([Bind("FullName,PhoneNumber,City,StreetAddress,Building,Apartment,PostalCode,IsDefault")] Address address, CancellationToken ct)
    { if (!ModelState.IsValid) return RedirectToAction(nameof(Index)); address.CustomerId = UserId(); if (address.IsDefault) { var old = await context.Addresses.Where(x => x.CustomerId == address.CustomerId && x.IsDefault).ToListAsync(ct); foreach (var item in old) item.IsDefault = false; } context.Addresses.Add(address); await context.SaveChangesAsync(ct); TempData["Success"] = "تم حفظ العنوان."; return RedirectToAction(nameof(Index)); }
    private Guid UserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
