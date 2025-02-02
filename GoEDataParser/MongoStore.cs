using MongoDB.Bson;
using MongoDB.Driver;

namespace Charging
{
    class MongoStore
    {
        private MongoClient client;

        // private IMongoCollection<Charge> collection = null;

        public MongoStore()
        {
            string? connectionString = "mongodb://localhost/?retryWrites=true&w=majority";

            client = new MongoClient(connectionString);
        }

        public Charge Insert(Charge charge)
        {
            var collection = client.GetDatabase("goe").GetCollection<Charge>("charges");
            if (charge._id is null)
            {
                charge._id = Guid.NewGuid().ToString();
            }
            collection.InsertOne(charge);

            return charge;
        }

        public Charge? Find(string session_id)
        {
            var collection = client.GetDatabase("goe").GetCollection<Charge>("charges");
            var filter = Builders<Charge>.Filter.Eq("session_id", session_id);
            var document = collection.Find(filter).FirstOrDefault();

            return document;
        }

        public Charge First()
        {
            var collection = client.GetDatabase("goe").GetCollection<Charge>("charges");
            //var filter = Builders<BsonDocument>.Filter.Eq("title", "Back to the Future");
            var filter = Builders<Charge>.Filter.Empty;
            var document = collection.Find(filter).First();

            return document;
        }
    }
}
