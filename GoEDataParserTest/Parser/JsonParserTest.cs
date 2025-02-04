using RichardSzalay.MockHttp;

namespace GoEDataParserTest;

public class JsonParserTests
{
    private string mock_url = "https://data.v3.go-e.io/api/v1/direct_json";
    private string media_type = "application/json";

    private MockHttpMessageHandler mockHttp;

    private List<Charging.Charge> charges = [];

    [SetUp]
    public void Setup()
    {
        mockHttp = new();
    }

    public void Initialize(string json_data)
    {
        _ = mockHttp.When(mock_url).Respond(media_type, json_data);
        HttpClient client = mockHttp.ToHttpClient();

        Charging.JsonParser parser = new(client);
        parser.load();
        charges = parser.GetCharges();
    }

    [Test]
    public void JsonParserTestSimple()
    {
        string json_data =
            /*lang=json,strict*/@"{""columns"":[], ""data"":[
                {""start"":""02.06.2023 16:34:46"",""end"":""02.06.2023 16:36:00"",""energy"":0.071},
                {""start"":""05.06.2023 10:34:00"",""end"":""05.06.2023 12:15:00"",""energy"":10.012}
                ]}";

        Initialize(json_data);

        Assert.Multiple(() =>
        {
            Assert.That(charges, Has.Count.EqualTo(2));
            Assert.That(charges[0].kwh, Is.EqualTo(0.071F));
            Assert.That(charges[1].kwh, Is.EqualTo(10.012F));
        });
    }

    [TestCase("data.json", 259, 0.071F)]
    [TestCase("one_data.json", 1, 6.535F)]
    [TestCase("no_data.json", 0, null)]
    [TestCase("no_columns.json", 0, null)]
    [TestCase("invalid.json", 0, null)]
    public void JsonParserFull(string filename, int expectedCount, float? expectedFirstValue)
    {
        string filepath = String.Join("/", Base.AppDirectory(), "../fixtures/json", filename);
        string json_data = File.ReadAllText(filepath);

        Initialize(json_data);

        Assert.Multiple(() =>
        {
            Assert.That(charges, Has.Count.EqualTo(expectedCount));
            if (expectedFirstValue is not null)
            {
                Assert.That(charges[0].kwh, Is.EqualTo(expectedFirstValue));
            }
        });
    }
}
