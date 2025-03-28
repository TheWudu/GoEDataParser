using Repository;

namespace RepositoryTest;

public class TestEntity : BaseEntity
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
        Id = id ?? Guid.NewGuid().ToString();
        Version = 1;
    }

    public TestEntity(string name, string? id, int version)
    {
        Name = name;
        Id = id ?? Guid.NewGuid().ToString();
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
        // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
        return base.GetHashCode();
    }
}
