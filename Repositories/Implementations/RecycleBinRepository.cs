using EddaStore.Data;
using EddaStore.Models.Common;
using EddaStore.Models.Coupons;
using EddaStore.Models.Customers;
using EddaStore.Models.Products;
using EddaStore.Models.ViewModels.Admin.RecycleBin;
using EddaStore.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EddaStore.Repositories.Implementations;

public class RecycleBinRepository(ApplicationDbContext context) : IRecycleBinRepository
{
    public async Task<IReadOnlyList<RecycleBinItemViewModel>> GetItemsAsync(string? entityType, string? search, CancellationToken ct = default)
    {
        var items = new List<RecycleBinItemViewModel>();
        if (string.IsNullOrWhiteSpace(entityType) || entityType == "Product") items.AddRange((await context.Products.IgnoreQueryFilters().Where(x => x.IsDeleted).ToListAsync(ct)).Select(x => Item(x.Id,"Product","منتج",x.Name,x.Slug,x.DeletedAt)));
        if (string.IsNullOrWhiteSpace(entityType) || entityType == "Category") items.AddRange((await context.Categories.IgnoreQueryFilters().Where(x => x.IsDeleted).ToListAsync(ct)).Select(x => Item(x.Id,"Category","تصنيف",x.Name,x.Slug,x.DeletedAt)));
        if (string.IsNullOrWhiteSpace(entityType) || entityType == "Brand") items.AddRange((await context.Brands.IgnoreQueryFilters().Where(x => x.IsDeleted).ToListAsync(ct)).Select(x => Item(x.Id,"Brand","ماركة",x.Name,x.Slug,x.DeletedAt)));
        if (string.IsNullOrWhiteSpace(entityType) || entityType == "Coupon") items.AddRange((await context.Coupons.IgnoreQueryFilters().Where(x => x.IsDeleted).ToListAsync(ct)).Select(x => Item(x.Id,"Coupon","كوبون",x.Code,x.Description,x.DeletedAt)));
        if (string.IsNullOrWhiteSpace(entityType) || entityType == "Review") items.AddRange((await context.Reviews.IgnoreQueryFilters().Where(x => x.IsDeleted).ToListAsync(ct)).Select(x => Item(x.Id,"Review","مراجعة",$"تقييم {x.Rating}/5",x.Comment,x.DeletedAt)));
        if (!string.IsNullOrWhiteSpace(search)) items = items.Where(x => x.Name.Contains(search,StringComparison.OrdinalIgnoreCase) || (x.Detail?.Contains(search,StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
        return items.OrderByDescending(x => x.DeletedAt).ToList();
    }

    public async Task RestoreAsync(string entityType, Guid id, CancellationToken ct = default)
    {
        switch (entityType)
        {
            case "Product": await RestoreProduct(id, ct); break;
            case "Category": await RestoreCategory(id, ct); break;
            case "Brand": await Restore<Brand>(id, ct); break;
            case "Coupon": await RestoreCoupon(id, ct); break;
            case "Review": await Restore<Review>(id, ct); break;
            default: throw new ArgumentException("نوع السجل غير صالح.");
        }
        await context.SaveChangesAsync(ct);
    }
    private async Task RestoreProduct(Guid id,CancellationToken ct){var x=await context.Products.IgnoreQueryFilters().FirstOrDefaultAsync(x=>x.Id==id&&x.IsDeleted,ct)??throw new KeyNotFoundException();if(await context.Products.AnyAsync(p=>p.Slug==x.Slug,ct))throw new InvalidOperationException("تعذر الاستعادة لأن رابط المنتج مستخدم حاليًا.");if(!await context.Categories.AnyAsync(c=>c.Id==x.CategoryId,ct)||!await context.Brands.AnyAsync(b=>b.Id==x.BrandId,ct))throw new InvalidOperationException("استعد التصنيف والماركة المرتبطين أولًا.");UnDelete(x);}
    private async Task RestoreCategory(Guid id,CancellationToken ct){var x=await context.Categories.IgnoreQueryFilters().FirstOrDefaultAsync(x=>x.Id==id&&x.IsDeleted,ct)??throw new KeyNotFoundException();if(await context.Categories.AnyAsync(c=>c.Slug==x.Slug,ct))throw new InvalidOperationException("تعذر الاستعادة لأن الرابط مستخدم حاليًا.");if(x.ParentCategoryId.HasValue&&!await context.Categories.AnyAsync(c=>c.Id==x.ParentCategoryId,ct))throw new InvalidOperationException("استعد التصنيف الأب أولًا.");UnDelete(x);}
    private async Task RestoreCoupon(Guid id,CancellationToken ct){var x=await context.Coupons.IgnoreQueryFilters().FirstOrDefaultAsync(x=>x.Id==id&&x.IsDeleted,ct)??throw new KeyNotFoundException();if(await context.Coupons.AnyAsync(c=>c.Code==x.Code,ct))throw new InvalidOperationException("تعذر الاستعادة لأن كود الكوبون مستخدم حاليًا.");UnDelete(x);}
    private async Task Restore<T>(Guid id,CancellationToken ct) where T:BaseEntity{var x=await context.Set<T>().IgnoreQueryFilters().FirstOrDefaultAsync(x=>x.Id==id&&x.IsDeleted,ct)??throw new KeyNotFoundException();UnDelete(x);}
    private static RecycleBinItemViewModel Item(Guid id,string type,string label,string name,string? detail,DateTimeOffset? deletedAt)=>new(){Id=id,EntityType=type,Name=name,Detail=$"{label} · {detail}",DeletedAt=deletedAt};
    private static void UnDelete(BaseEntity entity){entity.IsDeleted=false;entity.DeletedAt=null;}
}
