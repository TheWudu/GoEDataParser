namespace GoEDataParser.Downloader.ConsumptionDownloader;

public record Contract
{
    public string ContractNumber { get; init; } = null!;
    public string Branch { get; init; } = null!;
    public string ScaleType { get; init; } = null!;
    public bool Active { get; init; }
    public DateOnly MoveInDate { get; init; }
    public DateOnly MoveOutDate { get; init; }

    public Consumptions Consumptions { get; init; } = null!;
    public ReadingsHistory ReadingsHistory { get; init; } = null!;
    public EditableReadings EditableReadings { get; init; } = null!;
    public PointOfDelivery PointOfDelivery { get; init; } = null!;

    public string SmartMeterType { get; init; } = null!;
    public string SmartMeterTypeName { get; init; } = null!;
    public string SmartMeterTypeHelp { get; init; } = null!;

    public bool PowerGenerationUnit { get; init; }

    public string Station { get; init; } = null!;
    public string SubStation { get; init; } = null!;

    public GenerationData GenerationData { get; init; } = null!;
    public EnergyCommunityData EnergyCommunityData { get; init; } = null!;
    public Supplier Supplier { get; init; } = null!;

    public string SynthProfile { get; init; } = null!;

    public bool SmartMeterActivationPossible { get; init; }
    public bool LoadProfileActivationPossible { get; init; }
    public bool DailyProfileDispatchActive { get; init; }
    public bool MonthlyProfileDispatchActive { get; init; }
    public bool DailyProfileDispatchInactive { get; init; }
    public bool MonthlyProfileDispatchInactive { get; init; }
    public bool AmisActive { get; init; }
    public bool LoadCurveActive { get; init; }

    public string MonthlyProfileDispatchStatus { get; init; } = null!;
    public string DailyProfileDispatchStatus { get; init; } = null!;

    public bool NonSmart { get; init; }
    public bool AmisMeter { get; init; }
    public bool ProfileActive { get; init; }
    public bool DeviceKeyAvailable { get; init; }
    public bool NewReadingPossible { get; init; }
    public bool OptInPossible { get; init; }
    public bool ReactiveCurrentProfilePresent { get; init; }

    public string DeviceKeyStatus { get; init; } = null!;

    public List<string> AvailableProfileTypes { get; init; } = [];
}

public record Consumptions
{
    public List<ConsumptionValue> Values { get; init; } = [];
    public int TotalConsumption { get; init; }
    public ConsumptionValue LargestConsumption { get; init; } = null!;
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
    public string MeterPointAdministrationNumber { get; init; } = null!;
    public Meter Meter { get; init; } = null!;

    public List<Profile> Profiles { get; init; } = [];

    public string ActivationStatus { get; init; } = null!;
    public string DailyDispatchStatus { get; init; } = null!;
    public string MonthlyDispatchStatus { get; init; } = null!;

    public DateOnly RetroactiveActivationDate { get; init; }

    public string DeviceKeyStatus { get; init; } = null!;
    public string SnapStatus { get; init; } = null!;

    public Trend MonthlyTrend { get; init; } = null!;
    public Trend YearlyTrend { get; init; } = null!;

    public LastReadings LastReadings { get; init; } = null!;

    public DateOnly MinimumDate { get; init; }
    public DateOnly MaximumDate { get; init; }

    public bool SmartMeterActive { get; init; }
    public bool LoadProfileActive { get; init; }

    public List<string> AvailableProfileTypes { get; init; } = [];

    public DateRange AvailableTimeRange { get; init; } = null!;
}

public record Meter
{
    public string MeterNumber { get; init; } = null!;
}

public record Profile
{
    public DateOnly From { get; init; }
    public DateOnly To { get; init; }

    public string Granularity { get; init; } = null!;
    public string ProfileType { get; init; } = null!;
}

public record Trend
{
    public TrendConsumption ConsumptionOld { get; init; } = null!;
    public TrendConsumption ConsumptionNew { get; init; } = null!;

    public DateTimeRange TimerangeOld { get; init; } = null!;
    public DateTimeRange TimerangeNew { get; init; } = null!;
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
    public string Meternumber { get; init; } = null!;
    public string Equipmentnumber { get; init; } = null!;
    public string Registernumber { get; init; } = null!;

    public int IntegerPlaces { get; init; }
    public double DecimalPlaces { get; init; }

    public string ReferenceNumber { get; init; } = null!;

    public double CaloricValue { get; init; }
    public double AdditionalValue { get; init; }

    public ReadingResult OldResult { get; init; } = null!;
    public ReadingResultWithTimestamp NewResult { get; init; } = null!;

    public double CalculatedConsumption { get; init; }

    public string UnitForCalculatedConsumption { get; init; } = null!;

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

    public string TrafficLightColor { get; init; } = null!;
    public string TrafficLightReason { get; init; } = null!;
}

public record EnergyCommunityData
{
    public string Status { get; init; } = null!;

    public List<EnergyCommunityTimeslice> Timeslices { get; init; } = [];

    public bool EnergyCommunityActive { get; init; }
}

public record EnergyCommunityTimeslice
{
    public string EnergyCommunityId { get; init; } = null!;
    public string EnergyCommunityName { get; init; } = null!;

    public DateOnly From { get; init; }
    public DateOnly To { get; init; }

    public string Status { get; init; } = null!;
    public string StatusText { get; init; } = null!;

    public List<Profile> Profiles { get; init; } = [];

    public DateOnly ProfileDataAvailableFrom { get; init; }
    public DateOnly ProfileDataAvailableTo { get; init; }
}

public record Supplier
{
    public string Id { get; init; } = null!;
    public string Name { get; init; } = null!;
}