using EddaStore.Models.Products;
using EddaStore.Models.ViewModels.Admin.Catalog;
using EddaStore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EddaStore.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "Admin")]
public class BrandsController(ICatalogService catalogService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct) => View(await catalogService.GetBrandsAsync(ct));
    public IActionResult Create() => View("Form", new CatalogItemViewModel());
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Create(CatalogItemViewModel model, CancellationToken ct) { if (!ModelState.IsValid) return View("Form", model); try { await catalogService.CreateBrandAsync(Map(model), ct); TempData["Success"] = "تمت إضافة الماركة بنجاح."; return RedirectToAction(nameof(Index)); } catch (ArgumentException ex) { ModelState.AddModelError(string.Empty, ex.Message); return View("Form", model); } }
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct) { var x = await catalogService.GetBrandAsync(id, ct); return x is null ? NotFound() : View("Form", new CatalogItemViewModel { Id = x.Id, Name = x.Name, Slug = x.Slug, Description = x.Description, LogoUrl = x.LogoUrl }); }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Edit(Guid id, CatalogItemViewModel model, CancellationToken ct) { if (id != model.Id) return BadRequest(); if (!ModelState.IsValid) return View("Form", model); try { await catalogService.UpdateBrandAsync(Map(model), ct); TempData["Success"] = "تم تعديل الماركة بنجاح."; return RedirectToAction(nameof(Index)); } catch (KeyNotFoundException) { return NotFound(); } catch (ArgumentException ex) { ModelState.AddModelError(string.Empty, ex.Message); return View("Form", model); } }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Delete(Guid id, CancellationToken ct) { try { await catalogService.DeleteBrandAsync(id, ct); TempData["Success"] = "تم حذف الماركة بنجاح."; } catch (KeyNotFoundException) { return NotFound(); } catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; } return RedirectToAction(nameof(Index)); }
    private static Brand Map(CatalogItemViewModel x) => new() { Id = x.Id, Name = x.Name, Slug = x.Slug, Description = x.Description, LogoUrl = x.LogoUrl };
}
