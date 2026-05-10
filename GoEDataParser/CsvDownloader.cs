namespace GoEDataParser
{
    public class CsvDownloader
    {
        readonly string _baseUrl = "https://data.v3.go-e.io/api/v1/direct_export";
        readonly string _token = Configuration.Token();
        readonly string _parameters =
            "from=1682892000000&to=1767218340000&timezone=Europe%2FVienna&delimiter=%2C&decimalpoint=.";

        public string? Filepath;
        
        private readonly HttpClient _client = new();

        public void Run()
        {
            string url = _baseUrl + "?e=" + _token + "&" + _parameters;
            
            Task<Stream> stream = _client.GetStreamAsync(url);

            Filepath = "tmp/" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
            FileStream fs = new(Filepath, FileMode.OpenOrCreate);
            stream.Result.CopyTo(fs);
        }
    }
}
