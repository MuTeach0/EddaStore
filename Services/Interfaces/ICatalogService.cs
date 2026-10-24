using EddaStore.Models.Products;

namespace EddaStore.Services.Interfaces;

public interface ICatalogService
{
    Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Brand>> GetBrandsAsync(CancellationToken cancellationToken = default);
    Task<Category?> GetCategoryAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Brand?> GetBrandAsync(Guid id, CancellationToken cancellationToken = default);
    Task CreateCategoryAsync(Category entity, CancellationToken cancellationToken = default);
    Task UpdateCategoryAsync(Category entity, CancellationToken cancellationToken = default);
    Task DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default);
    Task CreateBrandAsync(Brand entity, CancellationToken cancellationToken = default);
    Task UpdateBrandAsync(Brand entity, CancellationToken cancellationToken = default);
    Task DeleteBrandAsync(Guid id, CancellationToken cancellationToken = default);
}
