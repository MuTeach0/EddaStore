using EddaStore.Data;
using EddaStore.Models.ViewModels;
using EddaStore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EddaStore.Services;

public sealed class HomeService(ApplicationDbContext context) : IHomeService
{
    public async Task<HomeViewModel> GetHomePageAsync(CancellationToken cancellationToken = default)
    {
        var categories = await context.Categories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .Take(8)
            .ToListAsync(cancellationToken);

        var products = await context.Products
            .AsNoTracking()
            .Where(product => product.IsActive && product.StockQuantity > 0)
            .Include(product => product.Images)
            .OrderByDescending(product => product.Name)
            .Take(8)
            .ToListAsync(cancellationToken);

        foreach (var product in products)
        {
            product.Images = product.Images
                .OrderByDescending(image => image.IsPrimary)
                .ThenBy(image => image.SortOrder)
                .Take(1)
                .ToList();
        }

        return new HomeViewModel { Categories = categories, FeaturedProducts = products };
    }
}
