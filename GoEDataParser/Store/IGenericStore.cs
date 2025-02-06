using MongoDB.Bson.Serialization.Attributes;

namespace Charging
{
    namespace Store
    {
        public interface IGenericStore<T>
            where T : BaseEntity
        {
            public T Insert(T entity);
            public T Update(T entity);
            public bool Delete(string id);
            public T? Find(string id);
            public T? FindBy<V>(string key, V value);
            public List<T> ReadAll();
            public long Count();
        }
    }
}
