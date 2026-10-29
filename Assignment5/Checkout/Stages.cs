namespace Checkout
{
    public record Draft(decimal Amount);

    public record Validated(decimal Amount);

    public record Priced(decimal Amount);

    public record Receipt(string TransactionId, decimal Amount);
}
