namespace GoEDataParser.Downloader.ConsumptionDownloader;

public record ContractAccountResponse
{
    public string ContractAccountNumber { get; init; } = default!;
    public string BusinessPartnerNumber { get; init; } = default!;
    public string Description { get; init; } = default!;
    public bool Active { get; init; }
    public string Branch { get; init; } = default!;
    public Address Address { get; init; } = default!;
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
    public string Street { get; init; } = default!;
    public string Housenumber { get; init; } = default!;
    public string Postcode { get; init; } = default!;
    public string City { get; init; } = default!;
    public string Country { get; init; } = default!;
}