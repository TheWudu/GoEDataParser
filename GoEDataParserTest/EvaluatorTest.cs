using Charging;
using RichardSzalay.MockHttp;

namespace GoEDataParserTest;

public class EvaluatorTests
{
    List<Charge> charges;

    [SetUp]
    public void Setup()
    {
        string json_data = File.ReadAllText("../../../fixtures/data.json");
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
    public void EvaluatorTest()
    {
        Evaluator evaluator = new();
        evaluator.run(charges);

        Assert.Multiple(() =>
        {
            Assert.That(charges, Has.Count.EqualTo(259));
            Assert.That(charges[0].kwh, Is.EqualTo(0.071F));
            Assert.That(charges[1].kwh, Is.EqualTo(9.874F));
        });
    }
}
