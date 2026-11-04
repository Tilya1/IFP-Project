namespace QuoteApp;

public abstract class Result<T>
{
    private Result() { }

    public static Result<T> Ok(T value)
    {
        if (value is null) throw new ArgumentNullException(nameof(value));
        return new Success(value);
    }

    public static Result<T> Fail(DomainError error) => new Failure(error);

    public abstract U Match<U>(Func<T, U> onOk, Func<DomainError, U> onFail);

    private sealed class Success : Result<T>
    {
        private readonly T _value;
        public Success(T value) => _value = value;
        public override U Match<U>(Func<T, U> onOk, Func<DomainError, U> onFail) => onOk(_value);
    }

    private sealed class Failure : Result<T>
    {
        private readonly DomainError _error;
        public Failure(DomainError error) => _error = error;
        public override U Match<U>(Func<T, U> onOk, Func<DomainError, U> onFail) => onFail(_error);
    }
}

public static class ResultExtensions
{
    public static Result<U> Map<T, U>(this Result<T> result, Func<T, U> function) =>
        result.Match<Result<U>>(
            value => Result<U>.Ok(function(value)),
            error => Result<U>.Fail(error));

    public static Result<U> Bind<T, U>(this Result<T> result, Func<T, Result<U>> function) =>
        result.Match<Result<U>>(
            value => function(value),
            error => Result<U>.Fail(error));
}
