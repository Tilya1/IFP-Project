using System;
using CallProcessing;
using Xunit;

namespace CallProcessing.Tests
{
    // Group A. Tariff correctness
    public class TariffTests
    {
        [Theory]
        [InlineData("KZ", false, 4.0, 60.00)]
        [InlineData("KZ", true, 0.5, 50.00)]
        [InlineData("US", true, 10.0, 1200.00)]
        [InlineData("DE", false, 3.0, 135.00)]
        [InlineData("XX", false, 2.0, 90.00)]
        [InlineData("KZ", true, 1.0, 45.00)]
        public void CalculateCost_ReturnsExpectedPrice(string country, bool roaming, double minutes, double expected)
        {
            CallRecord record = new CallRecord("A1", country, minutes, roaming);

            decimal cost = CallPricing.CalculateCost(in record);

            Assert.Equal((decimal)expected, cost);
        }
    }

    // Group B. Invalid inputs and boundaries
    public class ValidationTests
    {
        [Fact]
        public void Constructor_BlankRecordId_Throws()
        {
            Assert.Throws<ArgumentException>(() => new CallRecord(" ", "KZ", 1, false));
        }

        [Fact]
        public void Constructor_BlankCountry_Throws()
        {
            Assert.Throws<ArgumentException>(() => new CallRecord("B1", "", 1, false));
        }

        [Fact]
        public void Constructor_NegativeDuration_Throws()
        {
            Assert.Throws<ArgumentException>(() => new CallRecord("B1", "KZ", -1, false));
        }

        [Fact]
        public void Constructor_NaN_Throws()
        {
            Assert.Throws<ArgumentException>(() => new CallRecord("B1", "KZ", double.NaN, false));
        }

        [Fact]
        public void Constructor_Infinity_Throws()
        {
            Assert.Throws<ArgumentException>(() => new CallRecord("B1", "KZ", double.PositiveInfinity, false));
        }

        [Fact]
        public void Constructor_MoreThan10000_Throws()
        {
            Assert.Throws<ArgumentException>(() => new CallRecord("B1", "KZ", 10000.1, false));
        }

        [Fact]
        public void CalculateCost_Default_Throws()
        {
            CallRecord record = default(CallRecord);

            Assert.Throws<ArgumentException>(() => CallPricing.CalculateCost(in record));
        }

        [Fact]
        public void ZeroMinutes_RoamingKZ_IsFlatFee()
        {
            CallRecord record = new CallRecord("B2", "KZ", 0, true);

            Assert.Equal(50.00m, CallPricing.CalculateCost(in record));
        }

        [Fact]
        public void ZeroMinutes_NonRoaming_IsZero()
        {
            CallRecord record = new CallRecord("B3", "DE", 0, false);

            Assert.Equal(0.00m, CallPricing.CalculateCost(in record));
        }

        [Theory]
        [InlineData(0.999, 50.00)]
        [InlineData(1.0, 45.00)]
        public void Boundary_RoamingKZ_OneMinute(double minutes, double expected)
        {
            CallRecord record = new CallRecord("B4", "KZ", minutes, true);

            Assert.Equal((decimal)expected, CallPricing.CalculateCost(in record));
        }

        [Theory]
        [InlineData(9.999, 449.96)]
        [InlineData(10.0, 1200.00)]
        public void Boundary_RoamingUS_TenMinutes(double minutes, double expected)
        {
            CallRecord record = new CallRecord("B5", "US", minutes, true);

            Assert.Equal((decimal)expected, CallPricing.CalculateCost(in record));
        }
    }

    // Group C. Sequential-parallel agreement
    public class ParallelTests
    {
        private static CallRecord[] MakeRecords()
        {
            string[] countries = { "KZ", "US", "DE", "XX" };
            CallRecord[] records = new CallRecord[1000];
            for (int i = 0; i < records.Length; i++)
            {
                string country = countries[i % 4];
                double minutes = (i % 25) * 0.5;
                bool roaming = i % 3 == 0;
                records[i] = new CallRecord("C" + i, country, minutes, roaming);
            }
            return records;
        }

        [Fact]
        public void Parallel_EqualsSequential_100Times()
        {
            CallRecord[] records = MakeRecords();
            CallRecord[] copy = (CallRecord[])records.Clone();
            decimal sequential = CallProcessor.ProcessCallsSequential(records);

            for (int run = 0; run < 100; run++)
            {
                decimal parallel = CallProcessor.ProcessCallsParallel(records);
                Assert.Equal(sequential, parallel);
            }

            Assert.Equal(copy, records);
        }

        [Fact]
        public void Parallel_EmptyArray_ReturnsZero()
        {
            Assert.Equal(0m, CallProcessor.ProcessCallsParallel(new CallRecord[0]));
        }

        [Fact]
        public void Parallel_OddLength_Throws()
        {
            CallRecord[] records = { new CallRecord("C1", "KZ", 1, false) };

            Assert.Throws<ArgumentException>(() => CallProcessor.ProcessCallsParallel(records));
        }

        [Fact]
        public void Parallel_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => CallProcessor.ProcessCallsParallel(null));
        }

        [Fact]
        public void Parallel_ErrorInSecondWorker_Throws()
        {
            CallRecord[] records =
            {
                new CallRecord("C1", "KZ", 1, false),
                default(CallRecord)
            };

            Assert.Throws<ArgumentException>(() => CallProcessor.ProcessCallsParallel(records));
        }
    }
}
