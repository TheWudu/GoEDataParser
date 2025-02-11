using MongoDB.Driver;

namespace Charging
{
    public class ChargeMongoStore(string dbHost, string dbName)
        : Repository.GenericMongoStore<Charge>(dbHost, dbName, "charges")
    {
        public List<Charge> FindByStartDate(DateTime dateTime)
        {
            var start = dateTime.Date;
            var end = start.AddDays(1);
            var filter =
                Builders<Charge>.Filter.Gte("StartTime", start)
                & Builders<Charge>.Filter.Lt("StartTime", end);

            var documents = collection.Find(filter);

            return documents.ToList<Charge>();
        }
    }
}
