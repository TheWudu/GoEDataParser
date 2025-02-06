using MongoDB.Driver;

namespace Charging
{
    namespace Store
    {
        public class ChargeMongoStore : GenericMongoStore<Charge>
        {
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
        }
    }
}
