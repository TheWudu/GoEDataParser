using GoEDataParser.Models;
using GoEDataParser.Parser;
using RichardSzalay.MockHttp;
using Xunit;

namespace GoEDataParserTest.Parser;

public class JsonParserTests
{
    private string _mockUrl = "https://data.v3.go-e.io/api/v1/direct_json";
    private string _mediaType = "application/json";

    private MockHttpMessageHandler _mockHttp;

    private List<Charge> _charges = [];

    public JsonParserTests()
    {
        _mockHttp = new();
    }

    internal void Initialize(string jsonData)
    {
        _ = _mockHttp.When(_mockUrl).Respond(_mediaType, jsonData);
        HttpClient client = _mockHttp.ToHttpClient();

        JsonParser parser = new(client);
        parser.Load();
        _charges = parser.GetCharges();
    }

    [Fact]
    public void JsonParserTestSimple()
    {
        string jsonData =
            /*lang=json,strict*/@"{""columns"":[], ""data"":[
                {""session_identifier"":""abcd_1234"",""start"":""02.06.2023 16:34:46"",""end"":""02.06.2023 16:36:00"",""energy"":0.071},
                {""session_identifier"":""abcd_5678"",""start"":""05.06.2023 10:34:00"",""end"":""05.06.2023 12:15:00"",""energy"":10.012}
                ]}";

        Initialize(jsonData);

        Assert.Multiple(() =>
        {
            Assert.Equal(2, _charges.Count);
            Assert.Equal(0.071, _charges[0].Kwh);
            Assert.Equal(10.012, _charges[1].Kwh);
            Assert.Equal("abcd_1234", _charges.ElementAt(0).SessionId);
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
        string jsonData = File.ReadAllText(filepath);

        Initialize(jsonData);

        Assert.Multiple(() =>
        {
            Assert.Equal(_charges.Count, expectedCount);
            if (expectedFirstValue is not null)
            {
                Assert.Equal(_charges[0].Kwh, expectedFirstValue);
            }
        });
    }
}
