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
    }
}
