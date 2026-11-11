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

    public static DomainError FileUnwritable(string path, string detail) =>
        new("FILE_UNWRITABLE", $"Report file '{path}' could not be written: {detail}");

    public static DomainError InvalidPrice(decimal price) =>
        new("INVALID_PRICE", $"Price must be >= 0 with at most 2 decimals, got {price}.");

    public static DomainError InvalidDiscount(int percent) =>
        new("INVALID_DISCOUNT", $"Discount must be from 0 to 100, got {percent}.");

    public static DomainError NativeRejected(long code) =>
        new("NATIVE_REJECTED", $"Native library rejected the input (code {code}).");

    public static DomainError NativeUnavailable(string detail) =>
        new("NATIVE_UNAVAILABLE", $"Native library 'quotenative' could not be used: {detail}");
}
