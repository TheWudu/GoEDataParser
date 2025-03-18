namespace GoEDataParser.Models
{
    public class ChargeInfo
    {
        public string TimeKey = "";
        public double KwhSum;
        public int Count;
        public long TimeSum;
        public double Missing;
        public List<double> KwhValues = [];

        public ChargeInfo() { }

        public ChargeInfo(string timeKey, double kwhSum, int count, long timeSum, double missing)
        {
            TimeKey = timeKey;
            KwhSum = kwhSum;
            Count = count;
            TimeSum = timeSum;
            Missing = missing;
            KwhValues.Add(KwhSum);
        }
    }
}
