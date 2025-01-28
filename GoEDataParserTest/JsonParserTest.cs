using RichardSzalay.MockHttp;

namespace GoEDataParserTest;

public class JsonParserTests
{
    private string mock_url = "https://data.v3.go-e.io/api/v1/direct_json";
    private string media_type = "application/json";

    [SetUp]
    public void Setup() { }

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
    public void JsonParserTestWithMock()
    {
        MockHttpMessageHandler mockHttp = new();

        string json_data =
            /*lang=json,strict*/@"{""columns"":[], ""data"":[
                {""start"":""02.06.2023 16:34:46"",""end"":""02.06.2023 16:36:00"",""energy"":0.071}
                ]}";

        // Setup a respond for the user api (including a wildcard in the URL)
        _ = mockHttp.   When(mock_url).Respond(media_type, json_data);
        HttpClient client = mockHttp.ToHttpClient();

        Charging.JsonParser parser = new(client);
        parser.load();

        Assert.That(parser.charges, Has.Count.EqualTo(1));
        Assert.That(parser.charges[0].kwh, Is.EqualTo(0.071F));
    }
}
