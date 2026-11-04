using System.Text.Json;

namespace QuoteApp;

public static class CatalogLoader
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    public static Result<Catalog> Load(string path)
    {
        try
        {
            var json = File.ReadAllText(path);
            var items = JsonSerializer.Deserialize<List<Item>>(json, Options);

            if (items is null)
                return Result<Catalog>.Fail(Errors.FileMalformed(path, "file contains null."));

            if (!AreValid(items))
                return Result<Catalog>.Fail(Errors.FileMalformed(
                    path, "items need unique non-empty ids, price >= 0 and stock >= 0."));

            return Result<Catalog>.Ok(new Catalog(items));
        }
        catch (Exception e) when (e is FileNotFoundException or DirectoryNotFoundException)
        {
            return Result<Catalog>.Fail(Errors.FileMissing(path));
        }
        catch (JsonException e)
        {
            return Result<Catalog>.Fail(Errors.FileMalformed(path, e.Message));
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            return Result<Catalog>.Fail(Errors.FileUnreadable(path, e.Message));
        }
    }

    private static bool AreValid(List<Item> items) =>
        items.All(item => item is not null
                          && !string.IsNullOrWhiteSpace(item.Id)
                          && item.Price >= 0
                          && item.Stock >= 0)
        && items.Select(item => item.Id.ToLowerInvariant()).Distinct().Count() == items.Count;
}

public sealed class CatalogStore
{
    public CatalogStore(Catalog initial) => Current = initial;

    public Catalog Current { get; private set; }

    public Result<Catalog> Reload(string path)
    {
        var result = CatalogLoader.Load(path);
        result.Match(catalog =>
        {
            Current = catalog;
            return true;
        }, _ => false);
        return result;
    }
}
