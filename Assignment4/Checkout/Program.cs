using System;

namespace Checkout
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Task 1: old class");

            Console.WriteLine("Correct order:");
            var c = new OldCheckout();
            c.Start(1000m); c.Validate(); c.ApplyDiscount(0.10m); c.Charge();

            Console.WriteLine("Discount applied twice:");
            var c1 = new OldCheckout();
            c1.Start(1000m); c1.Validate(); c1.ApplyDiscount(0.10m); c1.ApplyDiscount(0.10m); c1.Charge();

            Console.WriteLine("Discount forgotten:");
            var c2 = new OldCheckout();
            c2.Start(1000m); c2.Validate(); c2.Charge();

            Console.WriteLine("Charge called twice:");
            var c3 = new OldCheckout();
            c3.Start(1000m); c3.Validate(); c3.ApplyDiscount(0.10m); c3.Charge(); c3.Charge();

            Console.WriteLine("Charge before Validate:");
            try
            {
                var c4 = new OldCheckout();
                c4.Start(1000m); c4.ApplyDiscount(0.10m); c4.Charge();
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            Console.WriteLine();
            Console.WriteLine("Task 2 and 3: new design");

            Func<decimal, string> realGateway = amount =>
            {
                Console.WriteLine($"Bank: {amount:F2} left the account");
                return "TX-" + Guid.NewGuid().ToString().Substring(0, 8);
            };

            int fakeCalls = 0;
            Func<decimal, string> fakeGateway = amount =>
            {
                fakeCalls++;
                return "FAKE-" + fakeCalls;
            };

            var draft = CheckoutSteps.Start(1000m);
            var validated = CheckoutSteps.Validate(draft);
            var priced = CheckoutSteps.ApplyDiscount(validated, 0.10m);

            var realReceipt = CheckoutSteps.Charge(priced, realGateway);
            Console.WriteLine(realReceipt);

            var fakeReceipt = CheckoutSteps.Charge(priced, fakeGateway);
            Console.WriteLine(fakeReceipt);
            Console.WriteLine("Fake gateway calls: " + fakeCalls);

            CheckoutSteps.Charge(priced, fakeGateway);
            Console.WriteLine("Charge called again with the same Priced, fake gateway calls: " + fakeCalls);

            Console.WriteLine("Validate with amount 0:");
            try
            {
                CheckoutSteps.Validate(CheckoutSteps.Start(0m));
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            // Task 4
            //
            // Discount applied twice:
            // CheckoutSteps.ApplyDiscount(priced, 0.10m);
            // Rejected: CS1503 Argument 1: cannot convert from 'Checkout.Priced' to 'Checkout.Validated'
            //
            // Discount forgotten:
            // CheckoutSteps.Charge(validated, realGateway);
            // Rejected: CS1503 Argument 1: cannot convert from 'Checkout.Validated' to 'Checkout.Priced'
            //
            // Charge before Validate:
            // CheckoutSteps.Charge(draft, realGateway);
            // Rejected: CS1503 Argument 1: cannot convert from 'Checkout.Draft' to 'Checkout.Priced'
            //
            // Charge called twice:
            // CheckoutSteps.Charge(priced, realGateway);
            // CheckoutSteps.Charge(priced, realGateway);
            // Compiles. Still possible, the money leaves the account two times.
        }
    }
}
