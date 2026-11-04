using QuoteApp;

var store = new CatalogStore(new Catalog(Array.Empty<Item>()));

Console.WriteLine("1. Load catalog");
Show(store.Reload("data/items.json").Map(catalog => $"loaded {catalog.Count} items"));

Console.WriteLine("2. Normal path");
Show(QuoteService.Create(store.Current, new QuoteRequest("LAP-1", 2, "SAVE10")));

Console.WriteLine("3. Domain failure: invalid quantity");
Show(QuoteService.Create(store.Current, new QuoteRequest("LAP-1", 0)));

Console.WriteLine("4. Domain failure: item not found");
Show(QuoteService.Create(store.Current, new QuoteRequest("XXX", 1)));

Console.WriteLine("5. Domain failure: out of stock");
Show(QuoteService.Create(store.Current, new QuoteRequest("CAB-1", 1)));

Console.WriteLine("6. Coupon lookup: present, zero, absent");
foreach (var code in new[] { "SAVE10", "NONE", "BAD" })
    Console.WriteLine("   " + Coupons.Find(code).Match(percent => $"{code}: {percent}%", () => $"{code}: not found"));

Console.WriteLine("7. File failure: missing file");
Show(store.Reload("data/missing.json").Map(catalog => $"loaded {catalog.Count} items"));

Console.WriteLine("8. File failure: malformed file");
Show(store.Reload("data/broken.json").Map(catalog => $"loaded {catalog.Count} items"));

Console.WriteLine($"Catalog after failures still has {store.Current.Count} items.");

static void Show(Result<string> result) =>
    Console.WriteLine("   " + result.Match(
        value => "OK: " + value,
        error => $"FAIL [{error.Code}] {error.Message}"));
