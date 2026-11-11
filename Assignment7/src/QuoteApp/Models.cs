namespace QuoteApp;

public sealed record Item(string Id, string Name, string Category, decimal Price, int Stock);

public sealed record QuoteRequest(string ItemId, int Quantity, string Coupon = "");

public sealed record Quote(string ItemName, int Quantity, int DiscountPercent, decimal Total);

public sealed record LowStockRow(string Name, string Category, int Stock);

public sealed record CategoryRow(string Category, int Products, int Units, decimal TotalValue);
