using MongoDB.Bson.Serialization.Attributes;
using Repository;

namespace GoEDataParser.Models;

public class Consumption : BaseEntity
{
    public required double Kwh { get; set; }
    public double KwhFromEg { get; set; } = 0;
    public double KwhFromNet { get; set; } = 0;

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public required DateTime StartTime { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public required DateTime EndTime { get; set; }
}
