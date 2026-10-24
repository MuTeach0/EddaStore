
using Microsoft.AspNetCore.Identity;
using EddaStore.Models.Common;

namespace EddaStore.Models.Identity;

public class ApplicationUser : IdentityUser<Guid>, ISoftDelete
{
    public bool IsDeleted { get; set; } = false;

    public DateTimeOffset? DeletedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; } =
        DateTimeOffset.UtcNow;

    public DateTimeOffset? UpdatedAt { get; set; }
}
