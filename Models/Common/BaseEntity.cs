
namespace EddaStore.Models.Common;

public class BaseEntity : ISoftDelete
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public bool IsDeleted { get; set; } = false;

    public DateTimeOffset? DeletedAt { get; set; }
}
