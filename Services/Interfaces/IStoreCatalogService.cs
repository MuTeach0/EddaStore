using EddaStore.Models.Products;using EddaStore.Models.ViewModels.Store;
namespace EddaStore.Services.Interfaces;
public interface IStoreCatalogService { Task<StoreIndexViewModel> GetAsync(StoreIndexViewModel filters,CancellationToken ct=default); Task<Product?> GetDetailsAsync(string slug,CancellationToken ct=default); }
