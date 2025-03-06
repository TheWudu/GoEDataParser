using System.Security.Cryptography.X509Certificates;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Charging
{
    public class ChargeMysqlStore : Repository.GenericMysqlStore<Charge>
    {
        public ChargeMysqlStore(string dbHost, string dbName, string dbUser, string dbPassword)
            : base(dbHost, dbName, "charges", dbUser, dbPassword) { }

        public List<Charge> FindByStartDate(DateTime dateTime)
        {
            return Dataset.Where(e => e.StartTime == dateTime).ToList();
        }
    }
}
