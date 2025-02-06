using MongoDB.Bson.Serialization.Attributes;

namespace Charging
{
    namespace Store
    {
        public interface IGenericStore<T>
            where T : BaseEntity
        {
            public T Insert(T entity);
            public T? FindBy<V>(string key, V value);
            public List<T> ReadAll();
            public long Count();
        }
    }
}
