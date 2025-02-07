using Charging;
using Xunit;

namespace GoEDataParserTest
{
    public class EvaluatorTests
    {
        List<Charge> charges;

        public EvaluatorTests()
        {
            Console.WriteLine("Setup in EvaluatorTests");

            charges = new()
            {
                new Charge()
                {
                    session_id = "s1",
                    start_time = DateTime.Parse("2024-11-20T05:08:00Z"),
                    end_time = DateTime.Parse("2024-11-20T11:08:00Z"),
                    kwh = 60.00F,
                },
                new Charge()
                {
                    session_id = "s2",
                    start_time = DateTime.Parse("2024-12-01T08:08:00Z"),
                    end_time = DateTime.Parse("2024-12-01T10:08:00Z"),
                    kwh = 8.00F,
                },
                new Charge()
                {
                    session_id = "s3",
                    start_time = DateTime.Parse("2024-12-24T12:08:00Z"),
                    end_time = DateTime.Parse("2024-12-24T15:08:00Z"),
                    kwh = 33.00F,
                },
                new Charge()
                {
                    session_id = "s4",
                    start_time = DateTime.Parse("2025-01-01T08:08:00Z"),
                    end_time = DateTime.Parse("2025-01-01T10:08:00Z"),
                    kwh = 12.00F,
                },
            };
        }

        [Fact]
        public void testMonthly()
        {
            Evaluator evaluator = new();
            Dictionary<string, ChargeInfo> monthly = evaluator.groupMonthly(charges);

            Assert.Multiple(() =>
            {
                Assert.Equal(3, monthly.Count);
                Assert.Equal(41.00F, monthly["2024.12"].kwhSum);
                Assert.False(monthly.ContainsKey("2025.02"));
            });
        }

        [Fact]
        public void testYearly()
        {
            Evaluator evaluator = new();
            Dictionary<string, ChargeInfo> monthly = evaluator.groupYearly(charges);

            Assert.Multiple(() =>
            {
                Assert.Equal(2, monthly.Count);
                Assert.False(monthly.ContainsKey("2022"));
                Assert.Equal(101.00F, monthly["2024"].kwhSum);
                Assert.Equal(12.0F, monthly["2025"].kwhSum);
            });
        }
    }
}
