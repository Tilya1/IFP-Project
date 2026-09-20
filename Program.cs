using System;

namespace FunctionalDeliveryCalculator
{
    enum DeliveryType
    {
        Pickup,
        Courier,
        DoorToDoor
    }

    enum DeliveryZone
    {
        City,
        OutsideCity,
        Remote
    }

    class Program
    {
        static void Main()
        {
            Console.WriteLine("Delivery Cost Calculator");
            Console.WriteLine();

            Console.Write("Base delivery price: ");
            string priceText = Console.ReadLine();
            decimal basePrice;
            if (!decimal.TryParse(priceText, out basePrice))
            {
                Console.WriteLine("Error: base price must be a number.");
                return;
            }
            if (basePrice < 0)
            {
                Console.WriteLine("Error: base price cannot be negative.");
                return;
            }

            Console.Write("Number of items: ");
            string itemsText = Console.ReadLine();
            int itemCount;
            if (!int.TryParse(itemsText, out itemCount))
            {
                Console.WriteLine("Error: number of items must be a whole number.");
                return;
            }
            if (itemCount < 1)
            {
                Console.WriteLine("Error: number of items must be 1 or more.");
                return;
            }

            Console.Write("Express delivery (true/false): ");
            string expressText = Console.ReadLine();
            bool isExpress;
            if (!bool.TryParse(expressText, out isExpress))
            {
                Console.WriteLine("Error: express delivery must be true or false.");
                return;
            }

            Console.Write("Delivery type (Pickup/Courier/DoorToDoor): ");
            string typeText = Console.ReadLine();
            DeliveryType deliveryType;
            if (!Enum.TryParse<DeliveryType>(typeText, true, out deliveryType))
            {
                Console.WriteLine("Error: delivery type must be Pickup, Courier or DoorToDoor.");
                return;
            }

            Console.Write("Delivery zone (City/OutsideCity/Remote): ");
            string zoneText = Console.ReadLine();
            DeliveryZone deliveryZone;
            if (!Enum.TryParse<DeliveryZone>(zoneText, true, out deliveryZone))
            {
                Console.WriteLine("Error: delivery zone must be City, OutsideCity or Remote.");
                return;
            }

            decimal finalPrice = CalculateFinalPrice(
                basePrice,
                itemCount,
                deliveryType,
                deliveryZone,
                isExpress);

            Console.WriteLine();
            Console.WriteLine("Final price: " + finalPrice.ToString("F2"));
        }

        static decimal ApplyRule(decimal price, Func<decimal, decimal> rule) => rule(price);

        static decimal GetItemsFactor(int itemCount)
        {
            if (itemCount >= 8)
            {
                return 1.20m;
            }
            if (itemCount >= 4)
            {
                return 1.10m;
            }
            return 1.00m;
        }

        static decimal GetTypeFactor(DeliveryType type)
        {
            switch (type)
            {
                case DeliveryType.Pickup:
                    return 0.80m;
                case DeliveryType.Courier:
                    return 1.00m;
                case DeliveryType.DoorToDoor:
                    return 1.15m;
                default:
                    return 1.00m;
            }
        }

        static decimal GetZoneFactor(DeliveryZone zone)
        {
            switch (zone)
            {
                case DeliveryZone.City:
                    return 1.00m;
                case DeliveryZone.OutsideCity:
                    return 1.25m;
                case DeliveryZone.Remote:
                    return 1.50m;
                default:
                    return 1.00m;
            }
        }

        static decimal CalculateFinalPrice(
            decimal basePrice,
            int itemCount,
            DeliveryType type,
            DeliveryZone zone,
            bool isExpress)
        {
            Func<decimal, decimal> itemsRule = price => price * GetItemsFactor(itemCount);
            Func<decimal, decimal> expressRule = price => isExpress ? price * 1.30m : price;

            decimal result = basePrice;

            result = ApplyRule(result, itemsRule);
            result = ApplyRule(result, price => price * GetTypeFactor(type));
            result = ApplyRule(result, price => price * GetZoneFactor(zone));
            result = ApplyRule(result, expressRule);

            return Math.Round(result, 2);
        }
    }
}
