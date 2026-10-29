using System;

namespace Checkout
{
    public class OldCheckout
    {
        private decimal _total;
        private bool _validated;

        public void Start(decimal amount) { _total = amount; }

        public void Validate() { _validated = _total > 0; }

        public void ApplyDiscount(decimal percent) { _total -= _total * percent; }

        public void Charge()
        {
            if (!_validated) throw new InvalidOperationException("Not validated.");
            Console.WriteLine($"Charged {_total:F2}");
        }
    }
}
