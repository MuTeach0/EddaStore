using EddaStore.Models.Products;

namespace EddaStore.Models.ViewModels;

public class HomeViewModel
{
    public IReadOnlyList<Category> Categories { get; set; }
        = Array.Empty<Category>();

    public IReadOnlyList<Product> FeaturedProducts { get; set; }
        = Array.Empty<Product>();
}