namespace GoEDataParser.Downloader.ConsumptionDownloader;

public record DashboardResponse
{
    // {
    public IList<BusinessPartner> BusinessPartners { get; init; } = [];

    public IList<ContractAccount> ContractAccounts { get; init; } = [];
}

public record ContractAccount
{
    public required string ContractAccountNumber { get; init; }
    public required string BusinessPartnerNumber { get; init; }
    public required List<Contract> Contracts { get; init; }
    public int NumberOfContracts  { get; init; }
    public string Branch { get; init; } = string.Empty;
}

public record BusinessPartner
{
    public required string Type { get; init;  }
    public required string BusinessPartnerNumber { get; init; }
}