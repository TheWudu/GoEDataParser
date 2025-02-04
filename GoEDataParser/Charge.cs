using MongoDB.Bson.Serialization.Attributes;

namespace Charging
{
    public class Charge
    {
        public string? _id = null;
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
                this._id != other._id
                || this.kwh != other.kwh
                || this.session_id != other.session_id
                || !this.start_time.Equals(other.start_time)
                || !this.end_time.Equals(other.end_time)
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
                * (_id is null ? 1 : _id.GetHashCode())
                * session_id.GetHashCode()
                * kwh.GetHashCode()
                * start_time.GetHashCode()
                * end_time.GetHashCode();
        }
    }
}
