# FunctionalDeliveryCalculator

Console-based delivery cost calculator (C# / .NET 8).

## How to run

```bash
cd FunctionalDeliveryCalculator
dotnet run
```

The program asks for five values, one per line:

1. Base delivery price (decimal, e.g. `1000` or `1250.50`)
2. Number of items (integer, at least `1`)
3. Delivery type (`Pickup`, `Courier`, `DoorToDoor`)
4. Delivery zone (`City`, `OutsideCity`, `Remote`)
5. Express delivery (`true` / `false`)

If any value is invalid, the program prints a clear error message, stops
immediately and returns exit code `1`. No exception is thrown.

## Pricing rules and order

Rules are applied strictly in this order:

| Stage | Rule | Factor |
|---|---|---|
| 1. Base price | value entered by the user | — |
| 2. Items | 1–3 items | × 1.00 |
| | 4–7 items | × 1.10 |
| | 8 or more items | × 1.20 |
| 3. Delivery type | `Pickup` | × 0.80 |
| | `Courier` | × 1.00 |
| | `DoorToDoor` | × 1.15 |
| 4. Zone | `City` | × 1.00 |
| | `OutsideCity` | × 1.25 |
| | `Remote` | × 1.50 |
| 5. Express | `true` | × 1.30 |
| | `false` | × 1.00 |
| 6. Final price | rounded to 2 decimals (`MidpointRounding.AwayFromZero`) | — |

Note: the specification does not define a coefficient for the `Remote`
zone. It is implemented as `× 1.50` and is defined in one place —
`Pricing.ZoneFactor` — so it can be changed in a single line.

## Answers to the README questions

**Which parts of your program handle user input and output?**

Only `Program.Main` and the private helper `Program.Fail`. They are the
only members that call `Console.Write`, `Console.WriteLine` or
`Console.ReadLine`. Everything else is free of I/O.

**Which functions perform only delivery price calculations?**

Everything in the static class `Pricing`: `QuantityFactor`,
`ExpressFactor`, `TypeFactor`, `ZoneFactor`, `ApplyRule`, `RoundMoney`
and `CalculateFinalPrice`. They take arguments, return a value, touch no
console and no mutable global state, so the same arguments always give
the same result.

**How is `Func<...>` used to apply delivery pricing rules?**

Two rules are stored as `Func<...>` values rather than methods:
`Func<int, decimal> QuantityFactor` and `Func<bool, decimal> ExpressFactor`.
Each pricing stage is then expressed as a lambda of type
`Func<decimal, decimal>` — a function "price in, price out" — and passed
to the higher-order function `ApplyRule(decimal price, Func<decimal, decimal> rule)`,
which applies it and returns the new price. `CalculateFinalPrice` is
therefore just four `ApplyRule` calls in the required order, which makes
the rule order explicit and lets a new rule be added as one more line.

**Why is `TryParse` useful when processing delivery data entered by the user?**

Console input is always a string and the user can type anything —
letters, an empty line, a negative number, an unknown enum name.
`decimal.Parse` / `Enum.Parse` would throw an exception in those cases and
crash the program. `TryParse` instead returns `false` and never throws, so
invalid input is an ordinary, expected branch of control flow: the
program reports what is wrong and exits early. It also lets each parser
add its own domain validation (price may not be negative, items must be
at least 1) in the same place.

## Test cases

See `TESTCASES.md` (10 cases: 6 valid, 4 invalid).
