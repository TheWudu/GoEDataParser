using Repository;
using Xunit;

namespace GoEDataParserTest;

class TestEntity : BaseEntity
{
    public string myName;

    public TestEntity(string name)
    {
        myName = name;
    }

    public override bool Equals(object? obj)
    {
        return this.Equals(obj as TestEntity);
    }

    public bool Equals(TestEntity? other)
    {
        if (other is null || other.myName != this.myName || other.Version != this.Version)
            return false;

        return true;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}

public class GenericMongoStoreTest
{
    GenericMongoStore<TestEntity> store;

    public GenericMongoStoreTest()
    {
        string dbHost = Charging.Configuration.MongoDbHost();
        string dbName = Charging.Configuration.MongoDbName();
        store = new(dbHost, dbName, "test_entities");
        store.Clear();
    }

    internal TestEntity CreateEntity(string name, string? id = null)
    {
        TestEntity te = new(name);
        te.Id ??= id is not null ? id : null;

        te = store.Insert(te);

        return te;
    }

    [Fact]
    public void InsertTest()
    {
        TestEntity testEntity = new("Martin");
        store.Insert(testEntity);

        Assert.Equal(1, store.Count());
    }

    [Fact]
    public void UpdateTest()
    {
        string id = "0c8af010-a101-4fef-957c-1c78977524ae";
        var te = CreateEntity("Daniel", id);

        te.myName = "Michael";
        var updatedEntity = store.Update(te);

        Assert.Equal(store.Find(id), updatedEntity);
        Assert.Equal(1, store.Count());
    }

    [Fact]
    public void DeleteTest()
    {
        string id = "0c8af010-a101-4fef-957c-1c78977524ae";
        var te = CreateEntity("Daniel", id);

        Assert.True(store.Delete(id));
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

        Assert.Equal(store.Count(), amount);
    }

    [Fact]
    public void FindByString()
    {
        var TestEntity = CreateEntity("michael");

        Assert.Equal(store.FindBy("myName", "michael"), TestEntity);
    }

    [Fact]
    public void FindByInt()
    {
        var TestEntity = CreateEntity("michael");

        Assert.Equal(store.FindBy("Version", 1), TestEntity);
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

        Assert.Equal(store.ReadAll().Count, amount);
    }

    [Fact]
    public void FindTest()
    {
        string id = "0c8af010-a101-4fef-957c-1c78977524ae";
        var te = CreateEntity("Daniel", id);
        string id2 = "0a4b5010-a101-4fef-957c-1c7897752400";
        var te2 = CreateEntity("Michael", id2);

        Assert.Equal(store.Find(id), te);
        Assert.Equal(store.Find(id2), te2);
        Assert.Null(store.Find("not-existing-id"));
    }
}
