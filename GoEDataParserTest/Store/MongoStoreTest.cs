using System.Globalization;
using Charging;
using Charging.Store;

namespace GoEDataParserTest;

public class MongoStoreTest
{
    Charging.Store.ChargeMongoStore store;

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

    Charging.Charge CreateCharge(int day, string? id = null)
    {
        Charging.Charge charge = new()
        {
            Id = id is not null ? id : Guid.NewGuid().ToString(),
            session_id = "sessionId_1234_" + day.ToString(),
            kwh = 10.123F,
            start_time = new DateTime(2025, 01, day, 09, 01, 02, DateTimeKind.Utc),
            end_time = new DateTime(2025, 01, day, 11, 05, 47, DateTimeKind.Utc),
        };

        store.Insert(charge);

        return charge;
    }

    [Test]
    public void FindBySuccess()
    {
        Charging.Charge charge = CreateCharge(10);

        Assert.That(store.FindBy("session_id", "sessionId_1234_10"), Is.EqualTo(charge));
    }

    [Test]
    public void FindByNotFound()
    {
        Assert.That(store.FindBy("session_id", "not-existing-id"), Is.Null);
    }

    [Test]
    public void FindByStartDate()
    {
        _ = CreateCharge(10);
        Charge charge = CreateCharge(22);
        _ = CreateCharge(23);

        List<Charge> charges = store.FindByStartDate(charge.start_time);
        Assert.Multiple(() =>
        {
            Assert.That(charges, Has.Count.EqualTo(1));
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

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(29)]
    public void ReadAll(int amount)
    {
        List<Charging.Charge> insertedCharges = new();

        for (int i = 0; i < amount; i++)
        {
            insertedCharges.Add(CreateCharge(i + 1));
        }
        List<Charging.Charge> charges = store.ReadAll();

        Assert.Multiple(() =>
        {
            Assert.That(charges, Has.Count.EqualTo(amount));
            Assert.That(charges.ElementAt(0), Is.EqualTo(insertedCharges.ElementAt(0)));
            Assert.That(
                charges.ElementAt(amount - 1),
                Is.EqualTo(insertedCharges.ElementAt(amount - 1))
            );
        });
    }

    [Test]
    public void UpdateExisting()
    {
        string id = "a3b28d06-f494-46ad-ac89-7927db386fc4";
        Charging.Charge charge = CreateCharge(14, id);

        charge.kwh += 10.0F;
        charge = store.Update(charge);

        Charging.Charge? updatedCharge = store.Find(id);
        Assert.That(charge, Is.Not.Null);
        Assert.That(updatedCharge, Is.EqualTo(charge));
    }

    [Test]
    public void UpdateTheCorrectOne()
    {
        string id = "a3b28d06-f494-46ad-ac89-7927db386fc4";
        Charging.Charge charge = CreateCharge(14, id);
        string unchangedId = "8da64b25-5522-4ded-8c0f-ffcdd4331840";
        Charging.Charge unchanged = CreateCharge(15, unchangedId);

        charge.kwh += 10.0F;
        charge = store.Update(charge);

        Assert.That(store.Find(id), Is.EqualTo(charge));
        Assert.That(store.Find(unchangedId), Is.EqualTo(unchanged));
    }

    [Test]
    public void UpdateOnlyWithProperVersion()
    {
        string id = "a3b28d06-f494-46ad-ac89-7927db386fc4";
        Charging.Charge charge = CreateCharge(14, id);

        charge.kwh += 10.0F;
        charge.Version = 5;

        Assert.Throws<EntityNotFoundException>(
            delegate
            {
                _ = store.Update(charge);
            }
        );
    }
}
