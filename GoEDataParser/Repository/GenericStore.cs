namespace Repository
{
    class GenericStore<T> : IGenericStore<T>
        where T : BaseEntity
    {
        private readonly IGenericStore<T> _store;

        public GenericStore(string dbHost, string dbName, string dbCollection)
        {
            _store = new GenericMongoStore<T>(dbHost, dbName, dbCollection);
        }

        public GenericStore(IGenericStore<T> genericStore)
        {
            _store = genericStore;
        }

        public T Insert(T entity)
        {
            return _store.Insert(entity);
        }

        public T Update(T entity)
        {
            return _store.Update(entity);
        }

        public T? Find(string id)
        {
            return _store.Find(id);
        }

        public T? FindBy<TV>(string key, TV value)
        {
            return _store.FindBy(key, value);
        }

        public long Count()
        {
            return _store.Count();
        }

        public List<T> ReadAll()
        {
            return _store.ReadAll();
        }

        public bool Delete(string id)
        {
            return _store.Delete(id);
        }
    };
}
