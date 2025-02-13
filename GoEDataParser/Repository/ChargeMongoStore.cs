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

        public class GroupingPerTime
        {
            public required string Time { get; set; }
            public double KwhSum { get; set; }
            public long TimeSum { get; set; }
            public int Count { get; set; }
        }

        public List<ChargeInfo> GroupMonthly()
        {
            var filter = Builders<Charge>.Filter.Empty;
            // var filter =
            //    Builders<Charge>.Filter.Gte("StartTime", DateTime.Parse("2023-01-01").Date)
            //    & Builders<Charge>.Filter.Lt("StartTime", DateTime.Parse("2023-06-04").Date);

            var pipeline = new EmptyPipelineDefinition<Charge>()
                .Match(filter)
                .Project(x => new
                {
                    Start = x.StartTime.ToString("%Y-%m"),
                    Kwh = x.Kwh,
                    SecondsCharged = x.SecondsCharged,
                })
                .Group(
                    c => c.Start,
                    g => new GroupingPerTime()
                    {
                        Time = g.Key,
                        KwhSum = g.Sum(c => (double)c.Kwh),
                        TimeSum = g.Sum(c => c.SecondsCharged),
                        Count = g.Count(),
                    }
                )
                .Sort(Builders<GroupingPerTime>.Sort.Ascending(x => x.Time));

            Console.WriteLine(pipeline);

            var documents = collection.Aggregate(pipeline).ToList();

            List<ChargeInfo> infos = new();

            foreach (var item in documents)
            {
                Console.WriteLine(
                    "{0}, {1,6:F2}, {2,2}, {3}",
                    item.Time,
                    item.KwhSum,
                    item.Count,
                    item.TimeSum
                );
                infos.Add(new ChargeInfo((float)item.KwhSum, item.Count, item.TimeSum, 0.0F));
            }

            return infos;
        }
    }
}
