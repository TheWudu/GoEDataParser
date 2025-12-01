namespace GoEDataParser.Models
{
    public class ChargeInfo
    {
        public string TimeKey = "";
        public double KwhSum;
        public int Count;
        public long TimeSum;
        public double Missing;
        public double Consumption;
        public double ConsumptionFromEg;
        public List<double> KwhValues = [];

        public ChargeInfo() { }

        public ChargeInfo(
            string timeKey,
            double kwhSum,
            int count,
            long timeSum,
            double missing,
            double consumption = 0.0,
            double consumptionFromEg = 0.0
        )
        {
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
