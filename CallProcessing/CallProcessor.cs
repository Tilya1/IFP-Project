using System;
using System.Threading;

namespace CallProcessing
{
    public static class CallProcessor
    {
        public static decimal ProcessCallsSequential(CallRecord[] records)
        {
            if (records == null)
            {
                throw new ArgumentNullException(nameof(records));
            }

            decimal total = 0;
            foreach (CallRecord record in records)
            {
                total += CallPricing.CalculateCost(in record);
            }
            return total;
        }

        public static decimal ProcessCallsParallel(CallRecord[] records)
        {
            if (records == null)
            {
                throw new ArgumentNullException(nameof(records));
            }
            if (records.Length % 2 != 0)
            {
                throw new ArgumentException("Array length must be even.");
            }
            if (records.Length == 0)
            {
                return 0;
            }

            int mid = records.Length / 2;
            CallRecord[] firstHalf = records[..mid];
            CallRecord[] secondHalf = records[mid..];

            // Each worker has its own output array
            decimal[] firstResults = new decimal[firstHalf.Length];
            decimal[] secondResults = new decimal[secondHalf.Length];

            Exception firstError = null;
            Exception secondError = null;

            Thread firstThread = new Thread(() =>
            {
                try
                {
                    for (int i = 0; i < firstHalf.Length; i++)
                    {
                        firstResults[i] = CallPricing.CalculateCost(in firstHalf[i]);
                    }
                }
                catch (Exception ex)
                {
                    firstError = ex;
                }
            });

            Thread secondThread = new Thread(() =>
            {
                try
                {
                    for (int i = 0; i < secondHalf.Length; i++)
                    {
                        secondResults[i] = CallPricing.CalculateCost(in secondHalf[i]);
                    }
                }
                catch (Exception ex)
                {
                    secondError = ex;
                }
            });

            firstThread.Start();
            secondThread.Start();
            firstThread.Join();
            secondThread.Join();

            // If a worker failed, throw the error on the calling thread (no partial total)
            if (firstError != null)
            {
                throw firstError;
            }
            if (secondError != null)
            {
                throw secondError;
            }

            decimal total = 0;
            foreach (decimal cost in firstResults)
            {
                total += cost;
            }
            foreach (decimal cost in secondResults)
            {
                total += cost;
            }
            return total;
        }
    }
}
