using System.Globalization;
using Microsoft.VisualBasic.FileIO;

namespace Charging
{
    public class CsvParser
    {
        private List<Charge> charges = [];
        private CultureInfo culture = CultureInfo.CreateSpecificCulture(
            Charging.Configuration.Culture()
        );

        public List<Charge> GetCharges()
        {
            return charges;
        }

        public void parse(string filepath)
        {
            TextFieldParser tfp = new TextFieldParser(filepath);
            tfp.Delimiters = new string[] { ";" };

            float kwh_sum = 0.0F;
            while (!tfp.EndOfData)
            {
                String[]? currentRow = tfp.ReadFields();

                if (currentRow == null)
                {
                    break;
                }
                if (currentRow[5] == "Start")
                {
                    continue;
                }

                DateTime start_time = DateTime.Parse(currentRow[5], culture);
                DateTime end_time = DateTime.Parse(currentRow[6], culture);
                float kwh = float.Parse(currentRow[11], NumberStyles.Any, culture);
                kwh_sum += kwh;
                Console.WriteLine(
                    "{0} - {1}: {2} / {3} -- {4}",
                    currentRow[5],
                    currentRow[6],
                    currentRow[11],
                    kwh_sum,
                    currentRow[14]
                );

                Charge charge = new Charge();
                charge.kwh = kwh;
                charge.start_time = start_time;
                charge.end_time = end_time;
                charges.Add(charge);
            }
        }
    }
}
