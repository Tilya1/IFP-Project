# FunctionalDeliveryCalculator

A console program that calculates the final delivery price.

## How to run

```
cd FunctionalDeliveryCalculator
dotnet run
```

The program asks five questions, one by one:

1. Base delivery price (a number, for example `1000`)
2. Number of items (a whole number, 1 or more)
3. Express delivery (`true` or `false`)
4. Delivery type (`Pickup`, `Courier` or `DoorToDoor`)
5. Delivery zone (`City`, `OutsideCity` or `Remote`)

If the input is wrong, the program prints an error message and stops.

## Pricing rules

The rules are applied in this order:

1. Base price
2. Number of items: 1-3 items x 1.00, 4-7 items x 1.10, 8 or more x 1.20
3. Delivery type: Pickup x 0.80, Courier x 1.00, DoorToDoor x 1.15
4. Zone: City x 1.00, OutsideCity x 1.25, Remote x 1.50
5. Express: true x 1.30, false x 1.00
6. The result is rounded to two decimal places

The task does not give a value for the `Remote` zone, so I used x 1.50.
It is written in one place, in the method `GetZoneFactor`.

## Answers to the questions

**Which parts of your program handle user input and output?**

Only the `Main` method. It is the only place where `Console.ReadLine`,
`Console.Write` and `Console.WriteLine` are used.

**Which functions perform only delivery price calculations?**

`GetItemsFactor`, `GetTypeFactor`, `GetZoneFactor`, `ApplyRule` and
`CalculateFinalPrice`. They take values as parameters and return a value.
They do not use the console and do not change anything outside themselves.

**How is `Func<...>` used to apply delivery pricing rules?**

Inside `CalculateFinalPrice` there are two variables of type
`Func<decimal, decimal>`: `itemsRule` and `expressRule`. Each of them is a
small function that takes a price and returns a new price. They are given
to the method `ApplyRule`, which calls the rule and returns the result.
The other two rules are written as lambdas directly in the `ApplyRule`
call. Because of this, every rule is applied the same way and the order of
the rules is easy to see.

**Why is `TryParse` useful when processing delivery data entered by the user?**

The user types text, and the text can be wrong: letters instead of a
number, an empty line, or a word that is not in the enum. `decimal.Parse`
would throw an exception and the program would crash. `TryParse` returns
`false` instead, so the program can print an error message and stop
normally, without an exception.

## Test cases

See `TESTCASES.md`.
