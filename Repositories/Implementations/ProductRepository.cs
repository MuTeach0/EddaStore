using EddaStore.Data;
using EddaStore.Models.Products;
using EddaStore.Models.ViewModels.Admin.Products;
using EddaStore.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EddaStore.Repositories.Implementations;

public class ProductRepository(ApplicationDbContext context)
        : GenericRepository<Product>(context), IProductRepository
{
    public new async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.Include(x => x.Images).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
    public async Task<Product?> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(
                p => p.Slug == slug,
                cancellationToken);
    }

    public async Task<bool> SlugExistsAsync(
        string slug,
        Guid? excludeProductId = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(p => p.Slug == slug);

        if (excludeProductId.HasValue)
        {
            query = query.Where(
                p => p.Id != excludeProductId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public new async Task<bool> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return await Context.SaveChangesAsync(
            cancellationToken) > 0;
    }


    public async Task<(IReadOnlyList<Product> Products, int TotalCount)>
        GetAdminProductsAsync(
            ProductIndexViewModel filters,
            CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
        {
            var searchTerm = filters.SearchTerm.Trim();

            query = query.Where(p =>
                p.Name.Contains(searchTerm) ||
                p.Slug.Contains(searchTerm) ||
                (p.SKU != null && p.SKU.Contains(searchTerm)));
        }

        if (filters.CategoryId.HasValue)
        {
            query = query.Where(p =>
                p.CategoryId == filters.CategoryId.Value);
        }

        if (filters.BrandId.HasValue)
        {
            query = query.Where(p =>
                p.BrandId == filters.BrandId.Value);
        }

        if (filters.IsActive.HasValue)
        {
            query = query.Where(p =>
                p.IsActive == filters.IsActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var page = Math.Max(1, filters.Page);
        var pageSize = Math.Clamp(filters.PageSize, 1, 100);

        var products = await query
            .OrderByDescending(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (products, totalCount);
    }

}
