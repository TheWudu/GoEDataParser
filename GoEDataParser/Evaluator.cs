namespace Charging
{
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
            Charge? prev = null;

            foreach (Charge c in charges)
            {
                double missing = 0.0F;
                // Console.WriteLine("From {0} to {1}: {2} -- {3}", c.StartTime.ToString("yy.MM.dd"), c.EndTime.ToString("yy.MM.dd"), c.Kwh, Kwh_sum);
                if (prev is not null)
                {
                    missing = MissingKwh(prev, c);
                }

                string key = c.StartTime.ToString(timecode);
                if (dict_monthly.TryGetValue(key, out ChargeInfo? info))
                {
                    info.KwhSum += c.Kwh;
                    info.Count += 1;
                    info.KwhValues.Add(c.Kwh);
                    info.TimeSum += c.SecondsCharged;
                    info.Missing += missing;
                }
                else
                {
                    dict_monthly.Add(key, new ChargeInfo(key, c.Kwh, 1, c.SecondsCharged, missing));
                }

                prev = c;
            }

            return dict_monthly;
        }

        public void PrintGroup(Dictionary<string, ChargeInfo> dict, string grouping)
        {
            double kwhSum = 0.0F;
            long timeSum = 0;
            Console.WriteLine("\nSums per {0}: ", grouping);
            foreach (KeyValuePair<string, ChargeInfo> kv in dict)
            {
                kwhSum += kv.Value.KwhSum;
                timeSum += kv.Value.TimeSum;
                double kwhAvg = kv.Value.KwhValues.Average();
                double kwhMax = kv.Value.KwhValues.Max();
                Console.WriteLine(
                    "{0}: {1,7:F2}, {2,7:F2} (Count: {3,3}, Max: {4:F2} kWh, Avg: {5,5:F2} kWh, TimeSum: {6,11}, Missing: {7:F2} kWh)",
                    kv.Key,
                    kv.Value.KwhSum,
                    kwhSum,
                    kv.Value.Count,
                    kwhMax,
                    kwhAvg,
                    TimeSpan.FromSeconds(kv.Value.TimeSum).ToString(),
                    kv.Value.Missing
                );
            }
            Console.WriteLine(
                "\n\e[31mOverall sum:\e[37m {0} in {1}",
                kwhSum,
                TimeSpan.FromSeconds(timeSum).ToString()
            );
        }

        public double MissingKwh(Charge prev, Charge curr)
        {
            if (prev.MeterEnd is null || curr.MeterStart is null)
            {
                return 0.0F;
            }

            if (prev.MeterEnd + 1.0F < curr.MeterStart)
            {
                // Console.WriteLine(
                //     "{0} {1} -> {2} {3}",
                //     prev.EndTime,
                //     prev.MeterEnd,
                //     curr.MeterStart,
                //     curr.StartTime
                // );

                double diff = (double)curr.MeterStart - (double)prev.MeterEnd;

                return diff;
            }

            return 0.0F;
        }
    }
}
