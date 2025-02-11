using System.Globalization;
using Charging;
using Xunit;

namespace GoEDataParserTest;

public class MongoStoreTest
{
    Charging.ChargeMongoStore store;

    public MongoStoreTest()
    {
        string dbHost = Charging.Configuration.MongoDbHost();
        string dbName = Charging.Configuration.MongoDbName();
        store = new(dbHost, dbName);
        store.Clear();
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

        store.Insert(charge);

        Assert.Equal(1, store.Count());
    }

    Charging.Charge CreateCharge(int day, string? id = null)
    {
        Charging.Charge charge = new()
        {
            Id = id is not null ? id : Guid.NewGuid().ToString(),
            SessionId = "sessionId_1234_" + day.ToString(),
            Kwh = 10.123F,
            StartTime = new DateTime(2025, 01, day, 09, 01, 02, DateTimeKind.Utc),
            EndTime = new DateTime(2025, 01, day, 11, 05, 47, DateTimeKind.Utc),
        };

        store.Insert(charge);

        return charge;
    }

    [Fact]
    public void FindBySuccess()
    {
        Charging.Charge charge = CreateCharge(10);

        Assert.Equal(store.FindBy("SessionId", "sessionId_1234_10"), charge);
    }

    [Fact]
    public void FindByNotFound()
    {
        Assert.Null(store.FindBy("SessionId", "not-existing-id"));
    }

    [Fact]
    public void FindByStartDate()
    {
        _ = CreateCharge(10);
        Charge charge = CreateCharge(22);
        _ = CreateCharge(23);

        List<Charge> charges = store.FindByStartDate(charge.StartTime);
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
        Assert.Equal(store.Count(), amount);
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
        List<Charging.Charge> charges = store.ReadAll();

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
        charge = store.Update(charge);

        Charging.Charge? updatedCharge = store.Find(id);
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
        charge = store.Update(charge);

        Assert.Equal(store.Find(id), charge);
        Assert.Equal(store.Find(unchangedId), unchanged);
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
                _ = store.Update(charge);
            }
        );
    }

    [Fact]
    public void DeleteExisting()
    {
        string id = "a3b28d06-f494-46ad-ac89-7927db386fc4";
        Charging.Charge charge = CreateCharge(14, id);

        Assert.True(store.Delete(id));
    }

    [Fact]
    public void DeleteNonExisting()
    {
        string id = "a3b28d06-f494-46ad-ac89-7927db386fc4";

        Assert.False(store.Delete(id));
    }
}
