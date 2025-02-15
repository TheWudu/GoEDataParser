using Xunit;

namespace GoEDataParserTest;

public class CsvParserTests
{
    string appPath = Base.AppDirectory();

    List<Charging.Charge> charges = [];
    Charging.Parser.CsvParser parser;

    public CsvParserTests()
    {
        parser = new();
    }

    internal void Initialize(string filename)
    {
        string filepath = String.Join("/", appPath, "../fixtures/csv", filename);

        parser.Parse(filepath);
        charges = parser.GetCharges();
    }

    [Theory]
    [InlineData("not_existing_file", 0, 0.0)]
    [InlineData("invalid_file.csv", 0, 0.0)]
    [InlineData("empty_file.csv", 0, 0.0)]
    [InlineData("no_entry.csv", 0, 0.0)]
    [InlineData("one_entry.csv", 1, 0.071)]
    [InlineData("20250124_part.csv", 18, 317.716)]
    [InlineData("20250124_goe.csv", 256, 3474.874)]
    public void CountRowsTest(string filename, int expectedCount, double expectedSum)
    {
        Initialize(filename);

        Assert.Equal(expectedCount, charges.Count);
        Assert.Equal(expectedSum, charges.Sum(item => item.Kwh), 3);
    }

    [Fact]
    public void CsvParserTestRealFile()
    {
        Initialize("20250124_goe.csv");

        Assert.Multiple(() =>
        {
            Assert.Equal(0.071, charges[0].Kwh);
            Assert.Equal(10.001, charges[254].Kwh);
            Assert.Equal(256, charges.Count);
        });
    }
}
