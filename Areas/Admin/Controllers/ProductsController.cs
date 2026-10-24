
using EddaStore.Models.Products;
using EddaStore.Models.ViewModels.Admin.Products;
using EddaStore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EddaStore.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ProductsController(
    IProductService productService,
    ICatalogService catalogService,
    IProductImageService productImageService) : Controller
{
    private readonly IProductService _productService = productService;
    private readonly ICatalogService _catalogService = catalogService;
    private readonly IProductImageService _productImageService = productImageService;

    public async Task<IActionResult> Index(
        ProductIndexViewModel filters,
        CancellationToken cancellationToken)
    {
        filters.Page = Math.Max(1, filters.Page);
        filters.PageSize = Math.Clamp(filters.PageSize, 1, 100);

        var result = await _productService.GetAdminProductsAsync(
            filters, cancellationToken);

        filters.Products = result.Products;
        filters.TotalCount = result.TotalCount;

        await LoadFilterOptionsAsync(
            filters, cancellationToken);

        return View(filters);
    }

    [HttpGet]
    public async Task<IActionResult> Create(
        CancellationToken cancellationToken)
    {
        var model = new ProductCreateViewModel();

        await LoadFormOptionsAsync(model, cancellationToken);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        ProductCreateViewModel model,
        CancellationToken cancellationToken)
    {
        ValidateComparisonPrice(
            model.Price, model.CompareAtPrice);

        if (!ModelState.IsValid)
        {
            await LoadFormOptionsAsync(model, cancellationToken);
            return View(model);
        }

        var product = new Product
        {
            Name = model.Name.Trim(),
            Slug = model.Slug.Trim(),
            Description = model.Description,
            Price = model.Price,
            CompareAtPrice = model.CompareAtPrice,
            StockQuantity = model.StockQuantity,
            SKU = model.SKU,
            IsActive = model.IsActive,
            CategoryId = model.CategoryId!.Value,
            BrandId = model.BrandId!.Value
        };

        try
        {
            await _productService.CreateAsync(
                product, cancellationToken);

            await _productImageService.AddAsync(product.Id, model.NewImages, cancellationToken);

            TempData["Success"] = "تمت إضافة المنتج بنجاح.";

            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        await LoadFormOptionsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(
        Guid id,
        CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(
            id, cancellationToken);

        if (product is null)
            return NotFound();

        var model = new ProductEditViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Slug = product.Slug,
            Description = product.Description,
            Price = product.Price,
            CompareAtPrice = product.CompareAtPrice,
            StockQuantity = product.StockQuantity,
            SKU = product.SKU,
            IsActive = product.IsActive,
            CategoryId = product.CategoryId,
            BrandId = product.BrandId
            ,ExistingImages = product.Images.OrderBy(x => x.SortOrder).ToList()
        };

        await LoadFormOptionsAsync(model, cancellationToken);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        Guid id,
        ProductEditViewModel model,
        CancellationToken cancellationToken)
    {
        if (id != model.Id)
            return BadRequest();

        ValidateComparisonPrice(
            model.Price, model.CompareAtPrice);

        if (!ModelState.IsValid)
        {
            await LoadFormOptionsAsync(model, cancellationToken);
            return View(model);
        }

        var product = new Product
        {
            Id = model.Id,
            Name = model.Name.Trim(),
            Slug = model.Slug.Trim(),
            Description = model.Description,
            Price = model.Price,
            CompareAtPrice = model.CompareAtPrice,
            StockQuantity = model.StockQuantity,
            SKU = model.SKU,
            IsActive = model.IsActive,
            CategoryId = model.CategoryId!.Value,
            BrandId = model.BrandId!.Value
        };

        try
        {
            await _productService.UpdateAsync(
                product, cancellationToken);

            await _productImageService.AddAsync(product.Id, model.NewImages, cancellationToken);

            TempData["Success"] = "تم تعديل المنتج بنجاح.";

            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        await LoadFormOptionsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            await _productService.DeleteAsync(id, cancellationToken);
            TempData["Success"] = "تم حذف المنتج بنجاح.";
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetPrimaryImage(Guid productId, Guid imageId, CancellationToken cancellationToken)
    {
        try { await _productImageService.SetPrimaryAsync(productId, imageId, cancellationToken); TempData["Success"] = "تم تعيين الصورة الرئيسية."; }
        catch (KeyNotFoundException) { return NotFound(); }
        return RedirectToAction(nameof(Edit), new { id = productId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteImage(Guid productId, Guid imageId, CancellationToken cancellationToken)
    {
        try { await _productImageService.DeleteAsync(productId, imageId, cancellationToken); TempData["Success"] = "تم حذف الصورة."; }
        catch (KeyNotFoundException) { return NotFound(); }
        return RedirectToAction(nameof(Edit), new { id = productId });
    }

    private void ValidateComparisonPrice(
        decimal price,
        decimal? compareAtPrice)
    {
        if (compareAtPrice.HasValue &&
            compareAtPrice.Value <= price)
        {
            ModelState.AddModelError(
                nameof(ProductCreateViewModel.CompareAtPrice),
                "السعر قبل الخصم يجب أن يكون أكبر من السعر الحالي.");
        }
    }

    private async Task LoadFormOptionsAsync(
        ProductCreateViewModel model,
        CancellationToken cancellationToken)
    {
        model.Categories = (await _catalogService.GetCategoriesAsync(cancellationToken))
            .Select(category => new SelectListItem
            {
                Value = category.Id.ToString(),
                Text = category.Name,
                Selected = model.CategoryId == category.Id
            })
            .ToList();

        model.Brands = (await _catalogService.GetBrandsAsync(cancellationToken))
            .Select(brand => new SelectListItem
            {
                Value = brand.Id.ToString(),
                Text = brand.Name,
                Selected = model.BrandId == brand.Id
            })
            .ToList();
    }

    private async Task LoadFilterOptionsAsync(
        ProductIndexViewModel model,
        CancellationToken cancellationToken)
    {
        ViewBag.Categories = (await _catalogService.GetCategoriesAsync(cancellationToken))
            .Select(category => new SelectListItem
            {
                Value = category.Id.ToString(),
                Text = category.Name
            })
            .ToList();

        ViewBag.Brands = (await _catalogService.GetBrandsAsync(cancellationToken))
            .Select(brand => new SelectListItem
            {
                Value = brand.Id.ToString(),
                Text = brand.Name
            })
            .ToList();
    }
}
