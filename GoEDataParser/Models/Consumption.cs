namespace GoEDataParser.Models;

public class Consumption
{
    public required double Kwh { get; set; }

    public required DateTime StartTime { get; set; }
    public required DateTime EndTime { get; set; }
}