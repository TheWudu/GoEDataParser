using MongoDB.Bson.Serialization.Attributes;

namespace Charging
{
    public class Charge : Repository.BaseEntity
    {
        public required string SessionId;
        public required double Kwh;

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public required DateTime StartTime;

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public required DateTime EndTime;
        public double? MeterStart;
        public double? MeterEnd;
        public double? MeterDiff;

        public long SecondsCharged;

        public void Print()
        {
            Console.WriteLine(
                "{0}: {1} - {2} ({4}) => {3}",
                SessionId,
                StartTime.ToLocalTime(),
                EndTime.ToLocalTime(),
                Kwh,
                SecondsCharged
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
                || this.Version != this.Version
                || this.Kwh != other.Kwh
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

        public override int GetHashCode()
        {
            int magicPrime = 23;

            return magicPrime
                * (Id is null ? 1 : Id.GetHashCode())
                * Version.GetHashCode()
                * SessionId.GetHashCode()
                * Kwh.GetHashCode()
                * StartTime.GetHashCode()
                * EndTime.GetHashCode()
                * SecondsCharged.GetHashCode();
        }
    }
}
