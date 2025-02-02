namespace Charging
{
    public class Charge
    {
        public string? _id = null;
        public string? session_id;
        public float kwh;
        public DateTime start_time;
        public DateTime end_time;
        public float? meter_start;
        public float? meter_end;
        public float? meter_diff;

        public void Print()
        {
            Console.WriteLine("{0}: {1} - {2} => {3}", session_id, start_time, end_time, kwh);
        }
    };
}
