using EddaStore.Data;
using EddaStore.Models.Products;
using EddaStore.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EddaStore.Repositories.Implementations;

public class CatalogRepository(ApplicationDbContext context) : ICatalogRepository
{
    public async Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default) => await context.Categories.AsNoTracking().Include(x => x.ParentCategory).Include(x => x.Products).Include(x => x.SubCategories).OrderBy(x => x.Name).ToListAsync(cancellationToken);
    public async Task<IReadOnlyList<Brand>> GetBrandsAsync(CancellationToken cancellationToken = default) => await context.Brands.AsNoTracking().Include(x => x.Products).OrderBy(x => x.Name).ToListAsync(cancellationToken);
    public Task<Category?> GetCategoryAsync(Guid id, CancellationToken cancellationToken = default) => context.Categories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public Task<Brand?> GetBrandAsync(Guid id, CancellationToken cancellationToken = default) => context.Brands.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public Task<bool> CategorySlugExistsAsync(string slug, Guid? excludedId, CancellationToken cancellationToken = default) => context.Categories.AnyAsync(x => x.Slug == slug && (!excludedId.HasValue || x.Id != excludedId), cancellationToken);
    public Task<bool> BrandSlugExistsAsync(string slug, Guid? excludedId, CancellationToken cancellationToken = default) => context.Brands.AnyAsync(x => x.Slug == slug && (!excludedId.HasValue || x.Id != excludedId), cancellationToken);
    public async Task<bool> CategoryCanBeDeletedAsync(Guid id, CancellationToken cancellationToken = default) => !await context.Products.AnyAsync(x => x.CategoryId == id, cancellationToken) && !await context.Categories.AnyAsync(x => x.ParentCategoryId == id, cancellationToken);
    public Task<bool> BrandCanBeDeletedAsync(Guid id, CancellationToken cancellationToken = default) => context.Products.AllAsync(x => x.BrandId != id, cancellationToken);
    public Task AddCategoryAsync(Category category, CancellationToken cancellationToken = default) => context.Categories.AddAsync(category, cancellationToken).AsTask();
    public Task AddBrandAsync(Brand brand, CancellationToken cancellationToken = default) => context.Brands.AddAsync(brand, cancellationToken).AsTask();
    public void DeleteCategory(Category category) => context.Categories.Remove(category);
    public void DeleteBrand(Brand brand) => context.Brands.Remove(brand);
    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default) => await context.SaveChangesAsync(cancellationToken) > 0;
}
