using Repository;
using Xunit;

namespace GoEDataParserTest;

public class GenericMongoStoreTest
{
    GenericMongoStore<TestEntity> _store;

    public GenericMongoStoreTest()
    {
        string dbHost = Charging.Configuration.MongoDbHost();
        string dbName = Charging.Configuration.MongoDbName();
        _store = new(dbHost, dbName, "test_entities");
        _store.Clear();
    }

    internal TestEntity CreateEntity(string name, string? id = null)
    {
        TestEntity te = new(name, id);

        te = _store.Insert(te);

        return te;
    }

    [Fact]
    public void InsertTest()
    {
        TestEntity testEntity = new("Martin");
        _store.Insert(testEntity);

        Assert.Equal(1, _store.Count());
    }

    [Fact]
    public void UpdateTest()
    {
        string id = "0c8af010-a101-4fef-957c-1c78977524ae";
        var te = CreateEntity("Daniel", id);

        te.Name = "Michael";
        var updatedEntity = _store.Update(te);

        var storedEntity = _store.Find(id);
        Assert.Equal(storedEntity, updatedEntity);
        Assert.Equal(storedEntity?.Version, 2);
        Assert.Equal(1, _store.Count());
    }

    [Fact]
    public void DeleteTest()
    {
        string id = "0c8af010-a101-4fef-957c-1c78977524ae";
        var te = CreateEntity("Daniel", id);

        Assert.True(_store.Delete(id));
    }

    [Theory]
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

        Assert.Equal(_store.FindBy("Name", "michael"), testEntity);
    }

    [Fact]
    public void FindByInt()
    {
        var testEntity = CreateEntity("michael");

        Assert.Equal(_store.FindBy("Version", 1), testEntity);
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
