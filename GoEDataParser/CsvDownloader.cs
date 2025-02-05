namespace Charging
{
    public class CsvDownloader
    {
        readonly string base_url = "https://data.v3.go-e.io/api/v1/direct_export";
        readonly string? token = Configuration.Token();
        readonly string parameters =
            "from=1682892000000&to=1767218340000&timezone=Europe%2FVienna&delimiter=%2C&decimalpoint=.";
        public string filepath = "tmp.csv";

        public void run()
        {
            string url = base_url + "?e=" + token + "&" + parameters;

            HttpClient client = new();
            Task<Stream> stream = client.GetStreamAsync(url);

            FileStream fs = new(filepath, FileMode.OpenOrCreate);
            stream.Result.CopyTo(fs);
        }
    }
}
