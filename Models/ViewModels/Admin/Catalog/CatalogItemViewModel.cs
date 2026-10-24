using System.ComponentModel.DataAnnotations;

namespace EddaStore.Models.ViewModels.Admin.Catalog;

public class CatalogItemViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "الاسم مطلوب.")]
    [StringLength(150)]
    [Display(Name = "الاسم")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "الرابط المختصر مطلوب.")]
    [StringLength(180)]
    [RegularExpression("^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "استخدم حروفًا إنجليزية صغيرة وأرقامًا وشرطة فقط.")]
    [Display(Name = "الرابط المختصر (Slug)")]
    public string Slug { get; set; } = string.Empty;

    [StringLength(1000)]
    [Display(Name = "الوصف")]
    public string? Description { get; set; }

    [Display(Name = "رابط الشعار")]
    [Url(ErrorMessage = "أدخل رابطًا صحيحًا.")]
    public string? LogoUrl { get; set; }

    [Display(Name = "التصنيف الأب")]
    public Guid? ParentCategoryId { get; set; }
}
