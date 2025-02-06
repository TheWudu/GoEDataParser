using MongoDB.Bson;
using MongoDB.Driver;

namespace Charging
{
    namespace Store
    {
        public class GenericMongoStore<T> : IGenericStore<T>
            where T : BaseEntity
        {
            private readonly MongoClient client;
            private readonly string dbHost = Charging.Configuration.MongoDbHost();
            private readonly string dbName = Charging.Configuration.MongoDbName();
            private readonly string collectionName = "charges";
            protected readonly IMongoCollection<T> collection;

            public GenericMongoStore()
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
                if (entity.Id is null)
                {
                    entity.Id = Guid.NewGuid().ToString();
                }
                collection.InsertOne(entity);

                return entity;
            }

            public T Update(T entity)
            {
                var filter = Builders<T>.Filter.Eq("_id", entity.Id);
                collection.ReplaceOne(filter, entity);

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
        }
    }
}
