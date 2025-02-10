namespace Charging
{
    public class ChargeInfo
    {
        public float kwhSum = 0.0F;
        public float count = 0;
        public long timeSum = 0;
        public List<float> kwhValues = [];

        public ChargeInfo(float kwhSum, int count, long timeSum)
        {
            this.kwhSum = kwhSum;
            this.count = count;
            this.timeSum = timeSum;
            this.kwhValues.Add(kwhSum);
        }
    }

    public class Evaluator
    {
        public void run(List<Charge> charges)
        {
            Dictionary<string, ChargeInfo> monthly = groupMonthly(charges);
            PrintGroup(monthly, "month");
        }

        public Dictionary<string, ChargeInfo> groupMonthly(List<Charge> charges)
        {
            return groupBy(charges, "yyyy.MM");
        }

        public Dictionary<string, ChargeInfo> groupYearly(List<Charge> charges)
        {
            return groupBy(charges, "yyyy");
        }

        private Dictionary<string, ChargeInfo> groupBy(List<Charge> charges, string timecode)
        {
            Dictionary<string, ChargeInfo> dict_monthly = [];

            foreach (Charge c in charges)
            {
                // Console.WriteLine("From {0} to {1}: {2} -- {3}", c.start_time.ToString("yy.MM.dd"), c.end_time.ToString("yy.MM.dd"), c.kwh, kwh_sum);

                string key = c.start_time.ToString(timecode);
                if (dict_monthly.TryGetValue(key, out ChargeInfo? info))
                {
                    info.kwhSum += c.kwh;
                    info.count += 1;
                    info.kwhValues.Add(c.kwh);
                    info.timeSum += c.seconds_charged;
                }
                else
                {
                    dict_monthly.Add(key, new ChargeInfo(c.kwh, 1, c.seconds_charged));
                }
            }

            return dict_monthly;
        }

        public void PrintGroup(Dictionary<string, ChargeInfo> dict, string grouping)
        {
            float kwh_sum = 0.0F;
            long time_sum = 0;
            Console.WriteLine("\nSums per {0}: ", grouping);
            foreach (KeyValuePair<string, ChargeInfo> kv in dict)
            {
                kwh_sum += kv.Value.kwhSum;
                time_sum += kv.Value.timeSum;
                float kwh_avg = kv.Value.kwhValues.Average();
                float kwh_max = kv.Value.kwhValues.Max();
                Console.WriteLine(
                    "{0}: {1,6:F2}, {2,7:F2} (Count: {3,2}, Max: {4:F2}, Avg: {5,5:F2}, TimeSum: {6})",
                    kv.Key,
                    kv.Value.kwhSum,
                    kwh_sum,
                    kv.Value.count,
                    kwh_max,
                    kwh_avg,
                    TimeSpan.FromSeconds(kv.Value.timeSum).ToString()
                );
            }
            Console.WriteLine(
                "\n\e[31mOverall sum:\e[37m {0} in {1}",
                kwh_sum,
                TimeSpan.FromSeconds(time_sum).ToString()
            );
        }

        public void evaluate_meter_values(Charge prev, Charge curr)
        {
            if (prev.meter_end + 1.0F < curr.meter_start)
            {
                Console.WriteLine(
                    "{0} {1} -> {2} {3}",
                    prev.end_time,
                    prev.meter_end,
                    curr.meter_start,
                    curr.start_time
                );
            }
        }
    }
}
