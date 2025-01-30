namespace GoEDataParserTest;

public class CsvParserTests
{
    string appPath = System.AppDomain.CurrentDomain.BaseDirectory;

    List<Charging.Charge> charges = [];
    Charging.CsvParser parser;

    [SetUp]
    public void Setup()
    {
        parser = new();
    }

    public void Initialize(string filename)
    {
        string filepath = appPath + "/../../../fixtures/" + filename;

        parser.Parse(filepath);
        charges = parser.GetCharges();
    }

    [TestCase("no_entry.csv", 0)]
    [TestCase("one_entry.csv", 1)]
    [TestCase("20250124_goe.csv", 256)]
    public void CountRowsTest(string filename, int expected_count)
    {
        Initialize(filename);

        Assert.That(charges, Has.Count.EqualTo(expected_count));
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

        return;
    }
}
