using EddaStore.Data;
using EddaStore.Models.Products;
using EddaStore.Repositories.Interfaces;
using EddaStore.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EddaStore.Services;

public class ProductImageService(
    IProductImageRepository repository,
    IProductRepository products,
    ApplicationDbContext context,
    IWebHostEnvironment environment) : IProductImageService
{
    private static readonly HashSet<string> Allowed =
        new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };
    private const long MaxSize = 5 * 1024 * 1024;

    public Task<IReadOnlyList<ProductImage>> GetAsync(
        Guid productId, CancellationToken ct = default) =>
        repository.GetByProductIdAsync(productId, ct);

    public async Task AddAsync(
        Guid productId,
        IReadOnlyList<IFormFile> files,
        int? primaryIndex = null,
        CancellationToken ct = default)
    {
        if (files.Count == 0) return;
        if (await products.GetByIdAsync(productId, ct) is null)
            throw new KeyNotFoundException();

        foreach (var file in files) ValidateFile(file);

        if (primaryIndex.HasValue &&
            (primaryIndex.Value < 0 || primaryIndex.Value >= files.Count))
            throw new ArgumentException("اختيار الصورة الرئيسية غير صالح.");

        var dir = Path.Combine(
            environment.WebRootPath, "uploads", "products", productId.ToString("N"));
        Directory.CreateDirectory(dir);

        var current = await repository.GetByProductIdAsync(productId, ct);
        var written = new List<string>();

        try
        {
            if (primaryIndex.HasValue)
            {
                var trackedExisting = await context.ProductImages
                    .Where(x => x.ProductId == productId)
                    .ToListAsync(ct);
                foreach (var image in trackedExisting)
                    image.IsPrimary = false;
            }

            var order = current.Count;
            var entities = new List<ProductImage>();

            for (var i = 0; i < files.Count; i++)
            {
                var file = files[i];
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                var name = $"{Guid.NewGuid():N}{ext}";
                var full = Path.Combine(dir, name);

                await using (var stream = File.Create(full))
                {
                    await file.CopyToAsync(stream, ct);
                }
                written.Add(full);

                var isPrimary = primaryIndex.HasValue
                    ? i == primaryIndex.Value
                    : (!current.Any(x => x.IsPrimary) && i == 0);

                entities.Add(new ProductImage
                {
                    ProductId = productId,
                    ImageUrl = $"/uploads/products/{productId:N}/{name}",
                    SortOrder = order + i,
                    IsPrimary = isPrimary
                });
            }

            foreach (var entity in entities)
                await repository.AddAsync(entity, ct);

            await repository.SaveChangesAsync(ct);
        }
        catch
        {
            foreach (var path in written)
            {
                try { if (File.Exists(path)) File.Delete(path); }
                catch { }
            }
            throw;
        }
    }
    public async Task SetPrimaryAsync(
        Guid productId, Guid imageId, CancellationToken ct = default)
    {
        var images = await repository.GetByProductIdAsync(productId, ct);
        if (!images.Any(x => x.Id == imageId))
            throw new KeyNotFoundException();

        foreach (var image in images)
        {
            var tracked = await repository.GetAsync(image.Id, ct);
            if (tracked is not null)
                tracked.IsPrimary = tracked.Id == imageId;
        }

        await repository.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(
        Guid productId, Guid imageId, CancellationToken ct = default)
    {
        var image = await repository.GetAsync(imageId, ct);
        if (image is null || image.ProductId != productId)
            throw new KeyNotFoundException();

        repository.Delete(image);
        await repository.SaveChangesAsync(ct);

        await DeleteFileIfUnusedAsync(image.ImageUrl, ct);
    }
    public async Task ApplyEditAsync(
        Guid productId,
        IReadOnlyList<IFormFile> newFiles,
        IReadOnlyList<Guid> deletedImageIds,
        Guid? primaryExistingImageId,
        int? primaryNewImageIndex,
        CancellationToken ct = default)
    {
        if (await products.GetByIdAsync(productId, ct) is null)
            throw new KeyNotFoundException();

        deletedImageIds ??= Array.Empty<Guid>();

        foreach (var file in newFiles) ValidateFile(file);

        if (primaryNewImageIndex.HasValue &&
            (primaryNewImageIndex.Value < 0 ||
             primaryNewImageIndex.Value >= newFiles.Count))
            throw new ArgumentException("اختيار الصورة الرئيسية غير صالح.");

        var existingImages = await context.ProductImages
            .Where(x => x.ProductId == productId)
            .ToListAsync(ct);

        foreach (var id in deletedImageIds.Distinct())
        {
            if (!existingImages.Any(x => x.Id == id))
                throw new KeyNotFoundException();
        }

        if (primaryExistingImageId.HasValue)
        {
            var primaryImage = existingImages
                .FirstOrDefault(x => x.Id == primaryExistingImageId.Value);
            if (primaryImage is null)
                throw new KeyNotFoundException();

            if (deletedImageIds.Contains(primaryImage.Id))
                primaryExistingImageId = null;
        }

        var dir = Path.Combine(
            environment.WebRootPath, "uploads", "products", productId.ToString("N"));
        Directory.CreateDirectory(dir);

        var written = new List<string>();
        var newEntities = new List<ProductImage>();
        var order = existingImages.Count;

        try
        {
            for (var i = 0; i < newFiles.Count; i++)
            {
                var file = newFiles[i];
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                var name = $"{Guid.NewGuid():N}{ext}";
                var full = Path.Combine(dir, name);

                await using (var stream = File.Create(full))
                {
                    await file.CopyToAsync(stream, ct);
                }
                written.Add(full);

                newEntities.Add(new ProductImage
                {
                    ProductId = productId,
                    ImageUrl = $"/uploads/products/{productId:N}/{name}",
                    SortOrder = order + i,
                    IsPrimary = false
                });
            }
            await using var transaction =
                await context.Database.BeginTransactionAsync(ct);

            try
            {
                foreach (var entity in newEntities)
                    await repository.AddAsync(entity, ct);

                foreach (var id in deletedImageIds.Distinct())
                {
                    var image = await repository.GetAsync(id, ct);
                    if (image is not null && image.ProductId == productId)
                        repository.Delete(image);
                }

                var allImages = await context.ProductImages
                    .Where(x => x.ProductId == productId)
                    .ToListAsync(ct);

                var currentPrimaryId = allImages
                    .FirstOrDefault(x => x.IsPrimary &&
                        !deletedImageIds.Contains(x.Id))?.Id;

                foreach (var image in allImages)
                    image.IsPrimary = false;
                foreach (var entity in newEntities)
                    entity.IsPrimary = false;

                if (primaryExistingImageId.HasValue)
                {
                    var target = allImages
                        .FirstOrDefault(x => x.Id == primaryExistingImageId.Value);
                    if (target is not null)
                        target.IsPrimary = true;
                }
                else if (primaryNewImageIndex.HasValue &&
                         primaryNewImageIndex.Value >= 0 &&
                         primaryNewImageIndex.Value < newEntities.Count)
                {
                    newEntities[primaryNewImageIndex.Value].IsPrimary = true;
                }
                else if (currentPrimaryId.HasValue)
                {
                    var keep = allImages
                        .FirstOrDefault(x => x.Id == currentPrimaryId.Value);
                    if (keep is not null)
                        keep.IsPrimary = true;
                }
                else
                {
                    var firstRemaining = allImages
                        .Where(x => !deletedImageIds.Contains(x.Id))
                        .OrderBy(x => x.SortOrder)
                        .FirstOrDefault();

                    if (firstRemaining is not null)
                        firstRemaining.IsPrimary = true;
                    else if (newEntities.Count > 0)
                        newEntities[0].IsPrimary = true;
                }

                await repository.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }

            foreach (var id in deletedImageIds.Distinct())
            {
                var image = existingImages.FirstOrDefault(x => x.Id == id);
                if (image is not null)
                {
                    try { await DeleteFileIfUnusedAsync(image.ImageUrl, ct); }
                    catch { }
                }
            }
        }
        catch
        {
            foreach (var path in written)
            {
                try { if (File.Exists(path)) File.Delete(path); }
                catch { }
            }
            throw;
        }
    }

    private static void ValidateFile(IFormFile file)
    {
        if (file is null || file.Length == 0)
            throw new ArgumentException("ملف الصورة فارغ.");

        if (file.Length > MaxSize)
            throw new ArgumentException(
                "حجم الصورة يتجاوز 5MB.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!Allowed.Contains(ext))
            throw new ArgumentException(
                "نوع الصورة غير مدعوم. استخدم JPG أو PNG أو WEBP.");

        if (!file.ContentType.StartsWith(
                "image/", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException(
                "يجب رفع ملف صورة صالح.");
    }

    private async Task DeleteFileIfUnusedAsync(
        string imageUrl, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(imageUrl)) return;

        var inUse = await context.ProductImages
            .IgnoreQueryFilters()
            .AnyAsync(x =>
                x.ImageUrl == imageUrl &&
                !x.IsDeleted, ct);

        if (inUse) return;

        var path = Path.Combine(
            environment.WebRootPath,
            imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch { }
    }
}
