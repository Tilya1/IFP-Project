namespace QuoteApp;

public sealed record Item(string Id, string Name, decimal Price, int Stock);

public sealed record QuoteRequest(string ItemId, int Quantity, string Coupon = "");

public sealed record Quote(string ItemName, int Quantity, int DiscountPercent, decimal Total);
