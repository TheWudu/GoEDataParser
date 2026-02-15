using GoEDataParser;
using GoEDataParser.Models;
using Xunit;

namespace GoEDataParserTest
{
    public class EvaluatorTests
    {
        List<Charge> _charges;

        public EvaluatorTests()
        {
            Console.WriteLine("Setup in EvaluatorTests");

            _charges = new()
            {
                new Charge()
                {
                    SessionId = "s1",
                    StartTime = DateTime.Parse("2024-11-20T05:08:00Z"),
                    EndTime = DateTime.Parse("2024-11-20T11:08:00Z"),
                    Kwh = 60.00F,
                },
                new Charge()
                {
                    SessionId = "s2",
                    StartTime = DateTime.Parse("2024-12-01T08:08:00Z"),
                    EndTime = DateTime.Parse("2024-12-01T10:08:00Z"),
                    Kwh = 8.00F,
                },
                new Charge()
                {
                    SessionId = "s3",
                    StartTime = DateTime.Parse("2024-12-24T12:08:00Z"),
                    EndTime = DateTime.Parse("2024-12-24T15:08:00Z"),
                    Kwh = 33.00F,
                },
                new Charge()
                {
                    SessionId = "s4",
                    StartTime = DateTime.Parse("2025-01-01T08:08:00Z"),
                    EndTime = DateTime.Parse("2025-01-01T10:08:00Z"),
                    Kwh = 12.00F,
                },
            };
        }

        [Fact]
        public async Task TestMonthly()
        {
            Evaluator evaluator = new();
            Dictionary<string, ChargeInfo> monthly = await evaluator.GroupMonthly(_charges);

            Assert.Multiple(() =>
            {
                Assert.Equal(3, monthly.Count);
                Assert.Equal(41.00F, monthly["2024.12"].KwhSum);
                Assert.False(monthly.ContainsKey("2025.02"));
            });
        }

        [Fact]
        public async Task TestYearly()
        {
            Evaluator evaluator = new();
            Dictionary<string, ChargeInfo> monthly = await evaluator.GroupYearly(_charges);

            Assert.Multiple(() =>
            {
                Assert.Equal(2, monthly.Count);
                Assert.False(monthly.ContainsKey("2022"));
                Assert.Equal(101.00F, monthly["2024"].KwhSum);
                Assert.Equal(12.0F, monthly["2025"].KwhSum);
            });
        }
    }
}
