using EddaStore.Models.Products;
using EddaStore.Models.ViewModels.Admin.Catalog;
using EddaStore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EddaStore.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "Admin")]
public class CategoriesController(ICatalogService catalogService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct) => View(await catalogService.GetCategoriesAsync(ct));
    public async Task<IActionResult> Create(CancellationToken ct) { await Parents(null, null, ct); return View("Form", new CatalogItemViewModel()); }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Create(CatalogItemViewModel model, CancellationToken ct) { if(!ModelState.IsValid) { await Parents(model.ParentCategoryId,null,ct); return View("Form",model); } try { await catalogService.CreateCategoryAsync(Map(model),ct); TempData["Success"]="تمت إضافة التصنيف بنجاح."; return RedirectToAction(nameof(Index)); } catch(ArgumentException ex) { ModelState.AddModelError(string.Empty,ex.Message); await Parents(model.ParentCategoryId,null,ct); return View("Form",model); } }
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct) { var x=await catalogService.GetCategoryAsync(id,ct); if(x is null)return NotFound(); var model=new CatalogItemViewModel{Id=x.Id,Name=x.Name,Slug=x.Slug,Description=x.Description,ParentCategoryId=x.ParentCategoryId}; await Parents(x.ParentCategoryId,id,ct); return View("Form",model); }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Edit(Guid id,CatalogItemViewModel model,CancellationToken ct) { if(id!=model.Id)return BadRequest(); if(!ModelState.IsValid){await Parents(model.ParentCategoryId,id,ct);return View("Form",model);} try{await catalogService.UpdateCategoryAsync(Map(model),ct);TempData["Success"]="تم تعديل التصنيف بنجاح.";return RedirectToAction(nameof(Index));}catch(KeyNotFoundException){return NotFound();}catch(ArgumentException ex){ModelState.AddModelError(string.Empty,ex.Message);await Parents(model.ParentCategoryId,id,ct);return View("Form",model);} }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Delete(Guid id,CancellationToken ct) { try{await catalogService.DeleteCategoryAsync(id,ct);TempData["Success"]="تم حذف التصنيف بنجاح.";}catch(KeyNotFoundException){return NotFound();}catch(InvalidOperationException ex){TempData["Error"]=ex.Message;}return RedirectToAction(nameof(Index)); }
    private async Task Parents(Guid? selected,Guid? excluded,CancellationToken ct) => ViewBag.Parents=(await catalogService.GetCategoriesAsync(ct)).Where(x=>x.Id!=excluded).Select(x=>new SelectListItem(x.Name,x.Id.ToString(),x.Id==selected)).ToList();
    private static Category Map(CatalogItemViewModel x)=>new(){Id=x.Id,Name=x.Name,Slug=x.Slug,Description=x.Description,ParentCategoryId=x.ParentCategoryId};
}
