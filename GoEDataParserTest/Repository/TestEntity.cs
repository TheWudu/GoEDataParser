using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Repository;

namespace GoEDataParserTest;

public class TestEntity : Repository.BaseEntity
{
    public string? Name { get; set; }

    public TestEntity()
    {
        Id = Guid.NewGuid().ToString();
        Version = 1;
    }

    public TestEntity(string? name)
    {
        Name = name;
        Id = Guid.NewGuid().ToString();
        Version = 1;
    }

    public TestEntity(string name, string? id)
    {
        Name = name;
        Id = id is not null ? id : Guid.NewGuid().ToString();
        Version = 1;
    }

    public TestEntity(string name, string? id, int version)
    {
        Name = name;
        Id = id is not null ? id : Guid.NewGuid().ToString();
        Version = version;
    }

    public override bool Equals(object? obj)
    {
        return this.Equals(obj as TestEntity);
    }

    public bool Equals(TestEntity? other)
    {
        if (other is null || other.Name != this.Name || other.Version != this.Version)
            return false;

        return true;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
