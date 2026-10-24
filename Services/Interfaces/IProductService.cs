
using EddaStore.Models.Products;
using EddaStore.Models.ViewModels.Admin.Products;

namespace EddaStore.Services.Interfaces;

public interface IProductService
{
    Task<IReadOnlyList<Product>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<Product?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Product?> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);

    Task CreateAsync(
        Product product,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Product product,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Product> Products, int TotalCount)> GetAdminProductsAsync(
        ProductIndexViewModel filters,
        CancellationToken cancellationToken = default);
}
