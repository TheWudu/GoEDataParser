using MongoDB.Driver;

namespace Repository
{
    public class EntityNotFoundException(string? message) : Exception(message) { }

    public class GenericMongoStore<T> : IGenericStore<T>
        where T : BaseEntity
    {
        protected readonly IMongoCollection<T> Collection;

        public GenericMongoStore(string dbHost, string dbName, string dbCollection)
        {
            string connectionString = "mongodb://" + dbHost + "/?retryWrites=true&w=majority";
            var client = new MongoClient(connectionString);
            Collection = client.GetDatabase(dbName).GetCollection<T>(dbCollection);
        }

        public long Count()
        {
            return Collection.CountDocuments(Builders<T>.Filter.Empty);
        }

        public T Insert(T entity)
        {
            Collection.InsertOne(entity);

            return entity;
        }

        public T Update(T entity)
        {
            var filter =
                Builders<T>.Filter.Eq("_id", entity.Id)
                & Builders<T>.Filter.Eq("Version", entity.Version);

            entity.Version += 1;
            var resp = Collection.ReplaceOne(filter, entity);
            if (resp.ModifiedCount != 1)
            {
                string message =
                    "Entity with Id: "
                    + entity.Id
                    + " and Version: "
                    + entity.Version
                    + " not found";
                throw new EntityNotFoundException(message);
            }

            return entity;
        }

        public T? FindBy<TV>(string key, TV value)
        {
            var filter = Builders<T>.Filter.Eq(key, value);

            return Collection.Find(filter).FirstOrDefault();
        }

        public void Clear()
        {
            Collection.DeleteMany(Builders<T>.Filter.Empty);
        }

        public List<T> ReadAll()
        {
            return Collection.Find(Builders<T>.Filter.Empty).ToList();
        }

        public T? Find(string id)
        {
            return FindBy("_id", id);
        }

        public bool Delete(string id)
        {
            var filter = Builders<T>.Filter.Eq(e => e.Id, id);
            var res = Collection.DeleteOne(filter);

            return res.DeletedCount == 1;
        }
    }
}
