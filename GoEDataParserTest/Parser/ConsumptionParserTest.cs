using GoEDataParser.Models;
using GoEDataParser.Parser.Parser;
using Xunit;

namespace GoEDataParserTest.Parser;

public class ConsumptionParserTests
{
    string _appPath = Base.AppDirectory();

    List<Consumption> _consumptions = [];
    ConsumptionParser _parser;

    public ConsumptionParserTests()
    {
        _parser = new();
    }

    internal void Initialize(string filename)
    {
        string filepath = String.Join("/", _appPath, "../fixtures/csv", filename);

        _parser.Parse(filepath);
        _consumptions = _parser.GetConsumptions();
    }

    [Theory]
    [InlineData("not_existing_file", 0)]
    [InlineData("manager_invalid.csv", 0)]
    public void CountRowsTest(string filename, int expectedCount)
    {
        Initialize(filename);

        Assert.Equal(expectedCount, _consumptions.Count);
    }

    [Fact]
    public void ConsumptionParserTestRealFile()
    {
        Initialize("manager.csv");

        Assert.Multiple(() =>
        {
            Assert.Equal(0.042999999999999997, _consumptions[0].Kwh);
            Assert.Equal(0.0089999999999999993, _consumptions[63458].Kwh);
            Assert.Equal(4231.1880000017845, _consumptions.Sum(c => c.Kwh));
            Assert.Equal(63460, _consumptions.Count);
        });
    }
}
