using EddaStore.Models.Products;
namespace EddaStore.Repositories.Interfaces;
public interface IProductImageRepository { Task<IReadOnlyList<ProductImage>> GetByProductIdAsync(Guid productId, CancellationToken ct=default); Task<ProductImage?> GetAsync(Guid id, CancellationToken ct=default); Task AddAsync(ProductImage image,CancellationToken ct=default); void Delete(ProductImage image); Task<bool> SaveChangesAsync(CancellationToken ct=default); }
