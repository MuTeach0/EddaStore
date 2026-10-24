using EddaStore.Models.Customers;
namespace EddaStore.Services.Interfaces;
public interface IReviewAdminService { Task<IReadOnlyList<Review>> GetAllAsync(CancellationToken ct=default); Task SetApprovalAsync(Guid id,bool approved,CancellationToken ct=default); Task DeleteAsync(Guid id,CancellationToken ct=default); }
