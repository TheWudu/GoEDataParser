using MongoDB.Bson;
using MongoDB.Driver;

namespace Repository
{
    public class EntityNotFoundException(string? message) : Exception(message) { }

    public class GenericMongoStore<T> : IGenericStore<T>
        where T : BaseEntity
    {
        private readonly MongoClient client;
        private readonly string collectionName = "charges";
        protected readonly IMongoCollection<T> collection;

        public GenericMongoStore(string dbHost, string dbName)
        {
            string? connectionString = "mongodb://" + dbHost + "/?retryWrites=true&w=majority";
            client = new MongoClient(connectionString);
            collection = client.GetDatabase(dbName).GetCollection<T>(collectionName);
        }

        public long Count()
        {
            return collection.CountDocuments(Builders<T>.Filter.Empty);
        }

        public T Insert(T entity)
        {
            entity.Id ??= Guid.NewGuid().ToString();
            entity.Version = 1;

            collection.InsertOne(entity);

            return entity;
        }

        public T Update(T entity)
        {
            var filter =
                Builders<T>.Filter.Eq("_id", entity.Id)
                & Builders<T>.Filter.Eq("Version", entity.Version);

            entity.Version += 1;
            var resp = collection.ReplaceOne(filter, entity);
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

        public T? FindBy<V>(string key, V value)
        {
            var filter = Builders<T>.Filter.Eq(key, value);

            return collection.Find(filter).FirstOrDefault();
        }

        public void Clear()
        {
            collection.DeleteMany(Builders<T>.Filter.Empty);
        }

        public List<T> ReadAll()
        {
            return collection.Find(Builders<T>.Filter.Empty).ToList();
        }

        public T? Find(string id)
        {
            return FindBy("_id", id);
        }

        public bool Delete(string id)
        {
            var filter = Builders<T>.Filter.Eq(e => e.Id, id);
            var res = collection.DeleteOne(filter);

            return res.DeletedCount == 1;
        }
    }
}
