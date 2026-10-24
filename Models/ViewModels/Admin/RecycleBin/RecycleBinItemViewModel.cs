namespace EddaStore.Models.ViewModels.Admin.RecycleBin;

public class RecycleBinItemViewModel
{
    public Guid Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Detail { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
