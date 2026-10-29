# Assignment 5 – Handling Temporal Coupling

## How to run

```
cd Checkout
dotnet run
```

## Structure

- `Checkout/OldCheckout.cs` – the original class from the task.
- `Checkout/Stages.cs` – records `Draft`, `Validated`, `Priced`, `Receipt`.
- `Checkout/CheckoutSteps.cs` – functions `Start`, `Validate`, `ApplyDiscount`, `Charge`.
- `Checkout/Program.cs` – Task 1 demo, new design with two gateways, Task 4 comments.

## Task 1 – wrong orders in the old class

- Discount applied twice -> `Charged 810.00`. Silent.
- Discount forgotten -> `Charged 1000.00`. Silent.
- Charge called twice -> `Charged 900.00` two times. Silent.
- Charge before Validate -> `InvalidOperationException: Not validated.` Noticed, but only at runtime.

So the program notices only one mistake, and the other three just charge a wrong amount.

## Task 2 – new design

Each stage is its own record. Each function takes the previous stage and returns the next one:

```
Start(decimal)              -> Draft
Validate(Draft)             -> Validated   (throws if Amount <= 0)
ApplyDiscount(Validated, %) -> Priced
Charge(Priced, gateway)     -> Receipt
```

```csharp
var draft = CheckoutSteps.Start(1000m);
var validated = CheckoutSteps.Validate(draft);
var priced = CheckoutSteps.ApplyDiscount(validated, 0.10m);
var receipt = CheckoutSteps.Charge(priced, realGateway);
```

## Task 3 – gateway

`Charge` gets the bank as a parameter `Func<decimal, string> gateway`.

- `realGateway` prints a message and returns an id like `TX-2fd2bb1a`.
- `fakeGateway` is a lambda that counts its calls in a captured variable and returns `FAKE-1`, `FAKE-2`.

Output:

```
Bank: 900.00 left the account
Receipt { TransactionId = TX-2fd2bb1a, Amount = 900.00 }
Receipt { TransactionId = FAKE-1, Amount = 900.00 }
Fake gateway calls: 1
Charge called again with the same Priced, fake gateway calls: 2
```

The same `Charge` works with both gateways.

## Task 4 – wrong sequences in the new code

- Discount twice `ApplyDiscount(priced, 0.10m)` – rejected, CS1503: cannot convert from 'Priced' to 'Validated'.
- Discount forgotten `Charge(validated, gateway)` – rejected, CS1503: cannot convert from 'Validated' to 'Priced'.
- Charge before Validate `Charge(draft, gateway)` – rejected, CS1503: cannot convert from 'Draft' to 'Priced'.
- Charge twice `Charge(priced, gateway)` two times – compiles. Still possible.

## Answers

**1. What is temporal coupling? Why is the runtime check inside Charge not enough?**

Temporal coupling is when methods only work if you call them in the right order, but nothing makes you do it.
The check in `Charge` runs only when the program is already running, and it checks only validation.
Discount twice, no discount and Charge twice are not checked at all.

**2. How do the stage types enforce the order? What can they not prevent?**

Every function accepts only the previous stage. You cannot get `Priced` without `Validated`, and you cannot get `Validated` without `Draft`, so a wrong order does not compile.
What is still possible:
- calling `Charge` two times with the same `Priced` (the fake gateway shows 2 calls);
- anyone can write `new Priced(5m)` because the record constructor is public, and skip `Validate` and `ApplyDiscount`.

**3. Why does the delegate make Charge easier to test? Which parts have no side effects?**

The bank comes in as a parameter, so in a test we pass a fake gateway. No real money moves, and we can check how many times the bank was called.
No side effects: the records, `Start`, `Validate`, `ApplyDiscount`. `Charge` itself only builds a `Receipt`; the side effect is only inside the gateway we pass in. The side effect is moved to the boundary.

**4. Why a new immutable object for every stage? What does it cost and what does it allow?**

Each step is a new state of the order, so we create a new value instead of changing the old one. The old stage never changes, so the discount cannot be applied to it again.
Cost: a new object at every step and more types to write.
It allows the compiler to check the order, makes every function easy to test with input and output, and the old stages are safe to read.
