using System;

namespace FunctionalTicketCalculator
{
    enum TicketType
    {
        Standard,
        Vip
    }

    enum DayType
    {
        Weekday,
        Weekend
    }

    class Program
    {
        static void Main()
        {
            Console.WriteLine("Ticket Price Calculator");
            Console.WriteLine();

            // 1. Base price
            Console.Write("Base price: ");
            string priceText = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(priceText))
            {
                Console.WriteLine("Error: base price is missing.");
                return;
            }
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

            // 2. Age
            Console.Write("Age: ");
            string ageText = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(ageText))
            {
                Console.WriteLine("Error: age is missing.");
                return;
            }
            int age;
            if (!int.TryParse(ageText, out age))
            {
                Console.WriteLine("Error: age must be a whole number.");
                return;
            }
            if (age < 0)
            {
                Console.WriteLine("Error: age cannot be negative.");
                return;
            }

            // 3. Student
            Console.Write("Student (true/false): ");
            string studentText = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(studentText))
            {
                Console.WriteLine("Error: student status is missing.");
                return;
            }
            bool isStudent;
            if (!bool.TryParse(studentText, out isStudent))
            {
                Console.WriteLine("Error: student status must be true or false.");
                return;
            }

            // 4. Ticket type
            Console.Write("Ticket type (Standard/Vip): ");
            string ticketText = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(ticketText))
            {
                Console.WriteLine("Error: ticket type is missing.");
                return;
            }
            TicketType ticketType;
            if (!Enum.TryParse<TicketType>(ticketText, true, out ticketType)
                || !Enum.IsDefined(typeof(TicketType), ticketType))
            {
                Console.WriteLine("Error: ticket type must be Standard or Vip.");
                return;
            }

            // 5. Day type
            Console.Write("Day type (Weekday/Weekend): ");
            string dayText = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(dayText))
            {
                Console.WriteLine("Error: day type is missing.");
                return;
            }
            DayType dayType;
            if (!Enum.TryParse<DayType>(dayText, true, out dayType)
                || !Enum.IsDefined(typeof(DayType), dayType))
            {
                Console.WriteLine("Error: day type must be Weekday or Weekend.");
                return;
            }

            decimal finalPrice = CalculateFinalPrice(basePrice, age, isStudent, ticketType, dayType);

            Console.WriteLine();
            Console.WriteLine("Final price: " + finalPrice.ToString("F2"));
        }

        // Higher-order function: it receives another function (rule) and applies it to the price.
        static decimal ApplyRule(decimal price, Func<decimal, decimal> rule) => rule(price);

        // Customer discount. The first rule that matches is used.
        static decimal GetCustomerFactor(int age, bool isStudent)
        {
            if (age < 6)
            {
                return 0.00m;
            }
            else if (age <= 12)
            {
                return 0.50m;
            }
            else if (isStudent)
            {
                return 0.85m;
            }
            else if (age >= 60)
            {
                return 0.70m;
            }
            else
            {
                return 1.00m;
            }
        }

        static decimal GetTicketFactor(TicketType ticketType)
        {
            switch (ticketType)
            {
                case TicketType.Vip:
                    return 1.25m;
                default:
                    return 1.00m;
            }
        }

        static decimal GetDayFactor(DayType dayType) => dayType == DayType.Weekend ? 1.10m : 1.00m;

        static decimal CalculateFinalPrice(decimal basePrice, int age, bool isStudent, TicketType ticketType, DayType dayType)
        {
            Func<decimal, decimal> customerRule = price => price * GetCustomerFactor(age, isStudent);
            Func<decimal, decimal> ticketRule = price => price * GetTicketFactor(ticketType);
            Func<decimal, decimal> dayRule = price => price * GetDayFactor(dayType);

            decimal afterCustomer = ApplyRule(basePrice, customerRule);
            decimal afterTicket = ApplyRule(afterCustomer, ticketRule);
            decimal afterDay = ApplyRule(afterTicket, dayRule);

            if (afterDay < 0)
            {
                afterDay = 0;
            }

            return Math.Round(afterDay, 2);
        }
    }
}
