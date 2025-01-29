using RichardSzalay.MockHttp;

namespace GoEDataParserTest;

public class JsonParserTests
{
    private string mock_url = "https://data.v3.go-e.io/api/v1/direct_json";
    private string media_type = "application/json";

    private MockHttpMessageHandler mockHttp;

    [SetUp]
    public void Setup()
    {
        mockHttp = new();
    }

    // Commented, as it would need a valid token to really
    // download the data from go-e cloud.
    // [Test]
    // public void JsonParserTestRealRequest()
    // {
    //     Charging.JsonParser parser = new(new HttpClient());
    //     parser.load();

    //     Assert.Multiple(() =>
    //     {
    //         Assert.That(parser.charges[0].kwh, Is.EqualTo(0.071F));
    //         Assert.That(parser.charges[254].kwh, Is.EqualTo(10.001F));
    //         Assert.That(parser.charges.Count, Is.GreaterThan(256));
    //     });
    // }

    [Test]
    public void JsonParserTestSimple()
    {
        string json_data =
            /*lang=json,strict*/@"{""columns"":[], ""data"":[
                {""start"":""02.06.2023 16:34:46"",""end"":""02.06.2023 16:36:00"",""energy"":0.071},
                {""start"":""05.06.2023 10:34:00"",""end"":""05.06.2023 12:15:00"",""energy"":10.012}
                ]}";

        // Setup a respond for the user api (including a wildcard in the URL)
        _ = mockHttp.When(mock_url).Respond(media_type, json_data);
        HttpClient client = mockHttp.ToHttpClient();

        Charging.JsonParser parser = new(client);
        parser.load();

        Assert.That(parser.charges, Has.Count.EqualTo(2));
        Assert.That(parser.charges[0].kwh, Is.EqualTo(0.071F));
        Assert.That(parser.charges[1].kwh, Is.EqualTo(10.012F));
    }

    [Test]
    public void JsonParserFull()
    {
        string json_data = File.ReadAllText("../../../fixtures/data.json");

        // Setup a respond for the user api (including a wildcard in the URL)
        _ = mockHttp.When(mock_url).Respond(media_type, json_data);
        HttpClient client = mockHttp.ToHttpClient();

        Charging.JsonParser parser = new(client);
        parser.load();

        Assert.Multiple(() =>
        {
            Assert.That(parser.charges, Has.Count.EqualTo(259));
            Assert.That(parser.charges[0].kwh, Is.EqualTo(0.071F));
            Assert.That(parser.charges[1].kwh, Is.EqualTo(9.874F));
        });
    }
}
