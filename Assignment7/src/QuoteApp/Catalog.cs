namespace QuoteApp;

public sealed class Catalog
{
    private readonly Dictionary<string, Item> _items;

    public Catalog(IEnumerable<Item> items)
    {
        Items = items.ToArray();
        _items = Items.ToDictionary(item => item.Id, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<Item> Items { get; }

    public int Count => Items.Count;

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
