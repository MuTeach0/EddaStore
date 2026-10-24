namespace EddaStore.Models.ViewModels.Store;
public class CartViewModel { public IReadOnlyList<CartLineViewModel> Items {get;set;}=Array.Empty<CartLineViewModel>(); public decimal Subtotal=>Items.Sum(x=>x.LineTotal); }
public class CartLineViewModel { public Guid ProductId {get;set;} public string Name {get;set;}=""; public string? ImageUrl {get;set;} public decimal UnitPrice {get;set;} public int Quantity {get;set;} public int AvailableQuantity {get;set;} public decimal LineTotal=>UnitPrice*Quantity; }
