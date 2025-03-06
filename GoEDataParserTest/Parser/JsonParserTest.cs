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
        parser.Load();
        charges = parser.GetCharges();
    }

    [Fact]
    public void JsonParserTestSimple()
    {
        string json_data =
            /*lang=json,strict*/@"{""columns"":[], ""data"":[
                {""session_identifier"":""abcd_1234"",""start"":""02.06.2023 16:34:46"",""end"":""02.06.2023 16:36:00"",""energy"":0.071},
                {""session_identifier"":""abcd_5678"",""start"":""05.06.2023 10:34:00"",""end"":""05.06.2023 12:15:00"",""energy"":10.012}
                ]}";

        Initialize(json_data);

        Assert.Multiple(() =>
        {
            Assert.Equal(2, charges.Count);
            Assert.Equal(0.071, charges[0].Kwh);
            Assert.Equal(10.012, charges[1].Kwh);
            Assert.Equal("abcd_1234", charges.ElementAt(0).SessionId);
        });
    }

    [Theory]
    [InlineData("data.json", 259, 0.071)]
    [InlineData("one_data.json", 1, 6.535)]
    [InlineData("no_data.json", 0, null)]
    [InlineData("no_columns.json", 0, null)]
    [InlineData("invalid.json", 0, null)]
    public void JsonParserFull(string filename, int expectedCount, double? expectedFirstValue)
    {
        string filepath = String.Join("/", Base.AppDirectory(), "../fixtures/json", filename);
        string json_data = File.ReadAllText(filepath);

        Initialize(json_data);

        Assert.Multiple(() =>
        {
            Assert.Equal(charges.Count, expectedCount);
            if (expectedFirstValue is not null)
            {
                Assert.Equal(charges[0].Kwh, expectedFirstValue);
            }
        });
    }
}
