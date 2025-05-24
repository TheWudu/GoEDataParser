using GoEDataParser.Models;
using GoEDataParser.Parser.Parser;

namespace GoEDataParser
{
    public class Evaluator
    {
        private readonly ConsumptionParser? _consumptionParser;

        public Evaluator() { }

        public Evaluator(ConsumptionParser cp)
        {
            _consumptionParser = cp;
        }

        public void Run(List<Charge> charges)
        {
            Dictionary<string, ChargeInfo> monthly = GroupMonthly(charges);
            PrintGroup(monthly, "month");
        }

        public Dictionary<string, ChargeInfo> GroupMonthly(List<Charge> charges)
        {
            return GroupBy(charges, "yyyy.MM");
        }

        public Dictionary<string, ChargeInfo> GroupYearly(List<Charge> charges)
        {
            return GroupBy(charges, "yyyy");
        }

        private Dictionary<string, ChargeInfo> GroupBy(List<Charge> charges, string timecode)
        {
            Dictionary<string, ChargeInfo> dictMonthly = [];
            Charge? prev = null;

            foreach (Charge c in charges)
            {
                double missing = 0.0F;
                // Console.WriteLine("From {0} to {1}: {2} -- {3}", c.StartTime.ToString("yy.MM.dd"), c.EndTime.ToString("yy.MM.dd"), c.Kwh, Kwh_sum);
                if (prev is not null)
                {
                    missing = MissingKwh(prev, c);
                }

                double consumption = 0.0;

                if (_consumptionParser is not null)
                {
                    consumption = _consumptionParser.ConsumpationWhile(
                        c.StartTime,
                        c.StartTime.Add(new TimeSpan(0, 0, (int)c.SecondsCharged))
                    );
                    // if (consumption > 0.0)
                    // {
                    //     var consumptionRate = 1.0;
                    //     if (consumption < c.Kwh)
                    //         consumptionRate = (consumption / c.Kwh);
                    //
                    //     Console.WriteLine(
                    //         "Consumption: {0} / {3} ({4,4:F2} %) at {1} - {2}",
                    //         consumption,
                    //         c.StartTime,
                    //         c.StartTime.Add(new TimeSpan(0, 0, (int)c.SecondsCharged)),
                    //         c.Kwh,
                    //         consumptionRate * 100
                    //     );
                    // }
                }

                if (consumption > c.Kwh)
                {
                    consumption = c.Kwh;
                }

                string key = c.StartTime.ToString(timecode);
                if (dictMonthly.TryGetValue(key, out ChargeInfo? info))
                {
                    info.KwhSum += c.Kwh;
                    info.Count += 1;
                    info.KwhValues.Add(c.Kwh);
                    info.TimeSum += c.SecondsCharged;
                    info.Missing += missing;
                    info.Consumption += consumption;
                }
                else
                {
                    dictMonthly.Add(
                        key,
                        new ChargeInfo(key, c.Kwh, 1, c.SecondsCharged, missing, consumption)
                    );
                }

                prev = c;
            }

            return dictMonthly;
        }

        public void PrintMissing(string key, string? lastKey)
        {
            if (lastKey == null)
                return;

            if (key.Split(".").Length == 1)
                return;

            int year = Convert.ToInt32(key.Split(".")[0]);
            int month = Convert.ToInt32(key.Split(".")[1]);

            int lastYear = Convert.ToInt32(lastKey.Split(".")[0]);
            int lastMonth = Convert.ToInt32(lastKey.Split(".")[1]);

            if (((lastYear + 1) < year) || ((lastMonth + 1) < month && lastMonth != 12))
            {
                for (int m = lastMonth + 1; m < month; m++)
                {
                    Console.WriteLine("{0}.{1,2}: data missing", lastYear, m.ToString("00"));
                }
            }
        }

        public void PrintGroup(Dictionary<string, ChargeInfo> dict, string grouping)
        {
            string? lastKey = null;
            double kwhSum = 0.0F;
            long timeSum = 0;
            Console.WriteLine("\nSums per {0}: ", grouping);
            foreach (KeyValuePair<string, ChargeInfo> kv in dict)
            {
                PrintMissing(kv.Key, lastKey);
                lastKey = kv.Key;
                kwhSum += kv.Value.KwhSum;
                timeSum += kv.Value.TimeSum;
                double kwhAvg = kv.Value.KwhValues.Average();
                double kwhMax = kv.Value.KwhValues.Max();
                Console.WriteLine(
                    "{0}: {1,7:F2}, {2,7:F2} (Count: {3,3}, Max: {4:F2} kWh, Avg: {5,5:F2} kWh, TimeSum: {6,11}, Missing: {7,6:F2} kWh, Consumption: {8,7:F2} ({9,5:F2})",
                    kv.Key,
                    kv.Value.KwhSum,
                    kwhSum,
                    kv.Value.Count,
                    kwhMax,
                    kwhAvg,
                    TimeSpan.FromSeconds(kv.Value.TimeSum).ToString(),
                    kv.Value.Missing,
                    kv.Value.Consumption,
                    ((kv.Value.Consumption / kv.Value.KwhSum) * 100)
                );
            }
            Console.WriteLine(
                "\n\e[31mOverall sum:\e[37m {0:F2} in {1}\n",
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
