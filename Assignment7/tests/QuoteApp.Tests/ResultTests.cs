using QuoteApp;
using static QuoteApp.Tests.TestHelpers;

namespace QuoteApp.Tests;

public class ResultTests
{
    private static readonly DomainError Boom = new("BOOM", "failure");

    [Fact]
    public void Map_OnSuccess_TransformsValue()
    {
        var result = Result<int>.Ok(2).Map(x => x * 10);

        Assert.Equal(20, Value(result));
    }

    [Fact]
    public void Bind_OnSuccess_ReturnsNextResult()
    {
        var next = Result<int>.Ok(2).Bind(x => Result<string>.Ok("v" + x));
        var failed = Result<int>.Ok(2).Bind(x => Result<string>.Fail(Boom));

        Assert.Equal("v2", Value(next));
        Assert.Equal("BOOM", Error(failed).Code);
    }

    [Fact]
    public void Map_OnFailure_SkipsFunction()
    {
        var calls = 0;

        var result = Result<int>.Fail(Boom).Map(x =>
        {
            calls++;
            return x * 2;
        });

        Assert.Equal(0, calls);
        Assert.Equal("BOOM", Error(result).Code);
    }

    [Fact]
    public void Bind_OnFailure_SkipsFunction()
    {
        var calls = 0;

        var result = Result<int>.Fail(Boom).Bind(x =>
        {
            calls++;
            return Result<int>.Ok(x);
        });

        Assert.Equal(0, calls);
        Assert.Equal("BOOM", Error(result).Code);
    }

    [Fact]
    public void Maybe_ZeroFalseAndEmptyString_ArePresent()
    {
        Assert.Equal(0, Maybe<int>.Some(0).Match(v => v, () => -1));
        Assert.False(Maybe<bool>.Some(false).Match(v => v, () => true));
        Assert.Equal(0, Maybe<string>.Some("").Match(v => v.Length, () => -1));
    }

    [Fact]
    public void Maybe_None_IsAbsent()
    {
        var result = Maybe<int>.None().Match(v => "present", () => "absent");

        Assert.Equal("absent", result);
    }
}
