namespace QuoteApp;

public static class ItemExtensions
{
    public static decimal StockValue(this Item item) => item.Price * item.Stock;

    public static IEnumerable<Item> InStock(this IEnumerable<Item> items) =>
        items.Where(item => item.Stock > 0);

    public static IEnumerable<string> ToLines<T>(this IEnumerable<T> rows, Func<T, string> format)
    {
        foreach (var row in rows)
            yield return format(row);
    }
}
