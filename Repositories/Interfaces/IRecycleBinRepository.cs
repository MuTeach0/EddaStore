using EddaStore.Models.ViewModels.Admin.RecycleBin;

namespace EddaStore.Repositories.Interfaces;

public interface IRecycleBinRepository
{
    Task<IReadOnlyList<RecycleBinItemViewModel>> GetItemsAsync(string? entityType, string? search, CancellationToken cancellationToken = default);
    Task RestoreAsync(string entityType, Guid id, CancellationToken cancellationToken = default);
}
