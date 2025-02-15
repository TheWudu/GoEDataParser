using System.Security.Cryptography.X509Certificates;
using MongoDB.Bson;
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
                Builders<Charge>.Filter.Gte(c => c.StartTime, start)
                & Builders<Charge>.Filter.Lt(c => c.StartTime, end);

            var documents = collection.Find(filter);

            return documents.ToList<Charge>();
        }

        public Dictionary<string, ChargeInfo> GroupMonthly()
        {
            var filter = Builders<Charge>.Filter.Empty;

            var pipeline = new EmptyPipelineDefinition<Charge>()
                .Project(x => new
                {
                    Start = x.StartTime.ToString("%Y.%m"),
                    Kwh = x.Kwh,
                    SecondsCharged = x.SecondsCharged,
                })
                .Group(
                    c => c.Start,
                    g => new ChargeInfo()
                    {
                        TimeKey = g.Key,
                        KwhSum = g.Sum(c => c.Kwh),
                        TimeSum = g.Sum(c => c.SecondsCharged),
                        Count = g.Count(),
                        KwhValues = g.Select(c => c.Kwh).ToList(),
                    }
                )
                .Sort(Builders<ChargeInfo>.Sort.Ascending(x => x.TimeKey));

            Console.WriteLine(pipeline);

            var documents = collection.Aggregate(pipeline).ToList();
            Dictionary<string, ChargeInfo> infos = new();

            foreach (var item in documents)
            {
                // Console.WriteLine(
                //     "{0}, {1,6:F2}, {2,2}, {3} [{4}]",
                //     item.TimeKey,
                //     item.KwhSum,
                //     item.Count,
                //     TimeSpan.FromSeconds(item.TimeSum).ToString(),
                //     String.Join(", ", item.KwhValues)
                // );

                infos.Add(item.TimeKey, item);
            }

            return infos;
        }
    }
}
