using System.Runtime.InteropServices;

namespace QuoteApp;

internal static class NativeMethods
{
    [DllImport("quotenative", EntryPoint = "qn_discounted_total", CallingConvention = CallingConvention.Cdecl)]
    internal static extern long DiscountedTotal(long unitPriceCents, int quantity, int discountPercent);
}

public sealed class NativeTotalAdapter
{
    private readonly Func<long, int, int, long> _call;

    public NativeTotalAdapter() : this(NativeMethods.DiscountedTotal) { }

    public NativeTotalAdapter(Func<long, int, int, long> call) => _call = call;

    public Result<decimal> Total(decimal unitPrice, int quantity, int discountPercent)
    {
        if (unitPrice < 0 || decimal.Round(unitPrice, 2) != unitPrice || unitPrice > 1_000_000m)
            return Result<decimal>.Fail(Errors.InvalidPrice(unitPrice));
        if (quantity < 1 || quantity > 10_000)
            return Result<decimal>.Fail(Errors.InvalidQuantity("item", quantity));
        if (discountPercent < 0 || discountPercent > 100)
            return Result<decimal>.Fail(Errors.InvalidDiscount(discountPercent));

        var cents = (long)(unitPrice * 100);

        try
        {
            var totalCents = _call(cents, quantity, discountPercent);
            return totalCents < 0
                ? Result<decimal>.Fail(Errors.NativeRejected(totalCents))
                : Result<decimal>.Ok(totalCents / 100m);
        }
        catch (Exception e) when (e is DllNotFoundException or EntryPointNotFoundException or BadImageFormatException)
        {
            return Result<decimal>.Fail(Errors.NativeUnavailable(e.Message));
        }
    }
}
