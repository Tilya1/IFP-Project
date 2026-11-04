namespace QuoteApp;

public abstract class Maybe<T>
{
    private Maybe() { }

    public static Maybe<T> Some(T value)
    {
        if (value is null) throw new ArgumentNullException(nameof(value));
        return new Present(value);
    }

    public static Maybe<T> None() => new Absent();

    public abstract U Match<U>(Func<T, U> onSome, Func<U> onNone);

    private sealed class Present : Maybe<T>
    {
        private readonly T _value;
        public Present(T value) => _value = value;
        public override U Match<U>(Func<T, U> onSome, Func<U> onNone) => onSome(_value);
    }

    private sealed class Absent : Maybe<T>
    {
        public override U Match<U>(Func<T, U> onSome, Func<U> onNone) => onNone();
    }
}

public static class MaybeExtensions
{
    public static Result<T> ToResult<T>(this Maybe<T> maybe, DomainError error) =>
        maybe.Match<Result<T>>(
            value => Result<T>.Ok(value),
            () => Result<T>.Fail(error));
}
