using EddaStore.Models.Products;

namespace EddaStore.Repositories.Interfaces;

public interface ICatalogRepository
{
    Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Brand>> GetBrandsAsync(CancellationToken cancellationToken = default);
    Task<Category?> GetCategoryAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Brand?> GetBrandAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> CategorySlugExistsAsync(string slug, Guid? excludedId, CancellationToken cancellationToken = default);
    Task<bool> BrandSlugExistsAsync(string slug, Guid? excludedId, CancellationToken cancellationToken = default);
    Task<bool> CategoryCanBeDeletedAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> BrandCanBeDeletedAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddCategoryAsync(Category category, CancellationToken cancellationToken = default);
    Task AddBrandAsync(Brand brand, CancellationToken cancellationToken = default);
    void DeleteCategory(Category category);
    void DeleteBrand(Brand brand);
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
}
