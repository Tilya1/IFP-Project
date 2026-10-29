using System;

namespace Checkout
{
    public static class CheckoutSteps
    {
        public static Draft Start(decimal amount) => new Draft(amount);

        public static Validated Validate(Draft draft)
        {
            if (draft.Amount <= 0)
                throw new ArgumentException("Amount must be greater than 0.");

            return new Validated(draft.Amount);
        }

        public static Priced ApplyDiscount(Validated validated, decimal percent) =>
            new Priced(validated.Amount - validated.Amount * percent);

        public static Receipt Charge(Priced priced, Func<decimal, string> gateway) =>
            new Receipt(gateway(priced.Amount), priced.Amount);
    }
}
