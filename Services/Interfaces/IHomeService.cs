using EddaStore.Models.ViewModels;

namespace EddaStore.Services.Interfaces;

public interface IHomeService
{
    Task<HomeViewModel> GetHomePageAsync(CancellationToken cancellationToken = default);
}
