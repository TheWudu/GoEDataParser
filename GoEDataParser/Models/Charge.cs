using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;
using Repository;

namespace GoEDataParser.Models
{
    public class Charge : BaseEntity
    {
        [MaxLength(100)]
        public required string SessionId { get; set; }
        public required double Kwh { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public required DateTime StartTime { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public required DateTime EndTime { get; set; }
        public double? MeterStart { get; set; }
        public double? MeterEnd { get; set; }
        public double? MeterDiff { get; set; }
        public long SecondsCharged { get; set; }

        public void Print()
        {
            Console.WriteLine(
                "{0}: {1} - {2} ({5}) ({4}) => {3}",
                SessionId,
                StartTime, // .ToLocalTime(),
                EndTime, // .ToLocalTime(),
                Kwh,
                SecondsCharged,
                StartTime.Kind
            );
        }

        public void PrintWithConsumption(double consumption, double consumptionFromEg)
        {
            var consFromEg = 0.0;
            if (consumption != 0)
                consFromEg = ((consumptionFromEg / consumption) * 100);

            Console.WriteLine(
                "{0}: {1} - {2} ({5}) ({4,5}) => {3,6:F}; ({6,5:F2} kWh, {7,5:F2} kWh {8,5:F2})",
                SessionId,
                StartTime.ToString("u"), // .ToLocalTime(),
                EndTime.ToString("u"), // .ToLocalTime(),
                Kwh,
                SecondsCharged,
                StartTime.Kind,
                consumption,
                consumptionFromEg,
                consFromEg
            );
        }

        public override bool Equals(object? obj)
        {
            return this.Equals(obj as Charge);
        }

        public bool Equals(Charge? other)
        {
            if (other == null)
            {
                return false;
            }

            if (
                this.Id != other.Id
                || this.Version != Version
                || Math.Abs(this.Kwh - other.Kwh) > Tolerance
                || this.SessionId != other.SessionId
                || this.StartTime.ToUniversalTime() != other.StartTime.ToUniversalTime()
                || this.EndTime.ToUniversalTime() != other.EndTime.ToUniversalTime()
                || this.SecondsCharged != other.SecondsCharged
            )
            {
                return false;
            }
            return true;
        }

        private const double Tolerance = 0.000001;

        public override int GetHashCode()
        {
            int magicPrime = 23;

            // ReSharper disable NonReadonlyMemberInGetHashCode
            return magicPrime * Version.GetHashCode() * SessionId.GetHashCode();
            // ReSharper enable NonReadonlyMemberInGetHashCode
        }
    }
}
