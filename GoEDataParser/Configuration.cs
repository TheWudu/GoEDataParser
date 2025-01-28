using System.Configuration;

namespace Charging
{
    class Configuration
    {
        public static string? Token()
        {
            return ConfigurationManager.AppSettings.Get("Token");
        }
    }
}
