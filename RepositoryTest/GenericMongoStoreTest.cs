using Repository;
using Xunit;

namespace RepositoryTest;

public class GenericMongoStoreTest
{
    GenericMongoStore<TestEntity> _store;

    public GenericMongoStoreTest()
    {
        string dbHost = Configuration.MongoDbHost();
        string dbName = Configuration.MongoDbName();
        _store = new(dbHost, dbName, "test_entities");
        _ = _store.Clear();
    }

    private async Task<TestEntity> CreateEntity(string name, string? id = null)
    {
        TestEntity te = new(name, id);

        te = await _store.Insert(te);

        return te;
    }

    [Fact]
    public async Task InsertTest()
    {
        TestEntity testEntity = new("Martin");
        await _store.Insert(testEntity);

        Assert.Equal(1, await _store.Count());
    }

    [Fact]
    public async Task UpdateTest()
    {
        string id = "0c8af010-a101-4fef-957c-1c78977524ae";
        var te = await CreateEntity("Daniel", id);

        te.Name = "Michael";
        var updatedEntity = await _store.Update(te);

        var storedEntity = await _store.Find(id);
        Assert.Equal(updatedEntity, storedEntity);
        Assert.Equal(2, storedEntity?.Version);
        Assert.Equal(1, await _store.Count());
    }

    [Fact]
    public async Task DeleteTest()
    {
        var id = "0c8af010-a101-4fef-957c-1c78977524ae";
        _ = await CreateEntity("Daniel", id);

        Assert.True(await _store.Delete(id));
    }

    [Theory]
    [InlineData(3)]
    [InlineData(7)]
    public async Task CountTest(int amount)
    {
        for (var i = 0; i < amount; i++)
        {
            await CreateEntity("Test" + i.ToString());
        }

        Assert.Equal(await _store.Count(), amount);
    }

    [Fact]
    public async Task FindByString()
    {
        var testEntity = await CreateEntity("michael");

        Assert.Equal(testEntity, await _store.FindBy("Name", "michael"));
    }

    [Fact]
    public async Task FindByInt()
    {
        var testEntity = await CreateEntity("michael");

        Assert.Equal(testEntity, await _store.FindBy("Version", 1));
    }

    [Fact]
    public async Task FindByExpression()
    {
        var testEntity = await CreateEntity("michael");

        Assert.Equal(testEntity, await _store.FindBy(e => e.Name == "michael"));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(7)]
    public async Task ReadAllTest(int amount)
    {
        for (var i = 0; i < amount; i++)
        {
            await CreateEntity("Test" + i.ToString());
        }

        Assert.Equal(amount, (await _store.ReadAll()).Count);
    }

    [Fact]
    public async Task FindTest()
    {
        var id = "0c8af010-a101-4fef-957c-1c78977524ae";
        var te = await CreateEntity("Daniel", id);
        var id2 = "0a4b5010-a101-4fef-957c-1c7897752400";
        var te2 = await CreateEntity("Michael", id2);

        Assert.Equal(te, await _store.Find(id));
        Assert.Equal(te2, await _store.Find(id2));
        Assert.Null(await _store.Find("not-existing-id"));
    }
}
