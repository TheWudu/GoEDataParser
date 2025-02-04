using MongoDB.Bson;
using MongoDB.Driver;

namespace Charging
{
    namespace Store
    {
        public class MongoStore : IChargeStore
        {
            private readonly MongoClient client;
            private readonly string dbHost = Configuration.MongoDbHost();
            private readonly string dbName = Configuration.MongoDbName();
            private readonly string collectionName = "charges";
            private readonly IMongoCollection<Charge> collection;

            public MongoStore()
            {
                string? connectionString = "mongodb://" + dbHost + "/?retryWrites=true&w=majority";
                client = new MongoClient(connectionString);
                collection = client.GetDatabase(dbName).GetCollection<Charge>(collectionName);
            }

            public long Count()
            {
                return collection.CountDocuments(Builders<Charge>.Filter.Empty);
            }

            public Charge Insert(Charge charge)
            {
                if (charge._id is null)
                {
                    charge._id = Guid.NewGuid().ToString();
                }
                collection.InsertOne(charge);

                return charge;
            }

            public Charge? FindBySessionId(string sessionId)
            {
                var filter = Builders<Charge>.Filter.Eq("session_id", sessionId);
                var document = collection.Find(filter).FirstOrDefault();

                return document;
            }

            public List<Charge> FindByStartDate(DateTime dateTime)
            {
                var start = dateTime.Date;
                var end = start.AddDays(1);
                var filter =
                    Builders<Charge>.Filter.Gte("start_time", start)
                    & Builders<Charge>.Filter.Lt("start_time", end);

                var documents = collection.Find(filter);

                return documents.ToList<Charge>();
            }

            public void Clear()
            {
                collection.DeleteMany(Builders<Charge>.Filter.Empty);
            }
        }
    }
}
