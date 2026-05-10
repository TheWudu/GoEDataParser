namespace GoEDataParser.Downloader.ConsumptionDownloader;

public record DashboardResponse
{
    // {
    public IList<BusinessPartner> BusinessPartners { get; init; } = [];

    public IList<ContractAccount> ContractAccounts { get; init; } = [];
}

public record ContractAccount
{
    public string ContractAccountNumber { get; init; }
    public string BusinessPartnerNumber { get; init; }
    public List<Contract> Contracts { get; init; } = [];
    public int NumberOfContracts  { get; init; }
    public string Branch  { get; init; }
}

public record BusinessPartner
{
    public string Type { get; init;  }
    public string BusinessPartnerNumber { get; init; }
}