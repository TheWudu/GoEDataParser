using RichardSzalay.MockHttp;
using Xunit;

namespace GoEDataParserTest;

public class JsonParserTests
{
    private string mock_url = "https://data.v3.go-e.io/api/v1/direct_json";
    private string media_type = "application/json";

    private MockHttpMessageHandler mockHttp;

    private List<Charging.Charge> charges = [];

    public JsonParserTests()
    {
        mockHttp = new();
    }

    internal void Initialize(string json_data)
    {
        _ = mockHttp.When(mock_url).Respond(media_type, json_data);
        HttpClient client = mockHttp.ToHttpClient();

        Charging.JsonParser parser = new(client);
        parser.load();
        charges = parser.GetCharges();
    }

    [Fact]
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
            Assert.Equal(2, charges.Count);
            Assert.Equal(0.071F, charges[0].kwh);
            Assert.Equal(10.012F, charges[1].kwh);
        });
    }

    [Theory]
    [InlineData("data.json", 259, 0.071F)]
    [InlineData("one_data.json", 1, 6.535F)]
    [InlineData("no_data.json", 0, null)]
    [InlineData("no_columns.json", 0, null)]
    [InlineData("invalid.json", 0, null)]
    public void JsonParserFull(string filename, int expectedCount, float? expectedFirstValue)
    {
        string filepath = String.Join("/", Base.AppDirectory(), "../fixtures/json", filename);
        string json_data = File.ReadAllText(filepath);

        Initialize(json_data);

        Assert.Multiple(() =>
        {
            Assert.Equal(charges.Count, expectedCount);
            if (expectedFirstValue is not null)
            {
                Assert.Equal(charges[0].kwh, expectedFirstValue);
            }
        });
    }
}
