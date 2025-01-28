namespace Charging
{
    public class Evaluator {
        public void run(List<Charge> charges) {

            Dictionary<string, float> dict = [];
            float kwh_sum = 0.0F;

            foreach(Charge c in charges) {
                kwh_sum += c.kwh;
                // Console.WriteLine("From {0} to {1}: {2} -- {3}", c.start_time.ToString("yy.MM.dd"), c.end_time.ToString("yy.MM.dd"), c.kwh, kwh_sum);

                string key = c.start_time.ToString("yyyy.MM");
                if (dict.TryGetValue(key, out float kwh)) {
                    kwh += c.kwh;
                    dict.Remove(key);
                    dict.Add(key, kwh);
                }
                else {
                    dict.Add(key, c.kwh);
                }
            }

            kwh_sum = 0.0F;
            Console.WriteLine("\nSums per month: ");
            foreach(KeyValuePair<string, float> kv in dict) {
                kwh_sum += kv.Value;
                Console.WriteLine("{0}: {1:F2}, {2:F2}", kv.Key, kv.Value, kwh_sum);
            }
            }
        }
}
