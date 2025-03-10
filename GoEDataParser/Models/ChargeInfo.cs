namespace GoEDataParser.Models
{
    public class ChargeInfo
    {
        public string TimeKey = "";
        public double KwhSum = 0.0;
        public int Count = 0;
        public long TimeSum = 0;
        public double Missing = 0.0;
        public List<double> KwhValues = [];

        public ChargeInfo() { }

        public ChargeInfo(string timeKey, double kwhSum, int count, long timeSum, double missing)
        {
            this.TimeKey = timeKey;
            this.KwhSum = kwhSum;
            this.Count = count;
            this.TimeSum = timeSum;
            this.Missing = missing;
            this.KwhValues.Add(KwhSum);
        }
    }
}
