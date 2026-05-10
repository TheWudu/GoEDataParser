namespace GoEDataParser.Downloader.ConsumptionDownloader;

public interface IConsumptionDownloader
{
    Task<bool> Login(string username, string password);
    Task<bool> DownloadCsv(DateTime fromDate, DateTime toDate, string fileName);
}