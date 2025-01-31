using Charging;
using RichardSzalay.MockHttp;

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

            string json_data = File.ReadAllText(Base.AppDirectory() + "/fixtures/json/data.json");
            string media_type = "application/json";
            string mock_url = "https://data.v3.go-e.io/api/v1/direct_json";
            MockHttpMessageHandler mockHttp = new();

            // Setup a respond for the user api (including a wildcard in the URL)
            _ = mockHttp.When(mock_url).Respond(media_type, json_data);
            HttpClient client = mockHttp.ToHttpClient();

            Charging.JsonParser parser = new(client);
            parser.load();
            charges = parser.charges;
        }

        [Test]
        public void testMonthly()
        {
            Evaluator evaluator = new();
            Dictionary<string, ChargeInfo> monthly = evaluator.groupMonthly(charges);

            Assert.Multiple(() =>
            {
                Assert.That(monthly, Has.Count.EqualTo(18));
                Assert.That(monthly["2024.12"].kwhSum, Is.EqualTo(384.706024F));
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
                Assert.That(monthly, Has.Count.EqualTo(3));
                Assert.That(monthly.ContainsKey("2022"), Is.False);
                Assert.That(monthly["2024"].kwhSum, Is.EqualTo(1852.09448F));
                Assert.That(monthly["2025"].kwhSum, Is.EqualTo(302.902008F));
            });
        }
    }
}
