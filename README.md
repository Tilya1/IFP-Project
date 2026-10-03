# FunctionalTicketCalculator

A console program that calculates the final ticket price.

## How to run

```
dotnet run
```

The program asks five questions, one by one:

1. Base price (a number, for example `5000`)
2. Age (a whole number, 0 or more)
3. Student (`true` or `false`)
4. Ticket type (`Standard` or `Vip`)
5. Day type (`Weekday` or `Weekend`)

If the input is wrong or empty, the program prints an error message and stops.

## Pricing rules

1. Customer discount (the first rule that matches is used):
   - age < 6 -> free (x 0.00)
   - age 6-12 -> 50% discount (x 0.50)
   - student -> 15% discount (x 0.85)
   - age 60 or more -> 30% discount (x 0.70)
   - others -> no discount (x 1.00)
2. Ticket type: Vip x 1.25, Standard x 1.00
3. Day type: Weekend x 1.10, Weekday x 1.00
4. The price cannot be negative and is rounded to two decimal places.

## Functions in the program

- `Main` - reads input, checks it with `TryParse`, prints the result.
- `ApplyRule` - higher-order function. It receives a price and a
  `Func<decimal, decimal>` rule and returns `rule(price)`.
- `GetCustomerFactor` - returns the customer discount factor (if / else if).
- `GetTicketFactor` - returns the ticket factor (switch).
- `GetDayFactor` - expression-bodied function with `? :`.
- `CalculateFinalPrice` - creates three `Func<decimal, decimal>` values
  with lambdas (`customerRule`, `ticketRule`, `dayRule`) and applies them
  one by one with `ApplyRule`. Every step returns a new value.

## Answers to the questions

**1. Which parts of the program are imperative?**

The `Main` method. It runs step by step: ask a question, read the line,
check it, and `return` early if it is wrong. Then it calls the calculation
and prints the result.

**2. Which functions are pure?**

`ApplyRule`, `GetCustomerFactor`, `GetTicketFactor`, `GetDayFactor` and
`CalculateFinalPrice`. They only use their parameters, always return the
same result for the same input, do not use `Console` and do not change
any global variables.

**3. Where do side effects remain?**

Only in `Main`: `Console.ReadLine` (reading input) and `Console.Write` /
`Console.WriteLine` (printing). Everything else has no side effects.

**4. Why is TryParse preferred to Parse for user input?**

The user can type anything: letters, an empty line, or a wrong word.
`Parse` throws an exception and the program crashes. `TryParse` just
returns `false`, so the program can show a clear error message and stop
normally.

## Test cases

See `TESTCASES.md`.
