using EddaStore.Data;
using EddaStore.Models.Customers;
using EddaStore.Models.Orders;
using EddaStore.Models.ViewModels.Store;
using EddaStore.Repositories.Interfaces;
using EddaStore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace EddaStore.Services;
public sealed class AccountService(ApplicationDbContext context, IGenericRepository<Address> addresses) : IAccountService
{
    public async Task<AccountViewModel> GetOverviewAsync(Guid customerId, string email, CancellationToken ct = default) => new()
    {
        Email = email,
        PhoneNumber = await context.Users.Where(x => x.Id == customerId).Select(x => x.PhoneNumber).FirstOrDefaultAsync(ct),
        WishlistCount = await context.WishlistItems.CountAsync(x => x.CustomerId == customerId, ct),
        Orders = await context.Orders.AsNoTracking().Where(x => x.CustomerId == customerId).Include(x => x.Items).OrderByDescending(x => x.PlacedAt).ToListAsync(ct),
        Addresses = await context.Addresses.AsNoTracking().Where(x => x.CustomerId == customerId).OrderByDescending(x => x.IsDefault).ToListAsync(ct)
    };
    public Task<Order?> GetOrderAsync(Guid orderId, Guid customerId, CancellationToken ct = default) => context.Orders.AsNoTracking().Where(x => x.Id == orderId && x.CustomerId == customerId).Include(x => x.Items).Include(x => x.StatusHistory).FirstOrDefaultAsync(ct);
    public async Task AddAddressAsync(Guid customerId, Address address, CancellationToken ct = default)
    {
        address.CustomerId = customerId;
        if (address.IsDefault)
        {
            var old = await context.Addresses.Where(x => x.CustomerId == customerId && x.IsDefault).ToListAsync(ct);
            foreach (var item in old) item.IsDefault = false;
        }
        await addresses.AddAsync(address, ct);
        await addresses.SaveChangesAsync(ct);
    }
}
