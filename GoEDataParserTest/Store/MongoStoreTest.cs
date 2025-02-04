using System.Globalization;
using Charging;

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

    Charging.Charge CreateCharge(int day)
    {
        Charging.Charge charge = new()
        {
            session_id = "sessionId_1234_" + day.ToString(),
            kwh = 10.123F,
            start_time = new DateTime(2025, 01, day, 09, 01, 02, DateTimeKind.Utc),
            end_time = new DateTime(2025, 01, day, 11, 05, 47, DateTimeKind.Utc),
        };

        store.Insert(charge);

        return charge;
    }

    [Test]
    public void FindBySessionIdSuccess()
    {
        Charging.Charge charge = CreateCharge(10);

        Assert.That(store.FindBySessionId("sessionId_1234_10"), Is.EqualTo(charge));
    }

    [Test]
    public void FindBySessionIdNotFound()
    {
        Assert.That(store.FindBySessionId("not-existing-id"), Is.Null);
    }

    [Test]
    public void FindByStartDate()
    {
        CreateCharge(10);
        Charge charge = CreateCharge(22);

        List<Charge> charges = store.FindByStartDate(charge.start_time);
        Assert.Multiple(() =>
        {
            Assert.That(charges.Count(), Is.EqualTo(1));
            Assert.That(charges.First(), Is.EqualTo(charge));
        });
    }

    [TestCase(1)]
    [TestCase(5)]
    public void Count(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            _ = CreateCharge(i + 1);
        }
        Assert.That(store.Count(), Is.EqualTo(amount));
    }
}
