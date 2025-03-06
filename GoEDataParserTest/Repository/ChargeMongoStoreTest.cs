using System.Globalization;
using Charging;
using Xunit;

namespace GoEDataParserTest;

public class MongoStoreTest
{
    Charging.ChargeMongoStore _store;

    public MongoStoreTest()
    {
        string dbHost = Charging.Configuration.MongoDbHost();
        string dbName = Charging.Configuration.MongoDbName();
        _store = new(dbHost, dbName);
        _store.Clear();
    }

    // Insert
    [Fact]
    public void InsertSimple()
    {
        Charging.Charge charge = new()
        {
            SessionId = "212234_1685716486",
            Kwh = 10.123F,
            StartTime = DateTime.Parse("2025-01-01T09:01:02Z"),
            EndTime = DateTime.Parse("2025-01-01T11:05:47Z"),
        };

        _store.Insert(charge);

        Assert.Equal(1, _store.Count());
    }

    Charging.Charge CreateCharge(int day, string? id = null)
    {
        Charging.Charge charge = new()
        {
            Id = id is not null ? id : Guid.NewGuid().ToString(),
            SessionId = "sessionId_1234_" + day.ToString(),
            Kwh = 10.123,
            StartTime = new DateTime(2025, 01, day, 09, 01, 02, DateTimeKind.Utc),
            EndTime = new DateTime(2025, 01, day, 11, 05, 47, DateTimeKind.Utc),
        };

        _store.Insert(charge);

        return charge;
    }

    [Fact]
    public void FindBySuccess()
    {
        Charging.Charge charge = CreateCharge(10);

        Assert.Equal(_store.FindBy("SessionId", "sessionId_1234_10"), charge);
    }

    [Fact]
    public void FindByNotFound()
    {
        Assert.Null(_store.FindBy("SessionId", "not-existing-id"));
    }

    [Fact]
    public void FindByStartDate()
    {
        _ = CreateCharge(10);
        Charge charge = CreateCharge(22);
        _ = CreateCharge(23);

        List<Charge> charges = _store.FindByStartDate(charge.StartTime);
        Assert.Multiple(() =>
        {
            Assert.Single(charges);
            Assert.Equal(charges.First(), charge);
        });
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void Count(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            _ = CreateCharge(i + 1);
        }
        Assert.Equal(_store.Count(), amount);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(29)]
    public void ReadAll(int amount)
    {
        List<Charging.Charge> insertedCharges = new();

        for (int i = 0; i < amount; i++)
        {
            insertedCharges.Add(CreateCharge(i + 1));
        }
        List<Charging.Charge> charges = _store.ReadAll();

        Assert.Multiple(() =>
        {
            Assert.Equal(charges.Count, amount);
            Assert.Equal(charges.ElementAt(0), insertedCharges.ElementAt(0));
            Assert.Equal(charges.ElementAt(amount - 1), insertedCharges.ElementAt(amount - 1));
        });
    }

    [Fact]
    public void UpdateExisting()
    {
        string id = "a3b28d06-f494-46ad-ac89-7927db386fc4";
        Charging.Charge charge = CreateCharge(14, id);

        charge.Kwh += 10.0F;
        charge = _store.Update(charge);

        Charging.Charge? updatedCharge = _store.Find(id);
        Assert.NotNull(charge);
        Assert.Equal(updatedCharge, charge);
    }

    [Fact]
    public void UpdateTheCorrectOne()
    {
        string id = "a3b28d06-f494-46ad-ac89-7927db386fc4";
        Charging.Charge charge = CreateCharge(14, id);
        string unchangedId = "8da64b25-5522-4ded-8c0f-ffcdd4331840";
        Charging.Charge unchanged = CreateCharge(15, unchangedId);

        charge.Kwh += 10.0F;
        charge = _store.Update(charge);

        Assert.Equal(_store.Find(id), charge);
        Assert.Equal(_store.Find(unchangedId), unchanged);
    }

    [Fact]
    public void UpdateOnlyWithProperVersion()
    {
        string id = "a3b28d06-f494-46ad-ac89-7927db386fc4";
        Charging.Charge charge = CreateCharge(14, id);

        charge.Kwh += 10.0F;
        charge.Version = 5;

        Assert.Throws<Repository.EntityNotFoundException>(
            delegate
            {
                _ = _store.Update(charge);
            }
        );
    }

    [Fact]
    public void DeleteExisting()
    {
        string id = "a3b28d06-f494-46ad-ac89-7927db386fc4";
        Charging.Charge charge = CreateCharge(14, id);

        Assert.True(_store.Delete(id));
    }

    [Fact]
    public void DeleteNonExisting()
    {
        string id = "a3b28d06-f494-46ad-ac89-7927db386fc4";

        Assert.False(_store.Delete(id));
    }

    [Fact]
    public void TestMonthly()
    {
        string id = "a3b28d06-f494-46ad-ac89-7927db386fc4";
        Charging.Charge charge = CreateCharge(14, id);

        var monthly = _store.GroupMonthly();

        Assert.Multiple(() =>
        {
            Assert.Single(monthly);
            Assert.Equal(10.123, monthly["2025.01"].KwhSum);
            Assert.False(monthly.ContainsKey("2025.02"));
        });
    }
}
