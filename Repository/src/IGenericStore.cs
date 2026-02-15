using System.Linq.Expressions;

namespace Repository
{
    public interface IGenericStore<T>
        where T : BaseEntity
    {
        public Task<T> Insert(T entity);
        public Task<T> Update(T entity);
        public Task<bool> Delete(string id);
        public Task<T?> FindBy(Expression<Func<T, bool>> expr);
        public Task<T?> Find(string id);
        public Task<List<T>> ReadAll();
        public Task<long> Count();
    }
}
