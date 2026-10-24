using EddaStore.Models.Identity;
using EddaStore.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EddaStore.Services;

public sealed class AdminUsersService(UserManager<ApplicationUser> userManager) : IAdminUsersService
{
    public async Task<IReadOnlyList<ApplicationUser>> GetUsersAsync(CancellationToken cancellationToken = default)
        => await userManager.Users.AsNoTracking()
            .OrderByDescending(user => user.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<bool> PhoneNumberExistsAsync(string phoneNumber, Guid? exceptUserId = null, CancellationToken cancellationToken = default)
        => userManager.Users.AnyAsync(user => user.PhoneNumber == phoneNumber && (!exceptUserId.HasValue || user.Id != exceptUserId.Value), cancellationToken);
}
