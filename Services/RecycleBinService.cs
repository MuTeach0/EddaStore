using EddaStore.Models.ViewModels.Admin.RecycleBin;using EddaStore.Repositories.Interfaces;using EddaStore.Services.Interfaces;
namespace EddaStore.Services;
public class RecycleBinService(IRecycleBinRepository repository):IRecycleBinService { public Task<IReadOnlyList<RecycleBinItemViewModel>> GetItemsAsync(string? entityType,string? search,CancellationToken ct=default)=>repository.GetItemsAsync(entityType,search,ct); public Task RestoreAsync(string entityType,Guid id,CancellationToken ct=default)=>repository.RestoreAsync(entityType,id,ct); }
