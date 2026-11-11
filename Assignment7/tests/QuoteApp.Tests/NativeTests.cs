using QuoteApp;
using static QuoteApp.Tests.TestHelpers;

namespace QuoteApp.Tests;

public class NativeTests
{
    [Fact]
    public void RealNativeCall_MatchesManualValue()
    {
        var adapter = new NativeTotalAdapter();

        Assert.Equal(1800.00m, Value(adapter.Total(1000m, 2, 10)));
        Assert.Equal(68.85m, Value(adapter.Total(25.50m, 3, 10)));
        Assert.Equal(2.83m, Value(adapter.Total(3.33m, 1, 15)));
    }

    [Fact]
    public void LibraryNotLoaded_ReturnsResultFailure()
    {
        var adapter = new NativeTotalAdapter((price, quantity, discount) =>
            throw new DllNotFoundException("quotenative not found"));

        var error = Error(adapter.Total(10m, 1, 0));

        Assert.Equal("NATIVE_UNAVAILABLE", error.Code);
    }

    [Fact]
    public void NegativeNativeCode_ReturnsRejected()
    {
        var adapter = new NativeTotalAdapter((price, quantity, discount) => -2);

        var error = Error(adapter.Total(10m, 1, 0));

        Assert.Equal("NATIVE_REJECTED", error.Code);
    }

    [Theory]
    [InlineData(-1, 1, 0, "INVALID_PRICE")]
    [InlineData(1.005, 1, 0, "INVALID_PRICE")]
    [InlineData(10, 0, 0, "INVALID_QUANTITY")]
    [InlineData(10, 1, 101, "INVALID_DISCOUNT")]
    public void InvalidInput_IsRejectedBeforeNativeCall(double price, int quantity, int discount, string code)
    {
        var calls = 0;
        var adapter = new NativeTotalAdapter((p, q, d) => { calls++; return 0; });

        var error = Error(adapter.Total((decimal)price, quantity, discount));

        Assert.Equal(code, error.Code);
        Assert.Equal(0, calls);
    }
}
