using System.Globalization;
using Microsoft.VisualBasic.FileIO;

namespace Charging
{
    public class CsvParser
    {
        public List<Charge> charges = [];

        public void parse(string filepath)
        {
            TextFieldParser tfp = new TextFieldParser(filepath);
            tfp.Delimiters = new string[] { ";" };

            CultureInfo culture = CultureInfo.CreateSpecificCulture("de-DE");
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
                    "{0}, {1}, {2} / {3}",
                    currentRow[5],
                    currentRow[6],
                    currentRow[11],
                    kwh_sum
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
