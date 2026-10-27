
using EddaStore.Models.Products;
using EddaStore.Models.ViewModels.Admin.Products;
using EddaStore.Repositories.Interfaces;
using EddaStore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EddaStore.Services;

public class ProductService(
    IProductRepository productRepository,
    ICatalogRepository catalogRepository) : IProductService
{
    private readonly IProductRepository _productRepository = productRepository;
    private readonly ICatalogRepository _catalogRepository = catalogRepository;

    public Task<IReadOnlyList<Product>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return _productRepository.GetAllAsync(cancellationToken);
    }

    public Task<Product?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _productRepository.GetByIdAsync(
            id, cancellationToken);
    }

    public Task<Product?> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        return _productRepository.GetBySlugAsync(
            slug, cancellationToken);
    }

    public async Task CreateAsync(
        Product product,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);

        await ValidateProductAsync(
            product, null, cancellationToken);

        await _productRepository.AddAsync(
            product, cancellationToken);

        await _productRepository.SaveChangesAsync(
            cancellationToken);
    }

    public async Task UpdateAsync(
        Product product,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);

        var existing = await _productRepository.GetByIdAsync(
            product.Id, cancellationToken);

        if (existing is null)
        {
            throw new KeyNotFoundException(
                "المنتج غير موجود.");
        }

        await ValidateProductAsync(
            product, product.Id, cancellationToken);

        // Update only the fields managed by the product form.
        existing.Name = product.Name;
        existing.Slug = product.Slug;
        existing.Description = product.Description;
        existing.Price = product.Price;
        existing.CompareAtPrice = product.CompareAtPrice;
        existing.StockQuantity = product.StockQuantity;
        existing.SKU = product.SKU;
        existing.IsActive = product.IsActive;
        existing.CategoryId = product.CategoryId;
        existing.BrandId = product.BrandId;

        _productRepository.Update(existing);

        await _productRepository.SaveChangesAsync(
            cancellationToken);
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdForDeleteAsync(
            id, cancellationToken);

        if (product is null)
        {
            throw new KeyNotFoundException(
                "المنتج غير موجود.");
        }

        // ApplicationDbContext converts Remove into a soft delete.
        _productRepository.Delete(product);

        await _productRepository.SaveChangesAsync(
            cancellationToken);
    }

    private async Task ValidateProductAsync(
        Product product,
        Guid? excludeProductId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(product.Name))
        {
            throw new ArgumentException(
                "اسم المنتج مطلوب.");
        }

        if (string.IsNullOrWhiteSpace(product.Slug))
        {
            throw new ArgumentException(
                "رابط المنتج مطلوب.");
        }

        // Normalize input before validating uniqueness.
        product.Name = product.Name.Trim();
        product.Slug = product.Slug.Trim();

        product.SKU = string.IsNullOrWhiteSpace(product.SKU)
            ? null
            : product.SKU.Trim();

        product.Description =
            string.IsNullOrWhiteSpace(product.Description)
                ? null
                : product.Description.Trim();

        if (product.Price < 0)
        {
            throw new ArgumentException(
                "السعر لا يمكن أن يكون سالبًا.");
        }

        if (product.CompareAtPrice.HasValue &&
            product.CompareAtPrice.Value < 0)
        {
            throw new ArgumentException(
                "السعر قبل الخصم لا يمكن أن يكون سالبًا.");
        }

        if (product.CompareAtPrice.HasValue &&
            product.CompareAtPrice.Value <= product.Price)
        {
            throw new ArgumentException(
                "السعر قبل الخصم يجب أن يكون أكبر من السعر الحالي.");
        }

        if (product.StockQuantity < 0)
        {
            throw new ArgumentException(
                "المخزون لا يمكن أن يكون سالبًا.");
        }

        var slugExists = await _productRepository.SlugExistsAsync(
            product.Slug,
            excludeProductId,
            cancellationToken);

        if (slugExists)
        {
            throw new ArgumentException(
                "رابط المنتج مستخدم بالفعل.");
        }

        var categoryExists = await _catalogRepository
            .GetCategoryAsync(product.CategoryId, cancellationToken) is not null;

        if (!categoryExists)
        {
            throw new ArgumentException(
                "التصنيف المحدد غير موجود أو غير متاح.");
        }

        var brandExists = await _catalogRepository
            .GetBrandAsync(product.BrandId, cancellationToken) is not null;

        if (!brandExists)
        {
            throw new ArgumentException(
                "الماركة المحددة غير موجودة أو غير متاحة.");
        }
    }

    public Task<(IReadOnlyList<Product> Products, int TotalCount)>
    GetAdminProductsAsync(
        ProductIndexViewModel filters,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filters);

        return _productRepository.GetAdminProductsAsync(
            filters,
            cancellationToken);
    }
}
