namespace GoEDataParserTest;

public class CsvParserTests
{
    string appPath = System.AppDomain.CurrentDomain.BaseDirectory;

    [SetUp]
    public void Setup() { }

    [Test]
    public void CsvParserTest1()
    {
        string filepath = appPath + "/../../../fixtures/20250124_goe.csv";
        Charging.CsvParser parser = new();
        parser.parse(filepath);

        Assert.Multiple(() =>
        {
            Assert.That(parser.charges[0].kwh, Is.EqualTo(0.071F));
            Assert.That(parser.charges[254].kwh, Is.EqualTo(10.001F));
            Assert.That(parser.charges, Has.Count.EqualTo(256));
        });

        return;
    }
}
