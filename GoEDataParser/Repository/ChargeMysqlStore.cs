using GoEDataParser.Models;
using Repository;

namespace GoEDataParser.Repository
{
    public class ChargeMysqlStore(string dbHost, string dbName, string dbUser, string dbPassword)
        : GenericMysqlStore<Charge>(dbHost, dbName, "charges", dbUser, dbPassword)
    {
        public List<Charge> FindByStartDate(DateTime dateTime)
        {
            return Dataset.Where(e => e.StartTime == dateTime).ToList();
        }

        public new List<Charge> ReadAll()
        {
            var list = Dataset.Where(e => true).OrderBy(e => e.StartTime).ToList();

            return list;
        }

        public Dictionary<string, ChargeInfo> GroupMonthly()
        {
            var documents = Dataset
                .Select(e => new
                {
                    Start = e.StartTime.Year.ToString() + "." + e.StartTime.Month.ToString(),
                    Kwh = e.Kwh,
                    SecondsCharged = e.SecondsCharged,
                })
                .GroupBy(e => e.Start)
                .Select(g => new ChargeInfo()
                {
                    TimeKey = g.Key,
                    KwhSum = g.Sum(c => c.Kwh),
                    TimeSum = g.Sum(c => c.SecondsCharged),
                    Count = g.Count(),
                    KwhValues = g.Select(c => c.Kwh).ToList(),
                })
                .ToList();

            Dictionary<string, ChargeInfo> infos = new();

            foreach (var item in documents)
            {
                // Workaround as i did not find a way to use
                // DATE_FORMAT in select directly
                var keys = item.TimeKey.Split(".");
                keys[1] = Convert.ToInt32(keys[1]).ToString("00");
                var key = String.Join(".", keys);
                infos.Add(key, item);
            }

            return infos;
        }
    }
}
