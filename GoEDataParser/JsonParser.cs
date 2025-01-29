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
        public string? session_identifier { get; set; }
        public string? id_chip { get; set; }
        public string? id_chip_uid { get; set; }
        public string? id_chip_name { get; set; }
        public string? start { get; set; }
        public string? end { get; set; }
        public string? seconds_total { get; set; }
        public string? seconds_charged { get; set; }
        public float? max_power { get; set; }
        public float? max_current { get; set; }
        public float? energy { get; set; }
        public float? eto_diff { get; set; }
        public float? eto_start { get; set; }
        public float? eto_end { get; set; }
        public string? wifi { get; set; }
        public string? link { get; set; }
    }

    public class JsonParser
    {
        public List<Charge> charges = [];
        string base_url = "https://data.v3.go-e.io/api/v1/direct_json";
        string? token = ""; // fetched from App.config
        string timezone = "Europe%2FVienna";
        long from = 1682892000000; // 01.05.2023
        long to = 1767218340000; // 31.12.2025

        private HttpClient? client;

        public JsonParser(HttpClient? client)
        {
            this.client = client;
            this.token = Charging.Configuration.Token();
        }

        public void load()
        {
            string json_data = fetch_json();
            JsonData? data = deserialize(json_data);
            parse_data(data);
        }

        private string fetch_json()
        {
            if (client is not null)
            {
                Console.WriteLine("uRL: {0}", Url());

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

        private JsonData? deserialize(string json_data) =>
            JsonSerializer.Deserialize<JsonData>(json_data);

        private void parse_data(JsonData? data)
        {
            if (data is null || data.data is null)
            {
                return;
            }

            CultureInfo culture = CultureInfo.CreateSpecificCulture("de-DE");
            float kwh_sum = 0.0F;
            foreach (Item item in data.data)
            {
                if (item.start is null || item.end is null)
                {
                    continue;
                }
                Charge charge = new()
                {
                    kwh = item.energy is float v ? v : 0.0F,
                    start_time = DateTime.Parse((string)item.start, culture),
                    end_time = DateTime.Parse((string)item.end, culture),
                    meter_diff = item.eto_diff,
                    meter_start = item.eto_start,
                    meter_end = item.eto_end,
                };
                kwh_sum += charge.kwh;
                charges.Add(charge);
            }
            Console.WriteLine("kWh sum: {0}", kwh_sum);
        }
    }
}
