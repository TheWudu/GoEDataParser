namespace Charging
{
    public class Evaluator
    {
        public void run(List<Charge> charges)
        {
            Dictionary<string, float> monthly = groupMonthly(charges);
            printGroup(monthly, "month");
        }

        private Dictionary<string, float> groupMonthly(List<Charge> charges)
        {
            Dictionary<string, float> dict_monthly = [];

            foreach (Charge c in charges)
            {
                // Console.WriteLine("From {0} to {1}: {2} -- {3}", c.start_time.ToString("yy.MM.dd"), c.end_time.ToString("yy.MM.dd"), c.kwh, kwh_sum);

                string key = c.start_time.ToString("yyyy.MM");
                if (dict_monthly.TryGetValue(key, out float kwh))
                {
                    kwh += c.kwh;
                    dict_monthly.Remove(key);
                    dict_monthly.Add(key, kwh);
                }
                else
                {
                    dict_monthly.Add(key, c.kwh);
                }
            }

            return dict_monthly;
        }

        public void printGroup(Dictionary<string, float> dict, string grouping)
        {
            float kwh_sum = 0.0F;
            Console.WriteLine("\nSums per {0}: ", grouping);
            foreach (KeyValuePair<string, float> kv in dict)
            {
                kwh_sum += kv.Value;
                Console.WriteLine("{0}: {1:F2}, {2:F2}", kv.Key, kv.Value, kwh_sum);
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
