using EddaStore.Models.ViewModels.Store;
namespace EddaStore.Services.Interfaces;

public interface ICartService { Task<CartViewModel> GetAsync(Guid customerId, CancellationToken ct = default); Task AddAsync(Guid customerId, Guid productId, int quantity, CancellationToken ct = default); Task UpdateAsync(Guid customerId, Guid productId, int quantity, CancellationToken ct = default); Task RemoveAsync(Guid customerId, Guid productId, CancellationToken ct = default); }
