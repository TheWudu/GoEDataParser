using System.Net;
using System.Text;
using System.Text.Json;

namespace GoEDataParser.Downloader.ConsumptionDownloader;

public class NetOoeConsumptionDownloader : IConsumptionDownloader
{
    private const string BaseUrl = "https://eservice.netzooe.at";
    private readonly HttpClientHandler _handler;
    private readonly HttpClient _client;
    
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true, WriteIndented = true};

    private string _contractAccountNumber = string.Empty;
    private string _businessPartnerNumber = string.Empty;
    private string _xsrfToken = string.Empty;
    
    private string _egId = string.Empty;
    private string _egName = string.Empty;
    private string _smartMeterPoint = string.Empty;

    public NetOoeConsumptionDownloader()
    {
        var cookieContainer = new CookieContainer();

        _handler = new HttpClientHandler
        {
            CookieContainer = cookieContainer,
            AutomaticDecompression = DecompressionMethods.All,
            UseCookies = true,
            AllowAutoRedirect = false
        };

        _client = new HttpClient(_handler);
        _client.BaseAddress = new Uri(BaseUrl);
    }

    public async Task<bool> Login(string username, string password)
    {
        // ------------------------
        // 1. LOGIN
        // ------------------------
        var json = $"{{\"j_username\":\"{username}\",\"j_password\":\"{password}\"}}";

        var request = new HttpRequestMessage(HttpMethod.Post, "/service/j_security_check");
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        request.Headers.Add("Accept", "application/json, text/plain, */*");
        request.Headers.Add("client-id", "netzonline");

        var loginResponse = await _client.SendAsync(request);
        loginResponse.EnsureSuccessStatusCode();

        Console.WriteLine($"Login: {loginResponse.StatusCode}");
        
        return await GetSession() && await GetXsrfToken();
    }

    private async Task<bool> GetSession()
    {
        var sessionResponse = await _client.GetAsync("/service/v1.0/session");
        sessionResponse.EnsureSuccessStatusCode();

        Console.WriteLine($"Session: {sessionResponse.StatusCode}");
        
        return sessionResponse.IsSuccessStatusCode;
    }

    private async Task<bool> GetXsrfToken()
    {
        // ------------------------
        // 2. GET CSRF TOKEN
        // ------------------------
        var csrfResponse = await _client.GetAsync("/service/v1.0/session/csrf");
        csrfResponse.EnsureSuccessStatusCode();

        // Extract XSRF-TOKEN from cookies
        var cookies = _handler.CookieContainer.GetCookies(new Uri(BaseUrl));

        foreach (Cookie cookie in cookies)
        {
            if (cookie.Name == "XSRF-TOKEN")
            {
                _xsrfToken = cookie.Value;
            }
        }

        if (_xsrfToken == null)
            throw new Exception("CSRF token not found");

        return csrfResponse.IsSuccessStatusCode;
    }

    private async Task<bool> GetDashboardData()
    {
        // Get data for download
        // dashboard
        // /service/v1.0/dashboard

        var dashboardResponse = await _client.GetAsync("/service/v1.0/dashboard");
        dashboardResponse.EnsureSuccessStatusCode();

        var body = await dashboardResponse.Content.ReadAsStringAsync();
        var dashboard = JsonSerializer.Deserialize<DashboardResponse>(body, _jsonOptions);
        if (dashboard is null)
            throw new Exception("Unable to parse dashbaord data");

        var contractAccount = dashboard.ContractAccounts.FirstOrDefault(ca =>
            ca.Contracts.FirstOrDefault(c => c.PowerGenerationUnit == false) != null);
        if (contractAccount is null)
            throw new Exception("Unable to find consumption contract");
        
        _contractAccountNumber = contractAccount.ContractAccountNumber;
        _businessPartnerNumber = contractAccount.BusinessPartnerNumber;
        
        Console.WriteLine($"Found businessPartnerNumber: {_businessPartnerNumber}");
        Console.WriteLine($"Found contractAccountNumber: {_contractAccountNumber}");

        return dashboardResponse.IsSuccessStatusCode;
    }

    private async Task<bool> GetContractAccount()
    {
        // get contract account
        // /service/v1.0/contract-accounts/1000059501/200100024429

        var contractAccountResponse =
            await _client.GetAsync($"/service/v1.0/contract-accounts/{_businessPartnerNumber}/{_contractAccountNumber}");
        contractAccountResponse.EnsureSuccessStatusCode();

        var body = await contractAccountResponse.Content.ReadAsStringAsync();
        var contractAccountData = JsonSerializer.Deserialize<ContractAccountResponse>(body, _jsonOptions);
        if(contractAccountData is null)
            throw new Exception("Unable to parse contract account data");

        var egData = contractAccountData.Contracts.First().EnergyCommunityData;
        _egId = egData.Timeslices.First().EnergyCommunityId;
        _egName = egData.Timeslices.First().EnergyCommunityName;

        _smartMeterPoint = contractAccountData.Contracts.First().PointOfDelivery.MeterPointAdministrationNumber;

        Console.WriteLine($"EnergyCommunity: {_egId} / {_egName}");
        Console.WriteLine($"SmartmeterPoint: {_smartMeterPoint}");

        return contractAccountResponse.IsSuccessStatusCode;
    }
    
    public async Task<bool> DownloadCsv(DateTime fromDate, DateTime toDate, string fileName)
    {
        if(!(await GetDashboardData()))
            return false;
           
        if(!(await GetContractAccount()))
            return false;

        // ------------------------
        // 3. DOWNLOAD DATA
        // ------------------------
        var pods = new[]
        {
            new
            {
                energyCommunityId = _egId,
                energyCommunityName = _egName,
                type = "ENERGY_COMMUNITY_CONSUMPTION_PER_CONTRIBUTION_FACTOR",
                bestAvailableGranularity = "QUARTER_OF_AN_HOUR",
                meterPointAdministrationNumber = _smartMeterPoint,
                contractAccountNumber = _contractAccountNumber,
                timerange = new
                {
                    from = fromDate.ToString("yyyy-MM-dd"),
                    to = toDate.ToString("yyyy-MM-dd")
                }
            },
            new
            {
                energyCommunityId = _egId,
                energyCommunityName = _egName,
                type = "ENERGY_COMMUNITY_OWN_COVERAGE",
                bestAvailableGranularity = "QUARTER_OF_AN_HOUR",
                meterPointAdministrationNumber = _smartMeterPoint,
                contractAccountNumber = _contractAccountNumber,
                timerange = new
                {
                    from = fromDate.ToString("yyyy-MM-dd"),
                    to = toDate.ToString("yyyy-MM-dd")
                }
            }
        };

        var csvDownloadObject = new
        {
            pods,
            dimension = "ENERGY",
            typesToExclude = Array.Empty<string>()
        };

        // Convert to JSON string
        string csvDownload = JsonSerializer.Serialize(csvDownloadObject, _jsonOptions);
        
        var request = new HttpRequestMessage(HttpMethod.Post,
            "/service/v1.0/consumptions/profile/active/download");

        request.Content = new StringContent(csvDownload, Encoding.UTF8, "application/json");

        // Required headers
        request.Headers.Add("X-XSRF-TOKEN", _xsrfToken);
        request.Headers.Add("Accept", "application/json, text/plain, */*");

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsByteArrayAsync();

        // Save file
        await File.WriteAllBytesAsync(fileName, content);

        Console.WriteLine($"Download complete, written to: {fileName}");

        return response.IsSuccessStatusCode;
    }

}