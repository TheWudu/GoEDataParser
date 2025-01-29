namespace Charging
{
    public class Charge
    {
        public float kwh;
        public DateTime start_time;
        public DateTime end_time;
        public float? meter_start;
        public float? meter_end;
        public float? meter_diff;
    };
}
