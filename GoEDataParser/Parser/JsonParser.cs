using System.Globalization;
using System.Text.Json;
using GoEDataParser.Models;
using GoEDataParser.Utils.Utils;

namespace GoEDataParser.Parser
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
        private readonly List<Charge> _charges = [];
        private const string BaseUrl = "https://data.v3.go-e.io/api/v1/direct_json";
        private readonly string? _token = null; // fetched from App.config
        private const string Timezone = "Europe%2FVienna";
        private const long From = 1682892000000; // 01.05.2023
        private const long To = 1767218340000; // 31.12.2025

        private readonly HttpClient? _client;
        private readonly CultureInfo _culture = CultureInfo.CreateSpecificCulture(
            Configuration.Culture()
        );

        public JsonParser(HttpClient? client)
        {
            this._client = client;
            this._token = Configuration.Token();
        }

        public List<Charge> GetCharges()
        {
            return _charges;
        }

        public void Load()
        {
            string jsonData = Time.MeasureTime("Fetching ... ", codeBlock: FetchJson);
            JsonData? data = Time.MeasureTime(
                "Deserializing ... ",
                codeBlock: () => Deserialize(jsonData)
            );
            Time.MeasureTimeVoid("Parsing ... ", codeBlock: () => ParseData(data));
        }

        private string FetchJson()
        {
            if (_client is null)
                throw new NullReferenceException("No client configured");

            Task<string> stream = _client.GetStringAsync(Url());
            return stream.Result;
        }

        private string Url()
        {
            return BaseUrl
                + "?e="
                + _token
                + "&timezone="
                + Timezone
                + "&from="
                + From.ToString()
                + "&to="
                + To.ToString();
        }

        private JsonData? Deserialize(string jsonData)
        {
            return JsonSerializer.Deserialize<JsonData>(jsonData);
        }

        private void ParseData(JsonData? data)
        {
            if (data?.data is null)
            {
                return;
            }

            foreach (Item item in data.data)
            {
                if (item.start is null || item.end is null)
                {
                    continue;
                }
                string sessionId = item.session_identifier ?? Guid.NewGuid().ToString();
                Charge charge = new()
                {
                    SessionId = sessionId,
                    Kwh = item.energy is { } v ? v : 0.0F,
                    StartTime = DateTime.Parse(
                        (string)item.start,
                        _culture,
                        DateTimeStyles.AssumeLocal
                    ),
                    EndTime = DateTime.Parse(
                        (string)item.end,
                        _culture,
                        DateTimeStyles.AssumeLocal
                    ),
                    MeterDiff = item.eto_diff,
                    MeterStart = item.eto_start,
                    MeterEnd = item.eto_end,
                    SecondsCharged = item.seconds_charged is not null
                        ? (long)TimeOnly.Parse(item.seconds_charged).ToTimeSpan().TotalSeconds
                        : 0,
                };
                _charges.Add(charge);
            }
        }
    }
}
