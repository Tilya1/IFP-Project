using QuoteApp;
using static QuoteApp.Tests.TestHelpers;

namespace QuoteApp.Tests;

public class QuoteTests
{
    private static readonly Catalog Shop = new(new[]
    {
        new Item("LAP-1", "Laptop", "Computers", 1000m, 5),
        new Item("CAB-1", "Cable", "Accessories", 5m, 0)
    });

    private static Result<decimal> Calc(decimal price, int quantity, int discount) =>
        Result<decimal>.Ok(price * quantity * (100 - discount) / 100m);

    [Fact]
    public void Quote_Success_ReturnsExpectedText()
    {
        var result = QuoteService.Create(Shop, new QuoteRequest("LAP-1", 2, "SAVE10"), Calc);

        Assert.Equal("2 x Laptop, discount 10%, total 1800.00", Value(result));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Quote_InvalidQuantity_Fails(int quantity)
    {
        var error = Error(QuoteService.Create(Shop, new QuoteRequest("LAP-1", quantity), Calc));

        Assert.Equal("INVALID_QUANTITY", error.Code);
        Assert.Contains("Laptop", error.Message);
    }

    [Fact]
    public void Quote_MissingItem_Fails()
    {
        var error = Error(QuoteService.Create(Shop, new QuoteRequest("XXX", 1), Calc));

        Assert.Equal("ITEM_NOT_FOUND", error.Code);
        Assert.Contains("XXX", error.Message);
    }

    [Fact]
    public void Quote_ZeroStock_IsOutOfStockNotMissing()
    {
        var error = Error(QuoteService.Create(Shop, new QuoteRequest("CAB-1", 1), Calc));

        Assert.Equal("OUT_OF_STOCK", error.Code);
    }

    [Fact]
    public void Quote_MissingItem_StopsBeforeQuantityCheck()
    {
        var error = Error(QuoteService.Create(Shop, new QuoteRequest("XXX", -5), Calc));

        Assert.Equal("ITEM_NOT_FOUND", error.Code);
    }

    [Fact]
    public void Coupons_ZeroDiscountIsFound_UnknownIsAbsent()
    {
        Assert.Equal(0, Coupons.Find("NONE").Match(p => p, () => -1));
        Assert.Equal(10, Coupons.Find("SAVE10").Match(p => p, () => -1));
        Assert.Equal(-1, Coupons.Find("BAD").Match(p => p, () => -1));
    }

    [Fact]
    public void Quote_UnknownCoupon_GivesNoDiscount()
    {
        var result = QuoteService.Create(Shop, new QuoteRequest("LAP-1", 1, "BAD"), Calc);

        Assert.Equal("1 x Laptop, discount 0%, total 1000.00", Value(result));
    }
}
