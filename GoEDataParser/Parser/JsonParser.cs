using System.Globalization;
using System.Runtime.Serialization;
using System.Text.Json;

namespace Charging
{
    public class JsonData
    {
        public List<ColumnItem>? columns { get; set; }
        public List<Item>? data { get; set; }
    }

    public class ColumnItem
    {
        public string? key { get; set; }
        public bool? hide { get; set; }
        public string? type { get; set; }
        public string? unit { get; set; }
    }

    public class Item
    {
        public int? session_number { get; set; }
        public string? SessionIdentifier { get; set; }
        public string? id_chip { get; set; }
        public string? id_chip_uid { get; set; }
        public string? id_chip_name { get; set; }
        public string? start { get; set; }
        public string? end { get; set; }
        public string? seconds_total { get; set; }
        public string? seconds_charged { get; set; }
        public double? max_power { get; set; }
        public double? max_current { get; set; }
        public double? energy { get; set; }
        public double? eto_diff { get; set; }
        public double? eto_start { get; set; }
        public double? eto_end { get; set; }
        public string? wifi { get; set; }
        public string? link { get; set; }
    }

    public class JsonParser
    {
        public List<Charge> charges = [];
        string base_url = "https://data.v3.go-e.io/api/v1/direct_json";
        string? token = null; // fetched from App.config
        string timezone = "Europe%2FVienna";
        long from = 1682892000000; // 01.05.2023
        long to = 1767218340000; // 31.12.2025

        private HttpClient? client;
        private CultureInfo culture = CultureInfo.CreateSpecificCulture(Configuration.Culture());

        public JsonParser(HttpClient? client)
        {
            this.client = client;
            this.token = Charging.Configuration.Token();
        }

        public List<Charge> GetCharges()
        {
            return charges;
        }

        public void load()
        {
            string json_data = FetchJson();
            JsonData? data = Deserialize(json_data);
            ParseData(data);
        }

        private string FetchJson()
        {
            if (client is not null)
            {
                // Console.WriteLine("uRL: {0}", Url());
                Task<string> stream = client.GetStringAsync(Url());
                return stream.Result;
            }

            throw new NullReferenceException("No client configured");
        }

        private string Url()
        {
            return base_url
                + "?e="
                + token
                + "&timezone="
                + timezone
                + "&from="
                + from.ToString()
                + "&to="
                + to.ToString();
        }

        private JsonData? Deserialize(string json_data)
        {
            return JsonSerializer.Deserialize<JsonData>(json_data);
        }

        private void ParseData(JsonData? data)
        {
            if (data is null || data.data is null)
            {
                return;
            }

            double kwhSum = 0.0F;
            foreach (Item item in data.data)
            {
                if (item.start is null || item.end is null)
                {
                    continue;
                }
                string SessionId = item.SessionIdentifier is not null
                    ? item.SessionIdentifier
                    : Guid.NewGuid().ToString();
                Charge charge = new()
                {
                    SessionId = SessionId,
                    Kwh = item.energy is double v ? v : 0.0F,
                    StartTime = DateTime.Parse(
                        (string)item.start,
                        culture,
                        DateTimeStyles.AssumeLocal
                    ),
                    EndTime = DateTime.Parse((string)item.end, culture, DateTimeStyles.AssumeLocal),
                    MeterDiff = item.eto_diff,
                    MeterStart = item.eto_start,
                    MeterEnd = item.eto_end,
                    SecondsCharged = item.seconds_charged is not null
                        ? (long)TimeOnly.Parse(item.seconds_charged).ToTimeSpan().TotalSeconds
                        : 0,
                };
                kwhSum += charge.Kwh;
                charges.Add(charge);
            }
            Console.WriteLine("Kwh sum: {0}", kwhSum);
        }
    }
}
