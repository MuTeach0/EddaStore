using EddaStore.Models.Identity;

namespace EddaStore.Services.Interfaces;

public interface IAdminUsersService
{
    Task<IReadOnlyList<ApplicationUser>> GetUsersAsync(CancellationToken cancellationToken = default);
    Task<bool> PhoneNumberExistsAsync(string phoneNumber, Guid? exceptUserId = null, CancellationToken cancellationToken = default);
}
