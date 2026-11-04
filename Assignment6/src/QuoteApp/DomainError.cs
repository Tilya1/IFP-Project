namespace QuoteApp;

public sealed record DomainError(string Code, string Message);

public static class Errors
{
    public static DomainError InvalidQuantity(string item, int quantity) =>
        new("INVALID_QUANTITY", $"Quantity for '{item}' must be at least 1, got {quantity}.");

    public static DomainError ItemNotFound(string id) =>
        new("ITEM_NOT_FOUND", $"Item '{id}' does not exist in the catalog.");

    public static DomainError OutOfStock(string item, int requested, int available) =>
        new("OUT_OF_STOCK", $"Item '{item}': requested {requested}, available {available}.");

    public static DomainError FileMissing(string path) =>
        new("FILE_MISSING", $"Data file '{path}' was not found.");

    public static DomainError FileUnreadable(string path, string detail) =>
        new("FILE_UNREADABLE", $"Data file '{path}' could not be read: {detail}");

    public static DomainError FileMalformed(string path, string detail) =>
        new("FILE_MALFORMED", $"Data file '{path}' has invalid content: {detail}");
}
