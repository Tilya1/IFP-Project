using System;
using System.Globalization;

namespace FunctionalDeliveryCalculator
{
    public enum DeliveryType
    {
        Pickup,
        Courier,
        DoorToDoor
    }

    public enum DeliveryZone
    {
        City,
        OutsideCity,
        Remote
    }

    public readonly struct DeliveryOrder
    {
        public decimal BasePrice { get; }
        public int ItemCount { get; }
        public bool IsExpress { get; }
        public DeliveryType Type { get; }
        public DeliveryZone Zone { get; }

        public DeliveryOrder(
            decimal basePrice,
            int itemCount,
            bool isExpress,
            DeliveryType type,
            DeliveryZone zone)
        {
            BasePrice = basePrice;
            ItemCount = itemCount;
            IsExpress = isExpress;
            Type = type;
            Zone = zone;
        }
    }

    public static class Pricing
    {
        public static readonly Func<int, decimal> QuantityFactor = itemCount =>
            itemCount >= 8 ? 1.20m :
            itemCount >= 4 ? 1.10m :
                             1.00m;

        public static readonly Func<bool, decimal> ExpressFactor = isExpress =>
            isExpress ? 1.30m : 1.00m;

        public static decimal TypeFactor(DeliveryType type) => type switch
        {
            DeliveryType.Pickup => 0.80m,
            DeliveryType.Courier => 1.00m,
            DeliveryType.DoorToDoor => 1.15m,
            _ => 1.00m
        };

        public static decimal ZoneFactor(DeliveryZone zone) => zone switch
        {
            DeliveryZone.City => 1.00m,
            DeliveryZone.OutsideCity => 1.25m,
            DeliveryZone.Remote => 1.50m,
            _ => 1.00m
        };

        public static decimal ApplyRule(decimal price, Func<decimal, decimal> rule) =>
            rule(price);

        public static decimal RoundMoney(decimal value) =>
            Math.Round(value, 2, MidpointRounding.AwayFromZero);

        public static decimal CalculateFinalPrice(DeliveryOrder order)
        {
            decimal price = order.BasePrice;

            price = ApplyRule(price, p => p * QuantityFactor(order.ItemCount));
            price = ApplyRule(price, p => p * TypeFactor(order.Type));
            price = ApplyRule(price, p => p * ZoneFactor(order.Zone));
            price = ApplyRule(price, p => p * ExpressFactor(order.IsExpress));

            return RoundMoney(price);
        }
    }

    public static class InputParser
    {
        public static bool TryParseBasePrice(string? raw, out decimal price, out string error)
        {
            price = 0m;
            error = string.Empty;

            if (string.IsNullOrWhiteSpace(raw))
            {
                error = "Base price is required. Input was empty.";
                return false;
            }

            string text = raw.Trim().Replace(',', '.');

            if (!decimal.TryParse(
                    text,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out price))
            {
                error = $"'{raw}' is not a valid decimal number.";
                price = 0m;
                return false;
            }

            if (price < 0m)
            {
                error = "Base price cannot be negative.";
                price = 0m;
                return false;
            }

            return true;
        }

        public static bool TryParseItemCount(string? raw, out int itemCount, out string error)
        {
            itemCount = 0;
            error = string.Empty;

            if (string.IsNullOrWhiteSpace(raw))
            {
                error = "Number of items is required. Input was empty.";
                return false;
            }

            if (!int.TryParse(
                    raw.Trim(),
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out itemCount))
            {
                error = $"'{raw}' is not a valid whole number.";
                itemCount = 0;
                return false;
            }

            if (itemCount < 1)
            {
                error = "Number of items must be at least 1.";
                itemCount = 0;
                return false;
            }

            return true;
        }

        public static bool TryParseExpress(string? raw, out bool isExpress, out string error)
        {
            isExpress = false;
            error = string.Empty;

            if (string.IsNullOrWhiteSpace(raw))
            {
                error = "Express delivery status is required. Enter true or false.";
                return false;
            }

            if (!bool.TryParse(raw.Trim(), out isExpress))
            {
                error = $"'{raw}' is not a valid boolean. Enter true or false.";
                isExpress = false;
                return false;
            }

            return true;
        }

        public static bool TryParseEnum<TEnum>(string? raw, out TEnum value, out string error)
            where TEnum : struct, Enum
        {
            value = default;
            error = string.Empty;

            if (string.IsNullOrWhiteSpace(raw))
            {
                error = $"{typeof(TEnum).Name} is required. Allowed values: {AllowedValues<TEnum>()}.";
                return false;
            }

            if (!Enum.TryParse<TEnum>(raw.Trim(), ignoreCase: true, out value) ||
                !Enum.IsDefined(typeof(TEnum), value))
            {
                error = $"'{raw}' is not a valid {typeof(TEnum).Name}. Allowed values: {AllowedValues<TEnum>()}.";
                value = default;
                return false;
            }

            return true;
        }

        public static string AllowedValues<TEnum>() where TEnum : struct, Enum =>
            string.Join(", ", Enum.GetNames(typeof(TEnum)));
    }

    public static class Program
    {
        public static int Main()
        {
            Console.WriteLine("=== Functional Delivery Calculator ===");
            Console.WriteLine();

            Console.Write("Base delivery price: ");
            if (!InputParser.TryParseBasePrice(Console.ReadLine(), out decimal basePrice, out string error))
            {
                return Fail(error);
            }

            Console.Write("Number of items: ");
            if (!InputParser.TryParseItemCount(Console.ReadLine(), out int itemCount, out error))
            {
                return Fail(error);
            }

            Console.Write($"Delivery type ({InputParser.AllowedValues<DeliveryType>()}): ");
            if (!InputParser.TryParseEnum(Console.ReadLine(), out DeliveryType deliveryType, out error))
            {
                return Fail(error);
            }

            Console.Write($"Delivery zone ({InputParser.AllowedValues<DeliveryZone>()}): ");
            if (!InputParser.TryParseEnum(Console.ReadLine(), out DeliveryZone deliveryZone, out error))
            {
                return Fail(error);
            }

            Console.Write("Express delivery (true/false): ");
            if (!InputParser.TryParseExpress(Console.ReadLine(), out bool isExpress, out error))
            {
                return Fail(error);
            }

            DeliveryOrder order = new DeliveryOrder(
                basePrice,
                itemCount,
                isExpress,
                deliveryType,
                deliveryZone);

            decimal finalPrice = Pricing.CalculateFinalPrice(order);

            Console.WriteLine();
            Console.WriteLine("--- Result ---");
            Console.WriteLine($"Base price     : {order.BasePrice.ToString("F2", CultureInfo.InvariantCulture)}");
            Console.WriteLine($"Items          : {order.ItemCount}");
            Console.WriteLine($"Delivery type  : {order.Type}");
            Console.WriteLine($"Delivery zone  : {order.Zone}");
            Console.WriteLine($"Express        : {order.IsExpress}");
            Console.WriteLine($"FINAL PRICE    : {finalPrice.ToString("F2", CultureInfo.InvariantCulture)}");

            return 0;
        }

        private static int Fail(string error)
        {
            Console.WriteLine();
            Console.WriteLine($"Input error: {error}");
            Console.WriteLine("Calculation stopped.");
            return 1;
        }
    }
}
