using EddaStore.Models.ViewModels.Store;

namespace EddaStore.Services.Interfaces;

public interface ICheckoutService
{
    Task<Guid> CreateOrderAsync(Guid customerId, CheckoutInputViewModel input, CancellationToken ct = default);
}
