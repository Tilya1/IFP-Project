using System.Globalization;

namespace QuoteApp;

public static class QuoteService
{
    public static Result<string> Create(Catalog catalog, QuoteRequest request) =>
        FindItem(catalog, request.ItemId)
            .Bind(item => ValidateQuantity(item, request.Quantity))
            .Bind(item => CheckStock(item, request.Quantity))
            .Map(item => BuildQuote(item, request))
            .Map(quote => Format(quote));

    private static Result<Item> FindItem(Catalog catalog, string id) =>
        catalog.Find(id).ToResult(Errors.ItemNotFound(id));

    private static Result<Item> ValidateQuantity(Item item, int quantity) =>
        quantity >= 1
            ? Result<Item>.Ok(item)
            : Result<Item>.Fail(Errors.InvalidQuantity(item.Name, quantity));

    private static Result<Item> CheckStock(Item item, int quantity) =>
        item.Stock >= quantity
            ? Result<Item>.Ok(item)
            : Result<Item>.Fail(Errors.OutOfStock(item.Name, quantity, item.Stock));

    private static Quote BuildQuote(Item item, QuoteRequest request)
    {
        var discount = DiscountFor(request.Coupon);
        var total = Math.Round(item.Price * request.Quantity * (100 - discount) / 100m, 2);
        return new Quote(item.Name, request.Quantity, discount, total);
    }

    private static int DiscountFor(string coupon) =>
        string.IsNullOrWhiteSpace(coupon)
            ? 0
            : Coupons.Find(coupon).Match(percent => percent, () => 0);

    private static string Format(Quote quote) =>
        $"{quote.Quantity} x {quote.ItemName}, discount {quote.DiscountPercent}%, " +
        $"total {quote.Total.ToString("F2", CultureInfo.InvariantCulture)}";
}
