namespace GoEDataParserTest;

public class CsvParserTests
{
    string appPath = System.AppDomain.CurrentDomain.BaseDirectory;

    List<Charging.Charge> charges;

    [SetUp]
    public void Setup()
    {
        string filepath = appPath + "/../../../fixtures/20250124_goe.csv";
        Charging.CsvParser parser = new();
        parser.parse(filepath);

        charges = parser.GetCharges();
    }

    [Test]
    public void CsvParserTest1()
    {
        Assert.Multiple(() =>
        {
            Assert.That(charges[0].kwh, Is.EqualTo(0.071F));
            Assert.That(charges[254].kwh, Is.EqualTo(10.001F));
            Assert.That(charges, Has.Count.EqualTo(256));
        });

        return;
    }
}
