namespace GoEDataParserTest;

public class CsvParserTests
{
    string appPath = Base.AppDirectory();

    List<Charging.Charge> charges = [];
    Charging.Parser.CsvParser parser;

    [SetUp]
    public void Setup()
    {
        parser = new();
    }

    public void Initialize(string filename)
    {
        string filepath = String.Join("/", appPath, "../fixtures/csv", filename);

        parser.Parse(filepath);
        charges = parser.GetCharges();
    }

    [TestCase("not_existing_file", 0, 0.0F)]
    [TestCase("invalid_file.csv", 0, 0.0F)]
    [TestCase("empty_file.csv", 0, 0.0F)]
    [TestCase("no_entry.csv", 0, 0.0F)]
    [TestCase("one_entry.csv", 1, 0.071F)]
    [TestCase("20250124_part.csv", 18, 317.716F)]
    [TestCase("20250124_goe.csv", 256, 3474.874F)]
    public void CountRowsTest(string filename, int expectedCount, float expectedSum)
    {
        Initialize(filename);

        Assert.That(charges, Has.Count.EqualTo(expectedCount));
        Assert.That(charges.Sum(item => item.kwh), Is.EqualTo(expectedSum));
    }

    [Test]
    public void CsvParserTestRealFile()
    {
        Initialize("20250124_goe.csv");

        Assert.Multiple(() =>
        {
            Assert.That(charges[0].kwh, Is.EqualTo(0.071F));
            Assert.That(charges[254].kwh, Is.EqualTo(10.001F));
            Assert.That(charges, Has.Count.EqualTo(256));
        });
    }
}
