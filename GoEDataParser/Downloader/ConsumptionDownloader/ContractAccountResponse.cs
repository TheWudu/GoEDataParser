namespace GoEDataParser.Downloader.ConsumptionDownloader;

public record ContractAccountResponse
{
    public string ContractAccountNumber { get; init; } = null!;
    public string BusinessPartnerNumber { get; init; } = null!;
    public string Description { get; init; } = null!;
    public bool Active { get; init; }
    public string Branch { get; init; } = null!;
    public Address Address { get; init; } = null!;
    public bool BilledByProvider { get; init; }

    public Dictionary<string, object>? BankAccountIn { get; init; }
    public Dictionary<string, object>? BankAccountOut { get; init; }
    public Dictionary<string, object>? InvoiceSettings { get; init; }

    public List<Contract> Contracts { get; init; } = [];
    public bool ProductChangeAvailable { get; init; }

    public Dictionary<string, object>? DisconnectionNotification { get; init; }

    public bool Editable { get; init; }
}

public record Address
{
    public string Street { get; init; } = null!;
    public string Housenumber { get; init; } = null!;
    public string Postcode { get; init; } = null!;
    public string City { get; init; } = null!;
    public string Country { get; init; } = null!;
}