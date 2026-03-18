namespace GoEDataParser.Models
{
    public class ChargeInfo
    {
        public string TimeKey { get; set; } = "";
        public int Year { get; set; }
        public double KwhSum { get; set; }
        public int Count { get; set; }
        public long TimeSum { get; set; }
        public double Missing { get; set; }
        public double Consumption { get; set; }
        public double ConsumptionFromEg { get; set; }
        public List<double> KwhValues { get; set; } = [];

        public ChargeInfo() { }

        public ChargeInfo(
            string timeKey,
            int year,
            double kwhSum,
            int count,
            long timeSum,
            double missing,
            double consumption = 0.0,
            double consumptionFromEg = 0.0
        )
        {
            Year = year;
            TimeKey = timeKey;
            KwhSum = kwhSum;
            Count = count;
            TimeSum = timeSum;
            Missing = missing;
            Consumption = consumption;
            ConsumptionFromEg = consumptionFromEg;
            KwhValues.Add(KwhSum);
        }
    }
}
