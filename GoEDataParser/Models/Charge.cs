using MongoDB.Bson.Serialization.Attributes;

namespace Charging
{
    public class Charge : Repository.BaseEntity
    {
        public required string session_id;
        public required float kwh;

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public required DateTime start_time;

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public required DateTime end_time;
        public float? meter_start;
        public float? meter_end;
        public float? meter_diff;

        public void Print()
        {
            Console.WriteLine("{0}: {1} - {2} => {3}", session_id, start_time, end_time, kwh);
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
                || this.kwh != other.kwh
                || this.session_id != other.session_id
                || this.start_time.ToUniversalTime() != other.start_time.ToUniversalTime()
                || this.end_time.ToUniversalTime() != other.end_time.ToUniversalTime()
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
                * session_id.GetHashCode()
                * kwh.GetHashCode()
                * start_time.GetHashCode()
                * end_time.GetHashCode();
        }
    }
}
