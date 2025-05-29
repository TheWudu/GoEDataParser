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

        _parser.ReadFile(filepath);
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
            Assert.Equal(14108, _consumptions.Count);
            Assert.Equal(0.001, _consumptions[0].Kwh);
            Assert.Equal(0.005, _consumptions[14107].Kwh);
            Assert.Equal(893.21799999990776, _consumptions.Sum(c => c.Kwh));
        });
    }
}
