using System.Configuration;

namespace Charging
{
    class Configuration
    {
        public static string? Token()
        {
            return ConfigurationManager.AppSettings.Get("Token");
        }

        public static string Culture()
        {
            string? culture = ConfigurationManager.AppSettings.Get("Culture");
            if (culture is null)
            {
                return "de-DE";
            }

            return culture;
        }

        public static string MongoDbName()
        {
            string? name = ConfigurationManager.AppSettings.Get("MongoDbName");
            if (name is null)
            {
                return "goe_default";
            }

            return name;
        }

        public static string MongoDbHost()
        {
            string? name = ConfigurationManager.AppSettings.Get("MongoDbHost");
            if (name is null)
            {
                return "localhost";
            }

            return name;
        }
    }
}
