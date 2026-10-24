using EddaStore.Models.Customers;
using EddaStore.Models.Orders;
using EddaStore.Models.ViewModels.Store;
namespace EddaStore.Services.Interfaces;
public interface IAccountService
{
    Task<AccountViewModel> GetOverviewAsync(Guid customerId, string email, CancellationToken ct = default);
    Task<Order?> GetOrderAsync(Guid orderId, Guid customerId, CancellationToken ct = default);
    Task AddAddressAsync(Guid customerId, Address address, CancellationToken ct = default);
}
