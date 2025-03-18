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
            Dictionary<string, ChargeInfo> documents = Dataset
                .Select(e => new
                {
                    Start = e.StartTime.Year + "." + e.StartTime.Month,
                    e.StartTime.Year,
                    e.StartTime.Month,
                    e.Kwh,
                    e.SecondsCharged,
                })
                .GroupBy(e => new { e.Year, e.Month })
                .Select(g => new ChargeInfo
                {
                    TimeKey = g.Key.Year + "." + g.Key.Month.ToString("00"),
                    KwhSum = g.Sum(c => c.Kwh),
                    TimeSum = g.Sum(c => c.SecondsCharged),
                    Count = g.Count(),
                    KwhValues = g.Select(c => c.Kwh).ToList(),
                })
                .ToDictionary(e => e.TimeKey, e => e);

            return documents;
        }
    }
}
