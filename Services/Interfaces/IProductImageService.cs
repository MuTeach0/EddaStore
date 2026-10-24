using Microsoft.AspNetCore.Http; using EddaStore.Models.Products;
namespace EddaStore.Services.Interfaces;
public interface IProductImageService { Task<IReadOnlyList<ProductImage>> GetAsync(Guid productId,CancellationToken ct=default); Task AddAsync(Guid productId,IReadOnlyList<IFormFile> files,CancellationToken ct=default); Task SetPrimaryAsync(Guid productId,Guid imageId,CancellationToken ct=default); Task DeleteAsync(Guid productId,Guid imageId,CancellationToken ct=default); }
