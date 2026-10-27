using EddaStore.Models.Products;
using EddaStore.Models.ViewModels.Admin.Products;

namespace EddaStore.Repositories.Interfaces;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<Product?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Product?> GetByIdForDeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Product?> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Product product,
        CancellationToken cancellationToken = default);

    void Update(Product product);

    void Delete(Product product);

    Task<bool> SlugExistsAsync(
        string slug,
        Guid? excludeProductId = null,
        CancellationToken cancellationToken = default);

    Task<bool> SaveChangesAsync(
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Product> Products, int TotalCount)> GetAdminProductsAsync(
        ProductIndexViewModel filters,
        CancellationToken cancellationToken = default);
}
