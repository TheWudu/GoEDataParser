using MongoDB.Bson;
using MongoDB.Driver;

namespace Charging
{
    namespace Store
    {
        public class MongoStore : IChargeStore
        {
            private MongoClient client;
            private string dbName;

            // private IMongoCollection<Charge> collection = null;

            public MongoStore()
            {
                string? connectionString = "mongodb://localhost/?retryWrites=true&w=majority";

                client = new MongoClient(connectionString);
                dbName = Configuration.MongoDbName();
            }

            public long Count()
            {
                var collection = client.GetDatabase(dbName).GetCollection<Charge>("charges");

                return collection.CountDocuments(Builders<Charge>.Filter.Empty);
            }

            public Charge Insert(Charge charge)
            {
                var collection = client.GetDatabase(dbName).GetCollection<Charge>("charges");
                if (charge._id is null)
                {
                    charge._id = Guid.NewGuid().ToString();
                }
                collection.InsertOne(charge);

                return charge;
            }

            public Charge? FindBySessionId(string session_id)
            {
                var collection = client.GetDatabase(dbName).GetCollection<Charge>("charges");
                var filter = Builders<Charge>.Filter.Eq("session_id", session_id);
                var document = collection.Find(filter).FirstOrDefault();

                return document;
            }

            public Charge First()
            {
                var collection = client.GetDatabase(dbName).GetCollection<Charge>("charges");
                //var filter = Builders<BsonDocument>.Filter.Eq("title", "Back to the Future");
                var filter = Builders<Charge>.Filter.Empty;
                var document = collection.Find(filter).First();

                return document;
            }

            public void Clear()
            {
                client.GetDatabase(dbName).DropCollection("charges");
            }
        }
    }
}
