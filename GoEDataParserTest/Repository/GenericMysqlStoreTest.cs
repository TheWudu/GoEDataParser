using System.Collections;
using MySql.Data.MySqlClient;
using Repository;
using Xunit;

namespace GoEDataParserTest;

public class GenericMysqlStoreTest
{
    GenericMysqlStore<TestEntity> store;

    public GenericMysqlStoreTest()
    {
        string dbHost = Charging.Configuration.MysqlDbHost();
        string dbName = Charging.Configuration.MysqlDbName();
        string dbUser = Charging.Configuration.MysqlDbUser();
        string dbPassword = Charging.Configuration.MysqlDbPassword();
        store = new(dbHost, dbName, "test_entities", dbUser, dbPassword);
        store.Clear();
    }

    internal TestEntity CreateEntity(string name, string? id = null)
    {
        TestEntity te = new(name, id);

        store.Insert(te);

        return te;
    }

    [Fact]
    public void MysqlInsertTest()
    {
        TestEntity testEntity = new("Martin");
        store.Insert(testEntity);

        Assert.Equal(1, store.Count());
    }

    [Fact]
    public void UpdateTest()
    {
        string id = "0c8af010-a101-4fef-957c-1c78977524cc";
        var te = CreateEntity("Daniel", id);

        te.Name = "Michael";
        var updatedEntity = store.Update(te);

        var storedEntity = store.Find(id);
        Assert.Equal(store.Find(id), updatedEntity);
        Assert.Equal(1, store.Count());
    }

    [Fact]
    public void DeleteTest()
    {
        string id = "0c8af010-a101-4fef-957c-1c78977524ae";
        var te = CreateEntity("Daniel", id);

        Console.WriteLine("{0} - {1}", te.Id, te.Name);

        Assert.True(store.Delete(id));
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

        Assert.Equal(store.Count(), amount);
    }

    [Fact]
    public void FindByString()
    {
        var TestEntity = CreateEntity("michael");

        Assert.Equal(store.FindBy("name", "michael"), TestEntity);
    }

    [Fact]
    public void FindByInt()
    {
        var TestEntity = CreateEntity("michael");

        Assert.Equal(store.FindBy("version", 1), TestEntity);
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
        //Assert.Equal(store.Find(id2), te2);
        //Assert.Null(store.Find("not-existing-id"));
    }
}
