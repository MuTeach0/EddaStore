using EddaStore.Models.Coupons;
namespace EddaStore.Services.Interfaces;
public interface ICouponService { Task<IReadOnlyList<Coupon>> GetAllAsync(CancellationToken ct=default); Task<Coupon?> GetAsync(Guid id,CancellationToken ct=default); Task CreateAsync(Coupon coupon,CancellationToken ct=default); Task UpdateAsync(Coupon coupon,CancellationToken ct=default); Task DeleteAsync(Guid id,CancellationToken ct=default); }
