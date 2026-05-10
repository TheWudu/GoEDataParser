namespace GoEDataParser.Downloader.ConsumptionDownloader;

public record Contract
{
    public string ContractNumber { get; init; } = default!;
    public string Branch { get; init; } = default!;
    public string ScaleType { get; init; } = default!;
    public bool Active { get; init; }
    public DateOnly MoveInDate { get; init; }
    public DateOnly MoveOutDate { get; init; }

    public Consumptions Consumptions { get; init; } = default!;
    public ReadingsHistory ReadingsHistory { get; init; } = default!;
    public EditableReadings EditableReadings { get; init; } = default!;
    public PointOfDelivery PointOfDelivery { get; init; } = default!;

    public string SmartMeterType { get; init; } = default!;
    public string SmartMeterTypeName { get; init; } = default!;
    public string SmartMeterTypeHelp { get; init; } = default!;

    public bool PowerGenerationUnit { get; init; }

    public string Station { get; init; } = default!;
    public string SubStation { get; init; } = default!;

    public GenerationData GenerationData { get; init; } = default!;
    public EnergyCommunityData EnergyCommunityData { get; init; } = default!;
    public Supplier Supplier { get; init; } = default!;

    public string SynthProfile { get; init; } = default!;

    public bool SmartMeterActivationPossible { get; init; }
    public bool LoadProfileActivationPossible { get; init; }
    public bool DailyProfileDispatchActive { get; init; }
    public bool MonthlyProfileDispatchActive { get; init; }
    public bool DailyProfileDispatchInactive { get; init; }
    public bool MonthlyProfileDispatchInactive { get; init; }
    public bool AmisActive { get; init; }
    public bool LoadCurveActive { get; init; }

    public string MonthlyProfileDispatchStatus { get; init; } = default!;
    public string DailyProfileDispatchStatus { get; init; } = default!;

    public bool NonSmart { get; init; }
    public bool AmisMeter { get; init; }
    public bool ProfileActive { get; init; }
    public bool DeviceKeyAvailable { get; init; }
    public bool NewReadingPossible { get; init; }
    public bool OptInPossible { get; init; }
    public bool ReactiveCurrentProfilePresent { get; init; }

    public string DeviceKeyStatus { get; init; } = default!;

    public List<string> AvailableProfileTypes { get; init; } = [];
}

public record Consumptions
{
    public List<ConsumptionValue> Values { get; init; } = [];
    public int TotalConsumption { get; init; }
    public ConsumptionValue LargestConsumption { get; init; } = default!;
}

public record ConsumptionValue
{
    public DateOnly From { get; init; }
    public DateOnly To { get; init; }
    public double Value { get; init; }
    public int NrOfDays { get; init; }
    public double ConsumptionPerDay { get; init; }
}

public record ReadingsHistory
{
    public double CalculatedConsumptionSum { get; init; }
    public double RelevantConsumptionSum { get; init; }
    public string? RelevantConsumptionUnit { get; init; }
    public double MaxConsumptionPerDay { get; init; }

    public Dictionary<string, object>? ReadingsPerMeter { get; init; }
}

public record EditableReadings
{
    public bool NewReadingPossible { get; init; }
}

public record PointOfDelivery
{
    public string MeterPointAdministrationNumber { get; init; } = default!;
    public Meter Meter { get; init; } = default!;

    public List<Profile> Profiles { get; init; } = [];

    public string ActivationStatus { get; init; } = default!;
    public string DailyDispatchStatus { get; init; } = default!;
    public string MonthlyDispatchStatus { get; init; } = default!;

    public DateOnly RetroactiveActivationDate { get; init; }

    public string DeviceKeyStatus { get; init; } = default!;
    public string SnapStatus { get; init; } = default!;

    public Trend MonthlyTrend { get; init; } = default!;
    public Trend YearlyTrend { get; init; } = default!;

    public LastReadings LastReadings { get; init; } = default!;

    public DateOnly MinimumDate { get; init; }
    public DateOnly MaximumDate { get; init; }

    public bool SmartMeterActive { get; init; }
    public bool LoadProfileActive { get; init; }

    public List<string> AvailableProfileTypes { get; init; } = [];

    public DateRange AvailableTimeRange { get; init; } = default!;
}

public record Meter
{
    public string MeterNumber { get; init; } = default!;
}

public record Profile
{
    public DateOnly From { get; init; }
    public DateOnly To { get; init; }

    public string Granularity { get; init; } = default!;
    public string ProfileType { get; init; } = default!;
}

public record Trend
{
    public TrendConsumption ConsumptionOld { get; init; } = default!;
    public TrendConsumption ConsumptionNew { get; init; } = default!;

    public DateTimeRange TimerangeOld { get; init; } = default!;
    public DateTimeRange TimerangeNew { get; init; } = default!;
}

public record TrendConsumption
{
    public double Sum { get; init; }
    public double PerDay { get; init; }
    public int Days { get; init; }
}

public record DateTimeRange
{
    public DateTime From { get; init; }
    public DateTime To { get; init; }
}

public record LastReadings
{
    public List<ReadingValue> Values { get; init; } = [];
    public bool NewReadingPossible { get; init; }
}

public record ReadingValue
{
    public string Meternumber { get; init; } = default!;
    public string Equipmentnumber { get; init; } = default!;
    public string Registernumber { get; init; } = default!;

    public int IntegerPlaces { get; init; }
    public double DecimalPlaces { get; init; }

    public string ReferenceNumber { get; init; } = default!;

    public double CaloricValue { get; init; }
    public double AdditionalValue { get; init; }

    public ReadingResult OldResult { get; init; } = default!;
    public ReadingResultWithTimestamp NewResult { get; init; } = default!;

    public double CalculatedConsumption { get; init; }

    public string UnitForCalculatedConsumption { get; init; } = default!;

    public double RelevantConsumption { get; init; }
}

public record ReadingResult
{
    public int IntegerPlaces { get; init; }
    public double DecimalPlaces { get; init; }
    public bool Plausible { get; init; }
    public double ReadingValue { get; init; }
}

public record ReadingResultWithTimestamp
{
    public DateTime Timestamp { get; init; }
    public int IntegerPlaces { get; init; }
    public double DecimalPlaces { get; init; }
    public bool Plausible { get; init; }
    public double ReadingValue { get; init; }
}

public record DateRange
{
    public DateOnly From { get; init; }
    public DateOnly To { get; init; }
}

public record GenerationData
{
    public bool RecentlyApprovedInFeed { get; init; }
    public bool InFeederInquiryPossible { get; init; }
    public bool SmallInFeederActivationPossible { get; init; }
    public bool SmallInFeederDeactivationPossible { get; init; }
    public bool ShowAnetteLight { get; init; }

    public string TrafficLightColor { get; init; } = default!;
    public string TrafficLightReason { get; init; } = default!;
}

public record EnergyCommunityData
{
    public string Status { get; init; } = default!;

    public List<EnergyCommunityTimeslice> Timeslices { get; init; } = [];

    public bool EnergyCommunityActive { get; init; }
}

public record EnergyCommunityTimeslice
{
    public string EnergyCommunityId { get; init; } = default!;
    public string EnergyCommunityName { get; init; } = default!;

    public DateOnly From { get; init; }
    public DateOnly To { get; init; }

    public string Status { get; init; } = default!;
    public string StatusText { get; init; } = default!;

    public List<Profile> Profiles { get; init; } = [];

    public DateOnly ProfileDataAvailableFrom { get; init; }
    public DateOnly ProfileDataAvailableTo { get; init; }
}

public record Supplier
{
    public string Id { get; init; } = default!;
    public string Name { get; init; } = default!;
}