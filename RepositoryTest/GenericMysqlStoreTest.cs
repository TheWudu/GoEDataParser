using Repository;
using Xunit;

namespace RepositoryTest;

public class GenericMysqlStoreTest
{
    GenericMysqlStore<TestEntity> _store;

    public GenericMysqlStoreTest()
    {
        var dbHost = Configuration.MysqlDbHost();
        var dbName = Configuration.MysqlDbName();
        var dbUser = Configuration.MysqlDbUser();
        var dbPassword = Configuration.MysqlDbPassword();
        _store = new(dbHost, dbName, (string)"test_entities", dbUser, dbPassword);

        _store.Setup();
        _store.Clear();
    }

    private async Task<TestEntity> CreateEntity(string name, string? id = null, int version = 1)
    {
        TestEntity te = new(name, id, version);

        await _store.Insert(te);

        return te;
    }

    [Fact]
    public async Task MysqlInsertTest()
    {
        TestEntity testEntity = new("Martin");
        await _store.Insert(testEntity);

        Assert.Equal(1, await _store.Count());
    }

    [Fact]
    public async Task UpdateTest()
    {
        var id = "0c8af010-a101-4fef-957c-1c78977524cc";
        _ = await CreateEntity("Daniel", id);

        var ut = await _store.Find(id);
        Assert.NotNull(ut);
        ut.Name = "Michael";
        _ = await _store.Update(ut);

        var storedEntity = await _store.Find(id);
        Assert.Equal(ut, storedEntity);
        Assert.Equal(2, storedEntity?.Version);
        Assert.Equal(1, await _store.Count());
    }

    [Fact]
    public async Task DeleteTest()
    {
        var id = "0c8af010-a101-4fef-957c-1c78977524ae";
        var te = await CreateEntity("Daniel", id);

        Console.WriteLine("{0} - {1}", te.Id, te.Name);

        Assert.True(await _store.Delete(id));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(7)]
    public async Task CountTest(int amount)
    {
        for (var i = 0; i < amount; i++)
        {
            await CreateEntity("Test" + i.ToString());
        }

        Assert.Equal(amount, await _store.Count());
    }

    [Fact]
    public async Task FindByString()
    {
        var testEntity = await CreateEntity("michael");

        Assert.Equal(testEntity, await _store.FindBy(e => e.Name == "michael"));
    }

    [Fact]
    public async Task FindBy_Expression()
    {
        var id1 = Guid.NewGuid().ToString();
        var id2 = Guid.NewGuid().ToString();
        _ = await CreateEntity("michael", id1);
        var testEntity2 = await CreateEntity("daniel", id2, 2);

        var readEntityV1 = await _store.FindBy(e => e.Version == 1);
        var readEntityV2 = await _store.FindBy(e => e.Version == 2);

        Assert.Equal(testEntity2, readEntityV2);
        Assert.NotNull(readEntityV1);
        Assert.Equal(id1, readEntityV1.Id);
        Assert.NotNull(readEntityV2);
        Assert.Equal(id2, readEntityV2.Id);
    }

    [Fact]
    public async Task FindByInt()
    {
        var id1 = Guid.NewGuid().ToString();
        var id2 = Guid.NewGuid().ToString();
        _ = await CreateEntity("michael", id1);
        var testEntity2 = await CreateEntity("daniel", id2, 2);

        var readEntityV1 = await _store.FindBy(e => e.Version == 1);
        var readEntityV2 = await _store.FindBy(e => e.Version == 2);

        Assert.Equal(testEntity2, readEntityV2);
        Assert.NotNull(readEntityV1);
        Assert.Equal(id1, readEntityV1.Id);
        Assert.NotNull(readEntityV2);
        Assert.Equal(id2, readEntityV2.Id);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(7)]
    public async Task ReadAllTest(int amount)
    {
        for (int i = 0; i < amount; i++)
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
