using System.Globalization;

namespace GoEDataParserTest;

public class MongoStoreTest
{
    Charging.Store.MongoStore store;

    [SetUp]
    public void Setup()
    {
        store = new();
        store.Clear();
    }

    // Insert
    [Test]
    public void InsertSimple()
    {
        Charging.Charge charge = new()
        {
            session_id = "212234_1685716486",
            kwh = 10.123F,
            start_time = DateTime.Parse("2025-01-01T09:01:02Z"),
            end_time = DateTime.Parse("2025-01-01T11:05:47Z"),
        };

        store.Insert(charge);

        Assert.That(store.Count(), Is.EqualTo(1));
    }

    [TestCase("212234_1685716486", "212234_1685716486", true)]
    [TestCase("212234_1685716486", "non-existing-id", false)]
    public void FindBySessionId(string sessionId, string searchId, bool findIt)
    {
        Charging.Charge charge = new()
        {
            session_id = sessionId,
            kwh = 10.123F,
            start_time = new DateTime(2025, 01, 01, 09, 01, 02, DateTimeKind.Utc),
            end_time = new DateTime(2025, 01, 01, 11, 05, 47, DateTimeKind.Utc),
        };

        store.Insert(charge);

        if (findIt)
        {
            Assert.That(store.FindBySessionId(searchId), Is.EqualTo(charge));
        }
        else
        {
            Assert.That(store.FindBySessionId(searchId), Is.Null);
        }
    }
    // First
}
