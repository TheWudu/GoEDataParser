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
    [InlineData("not_existing_file", 0, 0.0F)]
    [InlineData("invalid_file.csv", 0, 0.0F)]
    [InlineData("empty_file.csv", 0, 0.0F)]
    [InlineData("no_entry.csv", 0, 0.0F)]
    [InlineData("one_entry.csv", 1, 0.071F)]
    [InlineData("20250124_part.csv", 18, 317.716F)]
    [InlineData("20250124_goe.csv", 256, 3474.874F)]
    public void CountRowsTest(string filename, int expectedCount, float expectedSum)
    {
        Initialize(filename);

        Assert.Equal(charges.Count, expectedCount);
        Assert.Equal(charges.Sum(item => item.Kwh), expectedSum);
    }

    [Fact]
    public void CsvParserTestRealFile()
    {
        Initialize("20250124_goe.csv");

        Assert.Multiple(() =>
        {
            Assert.Equal(0.071F, charges[0].Kwh);
            Assert.Equal(10.001F, charges[254].Kwh);
            Assert.Equal(256, charges.Count);
        });
    }
}
