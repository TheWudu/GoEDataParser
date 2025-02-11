namespace Charging
{
    public class ChargeInfo
    {
        public float KwhSum = 0.0F;
        public float Count = 0;
        public long TimeSum = 0;
        public List<float> KwhValues = [];

        public ChargeInfo(float kwhSum, int count, long timeSum)
        {
            this.KwhSum = kwhSum;
            this.Count = count;
            this.TimeSum = timeSum;
            this.KwhValues.Add(KwhSum);
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
                // Console.WriteLine("From {0} to {1}: {2} -- {3}", c.StartTime.ToString("yy.MM.dd"), c.EndTime.ToString("yy.MM.dd"), c.Kwh, Kwh_sum);

                string key = c.StartTime.ToString(timecode);
                if (dict_monthly.TryGetValue(key, out ChargeInfo? info))
                {
                    info.KwhSum += c.Kwh;
                    info.Count += 1;
                    info.KwhValues.Add(c.Kwh);
                    info.TimeSum += c.SecondsCharged;
                }
                else
                {
                    dict_monthly.Add(key, new ChargeInfo(c.Kwh, 1, c.SecondsCharged));
                }
            }

            return dict_monthly;
        }

        public void PrintGroup(Dictionary<string, ChargeInfo> dict, string grouping)
        {
            float kwhSum = 0.0F;
            long timeSum = 0;
            Console.WriteLine("\nSums per {0}: ", grouping);
            foreach (KeyValuePair<string, ChargeInfo> kv in dict)
            {
                kwhSum += kv.Value.KwhSum;
                timeSum += kv.Value.TimeSum;
                float kwhAvg = kv.Value.KwhValues.Average();
                float kwhMax = kv.Value.KwhValues.Max();
                Console.WriteLine(
                    "{0}: {1,6:F2}, {2,7:F2} (Count: {3,2}, Max: {4:F2}, Avg: {5,5:F2}, TimeSum: {6})",
                    kv.Key,
                    kv.Value.KwhSum,
                    kwhSum,
                    kv.Value.Count,
                    kwhMax,
                    kwhAvg,
                    TimeSpan.FromSeconds(kv.Value.TimeSum).ToString()
                );
            }
            Console.WriteLine(
                "\n\e[31mOverall sum:\e[37m {0} in {1}",
                kwhSum,
                TimeSpan.FromSeconds(timeSum).ToString()
            );
        }

        public void evaluate_meter_values(Charge prev, Charge curr)
        {
            if (prev.MeterEnd + 1.0F < curr.MeterStart)
            {
                Console.WriteLine(
                    "{0} {1} -> {2} {3}",
                    prev.EndTime,
                    prev.MeterEnd,
                    curr.MeterStart,
                    curr.StartTime
                );
            }
        }
    }
}
