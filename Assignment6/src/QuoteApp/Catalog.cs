namespace QuoteApp;

public sealed class Catalog
{
    private readonly Dictionary<string, Item> _items;

    public Catalog(IEnumerable<Item> items)
    {
        _items = items.ToDictionary(item => item.Id, StringComparer.OrdinalIgnoreCase);
    }

    public int Count => _items.Count;

    public Maybe<Item> Find(string id) =>
        _items.TryGetValue(id, out var item)
            ? Maybe<Item>.Some(item)
            : Maybe<Item>.None();
}

public static class Coupons
{
    private static readonly Dictionary<string, int> Percents =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["SAVE10"] = 10,
            ["NONE"] = 0
        };

    public static Maybe<int> Find(string code) =>
        Percents.TryGetValue(code, out var percent)
            ? Maybe<int>.Some(percent)
            : Maybe<int>.None();
}
