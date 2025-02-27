namespace Repository
{
    class GenericStore<T> : IGenericStore<T>
        where T : BaseEntity
    {
        private readonly IGenericStore<T> store;

        public GenericStore(string dbHost, string dbName, string dbCollection)
        {
            store = new GenericMongoStore<T>(dbHost, dbName, dbCollection);
        }

        public GenericStore(IGenericStore<T> genericStore)
        {
            store = genericStore;
        }

        public T Insert(T entity)
        {
            return store.Insert(entity);
        }

        public T Update(T entity)
        {
            return store.Update(entity);
        }

        public T? Find(string id)
        {
            return store.Find(id);
        }

        public T? FindBy<V>(string key, V value)
        {
            return store.FindBy(key, value);
        }

        public long Count()
        {
            return store.Count();
        }

        public List<T> ReadAll()
        {
            return store.ReadAll();
        }

        public bool Delete(string id)
        {
            return store.Delete(id);
        }
    };
}
