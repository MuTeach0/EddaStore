using EddaStore.Models.Products;
using EddaStore.Repositories.Interfaces;
using EddaStore.Services.Interfaces;

namespace EddaStore.Services;

public class CatalogService(ICatalogRepository repository) : ICatalogService
{
    public Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default) => repository.GetCategoriesAsync(cancellationToken);
    public Task<IReadOnlyList<Brand>> GetBrandsAsync(CancellationToken cancellationToken = default) => repository.GetBrandsAsync(cancellationToken);
    public Task<Category?> GetCategoryAsync(Guid id, CancellationToken cancellationToken = default) => repository.GetCategoryAsync(id, cancellationToken);
    public Task<Brand?> GetBrandAsync(Guid id, CancellationToken cancellationToken = default) => repository.GetBrandAsync(id, cancellationToken);
    public async Task CreateCategoryAsync(Category entity, CancellationToken ct = default) { await ValidateCategory(entity, null, ct); await repository.AddCategoryAsync(entity, ct); await repository.SaveChangesAsync(ct); }
    public async Task UpdateCategoryAsync(Category entity, CancellationToken ct = default) { var current = await repository.GetCategoryAsync(entity.Id, ct) ?? throw new KeyNotFoundException(); await ValidateCategory(entity, entity.Id, ct); current.Name=entity.Name; current.Slug=entity.Slug; current.Description=entity.Description; current.ParentCategoryId=entity.ParentCategoryId; await repository.SaveChangesAsync(ct); }
    public async Task DeleteCategoryAsync(Guid id, CancellationToken ct = default) { var entity=await repository.GetCategoryAsync(id,ct) ?? throw new KeyNotFoundException(); if(!await repository.CategoryCanBeDeletedAsync(id,ct)) throw new InvalidOperationException("لا يمكن حذف تصنيف مرتبط بمنتجات أو تصنيفات فرعية."); repository.DeleteCategory(entity); await repository.SaveChangesAsync(ct); }
    public async Task CreateBrandAsync(Brand entity, CancellationToken ct = default) { await ValidateBrand(entity,null,ct); await repository.AddBrandAsync(entity,ct); await repository.SaveChangesAsync(ct); }
    public async Task UpdateBrandAsync(Brand entity, CancellationToken ct = default) { var current=await repository.GetBrandAsync(entity.Id,ct) ?? throw new KeyNotFoundException(); await ValidateBrand(entity,entity.Id,ct); current.Name=entity.Name; current.Slug=entity.Slug; current.Description=entity.Description; current.LogoUrl=entity.LogoUrl; await repository.SaveChangesAsync(ct); }
    public async Task DeleteBrandAsync(Guid id, CancellationToken ct = default) { var entity=await repository.GetBrandAsync(id,ct) ?? throw new KeyNotFoundException(); if(!await repository.BrandCanBeDeletedAsync(id,ct)) throw new InvalidOperationException("لا يمكن حذف ماركة مرتبطة بمنتجات."); repository.DeleteBrand(entity); await repository.SaveChangesAsync(ct); }
    private async Task ValidateCategory(Category x, Guid? id, CancellationToken ct) { Normalize(x); if(x.ParentCategoryId==x.Id) throw new ArgumentException("لا يمكن اختيار التصنيف نفسه كتصنيف أب."); if(await repository.CategorySlugExistsAsync(x.Slug,id,ct)) throw new ArgumentException("هذا الرابط مستخدم بالفعل."); }
    private async Task ValidateBrand(Brand x, Guid? id, CancellationToken ct) { Normalize(x); if(await repository.BrandSlugExistsAsync(x.Slug,id,ct)) throw new ArgumentException("هذا الرابط مستخدم بالفعل."); }
    private static void Normalize(Category x) { x.Name=x.Name.Trim(); x.Slug=x.Slug.Trim().ToLowerInvariant(); x.Description=string.IsNullOrWhiteSpace(x.Description)?null:x.Description.Trim(); }
    private static void Normalize(Brand x) { x.Name=x.Name.Trim(); x.Slug=x.Slug.Trim().ToLowerInvariant(); x.Description=string.IsNullOrWhiteSpace(x.Description)?null:x.Description.Trim(); x.LogoUrl=string.IsNullOrWhiteSpace(x.LogoUrl)?null:x.LogoUrl.Trim(); }
}
