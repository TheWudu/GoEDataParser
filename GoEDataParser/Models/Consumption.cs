using MongoDB.Bson.Serialization.Attributes;
using Repository;

namespace GoEDataParser.Models;

public class Consumption : BaseEntity
{
    public required double Kwh { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public required DateTime StartTime { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public required DateTime EndTime { get; set; }
}
