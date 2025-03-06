using System.Configuration;

namespace Charging
{
    public class Configuration
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
            string? host = ConfigurationManager.AppSettings.Get("MongoDbHost");
            if (host is null)
            {
                return "localhost";
            }

            return host;
        }

        public static string MysqlDbHost()
        {
            string? host = ConfigurationManager.AppSettings.Get("MysqlDbHost");
            if (host is null)
            {
                return "localhost";
            }

            return host;
        }

        public static string MysqlDbName()
        {
            string? name = ConfigurationManager.AppSettings.Get("MysqlDbName");
            if (name is null)
            {
                return "goe_default";
            }

            return name;
        }

        public static string MysqlDbUser()
        {
            string? user = ConfigurationManager.AppSettings.Get("MongoDbUser");
            if (user is null)
            {
                return "root";
            }

            return user;
        }

        public static string MysqlDbPassword()
        {
            string? password = ConfigurationManager.AppSettings.Get("MysqlDbPassword");
            if (password is null)
            {
                return "";
            }

            return password;
        }
    }
}
