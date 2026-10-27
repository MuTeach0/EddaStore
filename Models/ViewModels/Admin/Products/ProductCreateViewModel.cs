
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace EddaStore.Models.ViewModels.Admin.Products;

public class ProductCreateViewModel
{
    [Required(ErrorMessage = "اسم المنتج مطلوب.")]
    [StringLength(200, ErrorMessage = "اسم المنتج لا يتجاوز 200 حرف.")]
    [Display(Name = "اسم المنتج")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "رابط المنتج مطلوب.")]
    [StringLength(220, ErrorMessage = "رابط المنتج لا يتجاوز 220 حرف.")]
    [RegularExpression(@"^[a-zA-Z0-9-]+$",
        ErrorMessage = "الرابط يسمح بالحروف الإنجليزية والأرقام والشرطة فقط.")]
    [Display(Name = "رابط المنتج")]
    public string Slug { get; set; } = string.Empty;

    [StringLength(4000, ErrorMessage = "الوصف طويل جدًا.")]
    [Display(Name = "الوصف")]
    public string? Description { get; set; }

    [Range(typeof(decimal), "0.01", "9999999999999999",
        ErrorMessage = "السعر يجب أن يكون أكبر من صفر.")]
    [Display(Name = "السعر")]
    public decimal Price { get; set; }

    [Range(typeof(decimal), "0", "9999999999999999",
        ErrorMessage = "السعر قبل الخصم لا يمكن أن يكون سالبًا.")]
    [Display(Name = "السعر قبل الخصم")]
    public decimal? CompareAtPrice { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "المخزون لا يمكن أن يكون سالبًا.")]
    [Display(Name = "الكمية المتاحة")]
    public int StockQuantity { get; set; }

    [StringLength(100, ErrorMessage = "SKU لا يتجاوز 100 حرف.")]
    [Display(Name = "رمز المنتج SKU")]
    public string? SKU { get; set; }

    [Display(Name = "المنتج نشط")]
    public bool IsActive { get; set; } = true;

    [Required(ErrorMessage = "اختر التصنيف.")]
    [Display(Name = "التصنيف")]
    public Guid? CategoryId { get; set; }

    [Required(ErrorMessage = "اختر الماركة.")]
    [Display(Name = "الماركة")]
    public Guid? BrandId { get; set; }

    [ValidateNever]
    public IEnumerable<SelectListItem> Categories { get; set; }
        = Array.Empty<SelectListItem>();

    [ValidateNever]
    public IEnumerable<SelectListItem> Brands { get; set; }
        = Array.Empty<SelectListItem>();

    [ValidateNever]
    [Display(Name = "صور المنتج")]
    public IReadOnlyList<IFormFile>? NewImages { get; set; } = Array.Empty<IFormFile>();

    // Unified primary-image selector. Values: "new:<index>".
    [ValidateNever]
    [Display(Name = "الصورة الرئيسية")]
    public string? PrimaryImageKey { get; set; }
}
