using System;

namespace CallProcessing
{
    class Program
    {
        static void Main()
        {
            CallRecord[] records =
            {
                new CallRecord("1", "KZ", 4, false),
                new CallRecord("2", "KZ", 0.5, true),
                new CallRecord("3", "US", 10, true),
                new CallRecord("4", "DE", 3, false)
            };

            foreach (CallRecord record in records)
            {
                Console.WriteLine(record.RecordId + " " + record.DestinationCountry + " " + record.DurationMinutes
                    + " min, roaming: " + record.IsRoaming + " -> " + CallPricing.CalculateCost(in record));
            }

            Console.WriteLine();
            Console.WriteLine("Sequential total: " + CallProcessor.ProcessCallsSequential(records));
            Console.WriteLine("Parallel total:   " + CallProcessor.ProcessCallsParallel(records));
        }
    }
}
