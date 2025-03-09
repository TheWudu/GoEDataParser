using System.Collections;
using Repository;
using Xunit;

namespace GoEDataParserTest;

public class GenericMysqlStoreTest
{
    GenericMysqlStore<TestEntity> _store;

    public GenericMysqlStoreTest()
    {
        string dbHost = Charging.Configuration.MysqlDbHost();
        string dbName = Charging.Configuration.MysqlDbName();
        string dbUser = Charging.Configuration.MysqlDbUser();
        string dbPassword = Charging.Configuration.MysqlDbPassword();
        _store = new(dbHost, dbName, (string)"test_entities", dbUser, dbPassword);

        _store.Clear();
    }

    internal TestEntity CreateEntity(string name, string? id = null, int version = 1)
    {
        TestEntity te = new(name, id, version);

        _store.Insert(te);

        return te;
    }

    [Fact]
    public void MysqlInsertTest()
    {
        TestEntity testEntity = new("Martin");
        _store.Insert(testEntity);

        Assert.Equal(1, _store.Count());
    }

    [Fact]
    public void UpdateTest()
    {
        string id = "0c8af010-a101-4fef-957c-1c78977524cc";
        var te = CreateEntity("Daniel", id);

        TestEntity? ut = _store.Find(id);
        Assert.NotNull(ut);
        ut.Name = "Michael";
        var updatedEntity = _store.Update(ut);

        var storedEntity = _store.Find(id);
        Assert.Equal(storedEntity, ut);
        Assert.Equal(storedEntity?.Version, 2);
        Assert.Equal(1, _store.Count());
    }

    [Fact]
    public void DeleteTest()
    {
        string id = "0c8af010-a101-4fef-957c-1c78977524ae";
        var te = CreateEntity("Daniel", id);

        Console.WriteLine("{0} - {1}", te.Id, te.Name);

        Assert.True(_store.Delete(id));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(7)]
    public void CountTest(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            CreateEntity("Test" + i.ToString());
        }

        Assert.Equal(_store.Count(), amount);
    }

    [Fact]
    public void FindByString()
    {
        var testEntity = CreateEntity("michael");

        Assert.Equal(_store.FindBy("name", "michael"), testEntity);
    }

    [Fact]
    public void FindByInt()
    {
        string id1 = Guid.NewGuid().ToString();
        string id2 = Guid.NewGuid().ToString();
        var testEntity1 = CreateEntity("michael", id1);
        var testEntity2 = CreateEntity("daniel", id2, 2);

        var readEntityV1 = _store.FindBy("version", 1);
        var readEntityV2 = _store.FindBy("version", 2);

        Assert.Equal(readEntityV2, testEntity2);
        Assert.NotNull(readEntityV1);
        Assert.Equal(readEntityV1.Id, id1);
        Assert.NotNull(readEntityV2);
        Assert.Equal(readEntityV2.Id, id2);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(7)]
    public void ReadAllTest(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            CreateEntity("Test" + i.ToString());
        }

        Assert.Equal(_store.ReadAll().Count, amount);
    }

    [Fact]
    public void FindTest()
    {
        string id = "0c8af010-a101-4fef-957c-1c78977524ae";
        var te = CreateEntity("Daniel", id);
        string id2 = "0a4b5010-a101-4fef-957c-1c7897752400";
        var te2 = CreateEntity("Michael", id2);

        Assert.Equal(_store.Find(id), te);
        Assert.Equal(_store.Find(id2), te2);
        Assert.Null(_store.Find("not-existing-id"));
    }
}
