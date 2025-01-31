namespace Charging
{
    public class ChargeInfo
    {
        public float kwhSum = 0.0F;
        public float count = 0;

        public ChargeInfo(float kwhSum, int count)
        {
            this.kwhSum = kwhSum;
            this.count = count;
        }
    }

    public class Evaluator
    {
        public void run(List<Charge> charges)
        {
            Dictionary<string, ChargeInfo> monthly = groupMonthly(charges);
            printGroup(monthly, "month");
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
                }
                else
                {
                    dict_monthly.Add(key, new ChargeInfo(c.kwh, 1));
                }
            }

            return dict_monthly;
        }

        public void printGroup(Dictionary<string, ChargeInfo> dict, string grouping)
        {
            float kwh_sum = 0.0F;
            Console.WriteLine("\nSums per {0}: ", grouping);
            foreach (KeyValuePair<string, ChargeInfo> kv in dict)
            {
                kwh_sum += kv.Value.kwhSum;
                Console.WriteLine(
                    "{0}: {1:F2}, {2:F2} ({3})",
                    kv.Key,
                    kv.Value.kwhSum,
                    kwh_sum,
                    kv.Value.count
                );
            }
            Console.WriteLine("\n\e[31mOverall sum:\e[37m {0}", kwh_sum);
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
