using GoEDataParser.Models;
using MongoDB.Driver;
using Repository;

namespace GoEDataParser.Repository
{
    public class ConsumptionMongoStore(string dbHost, string dbName)
        : GenericMongoStore<Consumption>(dbHost, dbName, "consumptions")
    {
        public Consumption? FindByStartTime(DateTime startTime)
        {
            var filter = Builders<Consumption>.Filter.Eq(c => c.StartTime, startTime);

            var documents = Collection.Find(filter);

            if (documents.ToList().Count == 1)
                return documents.ToList()[0];
            else
                return null;
        }

        public List<Consumption> FindConsumptions(DateTime chargeStart, DateTime chargeEnd)
        {
            var filter = Builders<Consumption>.Filter.Or(
                (
                    Builders<Consumption>.Filter.Gt(c => c.StartTime, chargeStart)
                    & Builders<Consumption>.Filter.Lt(c => c.StartTime, chargeEnd)
                ),
                (
                    Builders<Consumption>.Filter.Gt(c => c.EndTime, chargeStart)
                    & Builders<Consumption>.Filter.Lt(c => c.EndTime, chargeEnd)
                )
            );

            var documents = Collection.Find(
                filter //,
            //new FindOptions { Hint = "StartTime_-1_EndTime_-1" }
            );
            // Console.WriteLine(documents);

            return documents.ToList();
        }

        public Consumption Upsert(Consumption consumption)
        {
            var cons = FindByStartTime(consumption.StartTime);
            if (cons is null)
            {
                consumption.Id = Guid.NewGuid().ToString();
                consumption.Version = 1;

                Console.Write(".");

                return Insert(consumption);
            }
            else
            {
                consumption.Id = cons.Id;
                consumption.Version = cons.Version;

                Console.Write("+");

                return Update(consumption);
            }
        }
    }
}
