namespace Charging
{
    public class ChargeInfo
    {
        public float KwhSum = 0.0F;
        public float Count = 0;
        public long TimeSum = 0;
        public float Missing = 0.0F;
        public List<float> KwhValues = [];

        public ChargeInfo(float kwhSum, int count, long timeSum, float missing)
        {
            this.KwhSum = kwhSum;
            this.Count = count;
            this.TimeSum = timeSum;
            this.Missing = missing;
            this.KwhValues.Add(KwhSum);
        }
    }
}
