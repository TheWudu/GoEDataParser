using System.Linq.Expressions;

namespace Repository
{
    public interface IGenericStore<T>
        where T : BaseEntity
    {
        public T Insert(T entity);
        public T Update(T entity);
        public bool Delete(string id);
        public T? FindBy(Expression<Func<T, bool>> expr);
        public T? Find(string id);
        public T? FindBy<TV>(string key, TV value);
        public List<T> ReadAll();
        public long Count();
    }
}
