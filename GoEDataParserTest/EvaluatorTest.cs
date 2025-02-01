using Charging;

namespace GoEDataParserTest
{
    [SetUpFixture]
    public class MySetupClass
    {
        [OneTimeSetUp]
        public void RunBeforeAnyTest()
        {
            TestContext.Progress.WriteLine("RunBeforeAnyTest");
        }

        [OneTimeTearDown]
        public void RunAfterAnyTest()
        {
            TestContext.Progress.WriteLine("RunAfterAnyTest");
        }
    }

    public class EvaluatorTests
    {
        List<Charge> charges;

        [SetUp]
        public void Setup()
        {
            Console.WriteLine("Setup in EvaluatorTests");

            charges = new()
            {
                new Charge()
                {
                    start_time = DateTime.Parse("2024-11-20T05:08:00Z"),
                    end_time = DateTime.Parse("2024-11-20T11:08:00Z"),
                    kwh = 60.00F,
                },
                new Charge()
                {
                    start_time = DateTime.Parse("2024-12-01T08:08:00Z"),
                    end_time = DateTime.Parse("2024-12-01T10:08:00Z"),
                    kwh = 8.00F,
                },
                new Charge()
                {
                    start_time = DateTime.Parse("2024-12-24T12:08:00Z"),
                    end_time = DateTime.Parse("2024-12-24T15:08:00Z"),
                    kwh = 33.00F,
                },
                new Charge()
                {
                    start_time = DateTime.Parse("2025-01-01T08:08:00Z"),
                    end_time = DateTime.Parse("2025-01-01T10:08:00Z"),
                    kwh = 12.00F,
                },
            };
        }

        [Test]
        public void testMonthly()
        {
            Evaluator evaluator = new();
            Dictionary<string, ChargeInfo> monthly = evaluator.groupMonthly(charges);

            Assert.Multiple(() =>
            {
                Assert.That(monthly, Has.Count.EqualTo(3));
                Assert.That(monthly["2024.12"].kwhSum, Is.EqualTo(41.00F));
                Assert.That(monthly.ContainsKey("2025.02"), Is.False);
            });
        }

        [Test]
        public void testYearly()
        {
            Evaluator evaluator = new();
            Dictionary<string, ChargeInfo> monthly = evaluator.groupYearly(charges);

            Assert.Multiple(() =>
            {
                Assert.That(monthly, Has.Count.EqualTo(2));
                Assert.That(monthly.ContainsKey("2022"), Is.False);
                Assert.That(monthly["2024"].kwhSum, Is.EqualTo(101.00F));
                Assert.That(monthly["2025"].kwhSum, Is.EqualTo(12.0F));
            });
        }
    }
}
