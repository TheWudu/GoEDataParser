using GoEDataParser;
using GoEDataParser.Models;
using GoEDataParser.Repository;
using Xunit;

namespace GoEDataParserTest.Repository;

public class ChargeMongoStoreTest
{
    ChargeMongoStore _store;

    public ChargeMongoStoreTest()
    {
        var dbHost = Configuration.MongoDbHost();
        var dbName = Configuration.MongoDbName();
        _store = new(dbHost, dbName);
        _store.Clear();
    }

    // Insert
    [Fact]
    public async Task InsertSimple()
    {
        Charge charge = new()
        {
            SessionId = "212234_1685716486",
            Kwh = 10.123F,
            StartTime = DateTime.Parse("2025-01-01T09:01:02Z"),
            EndTime = DateTime.Parse("2025-01-01T11:05:47Z"),
        };

        await _store.Insert(charge);

        Assert.Equal(1, await _store.Count());
    }

    async Task<Charge> CreateCharge(int day, string? id = null)
    {
        Charge charge = new()
        {
            Id = id is not null ? id : Guid.NewGuid().ToString(),
            SessionId = "sessionId_1234_" + day.ToString(),
            Kwh = 10.123,
            StartTime = new DateTime(2025, 01, day, 09, 01, 02, DateTimeKind.Utc),
            EndTime = new DateTime(2025, 01, day, 11, 05, 47, DateTimeKind.Utc),
        };

        await _store.Insert(charge);

        return charge;
    }

    [Fact]
    public async Task FindBySuccess()
    {
        var charge = await CreateCharge(10);

        Assert.Equal(charge, await _store.FindBy("SessionId", "sessionId_1234_10"));
    }

    [Fact]
    public async Task FindByNotFound()
    {
        Assert.Null(await _store.FindBy("SessionId", "not-existing-id"));
    }

    [Fact]
    public async Task FindByStartDate()
    {
        _ = CreateCharge(10);
        var charge = await CreateCharge(22);
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
    public async Task Count(int amount)
    {
        for (var i = 0; i < amount; i++)
        {
            _ = await CreateCharge(i + 1);
        }
        Assert.Equal(amount, await _store.Count());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(29)]
    public async Task ReadAll(int amount)
    {
        List<Charge> insertedCharges = new();

        for (var i = 0; i < amount; i++)
        {
            insertedCharges.Add(await CreateCharge(i + 1));
        }
        var charges = await _store.ReadAll();

        Assert.Multiple(() =>
        {
            Assert.Equal(charges.Count, amount);
            Assert.Equal(charges.ElementAt(0), insertedCharges.ElementAt(0));
            Assert.Equal(charges.ElementAt(amount - 1), insertedCharges.ElementAt(amount - 1));
        });
    }

    [Fact]
    public async Task UpdateExisting()
    {
        string id = "a3b28d06-f494-46ad-ac89-7927db386fc4";
        Charge charge = await CreateCharge(14, id);

        charge.Kwh += 10.0F;
        charge = await _store.Update(charge);

        Charge? updatedCharge = await _store.Find(id);
        Assert.NotNull(charge);
        Assert.Equal(updatedCharge, charge);
    }

    [Fact]
    public async Task UpdateTheCorrectOne()
    {
        var id = "a3b28d06-f494-46ad-ac89-7927db386fc4";
        var charge = await CreateCharge(14, id);
        var unchangedId = "8da64b25-5522-4ded-8c0f-ffcdd4331840";
        var unchanged = await CreateCharge(15, unchangedId);

        charge.Kwh += 10.0F;
        charge = await _store.Update(charge);

        Assert.Equal(charge, await _store.Find(id));
        Assert.Equal(unchanged, await _store.Find(unchangedId));
    }

    [Fact]
    public async Task UpdateOnlyWithProperVersion()
    {
        var id = "a3b28d06-f494-46ad-ac89-7927db386fc4";
        var charge = await CreateCharge(14, id);

        charge.Kwh += 10.0F;
        charge.Version = 5;

        await Assert.ThrowsAsync<global::Repository.EntityNotFoundException>(
            () => _store.Update(charge)
        );
    }

    [Fact]
    public async Task DeleteExisting()
    {
        var id = "a3b28d06-f494-46ad-ac89-7927db386fc4";
        _ = await CreateCharge(14, id);

        Assert.True(await _store.Delete(id));
    }

    [Fact]
    public async Task DeleteNonExisting()
    {
        var id = "a3b28d06-f494-46ad-ac89-7927db386fc4";

        Assert.False(await _store.Delete(id));
    }

    [Fact]
    public void TestMonthly()
    {
        string id = "a3b28d06-f494-46ad-ac89-7927db386fc4";
        _ = CreateCharge(14, id);

        var monthly = _store.GroupMonthly();

        Assert.Multiple(() =>
        {
            Assert.Single(monthly);
            Assert.Equal(10.123, monthly["2025.01"].KwhSum);
            Assert.False(monthly.ContainsKey("2025.02"));
        });
    }
}
