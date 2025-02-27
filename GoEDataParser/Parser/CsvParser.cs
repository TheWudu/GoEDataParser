using System.Globalization;
using Microsoft.VisualBasic.FileIO;

namespace Charging
{
    namespace Parser
    {
        public class CsvParser
        {
            private readonly List<Charge> charges = [];
            private readonly CultureInfo culture = CultureInfo.CreateSpecificCulture(
                Charging.Configuration.Culture()
            );

            public List<Charge> GetCharges()
            {
                return charges;
            }

            public void Parse(string filepath)
            {
                if (!File.Exists(filepath))
                {
                    return;
                }

                TextFieldParser tfp = new(filepath);
                tfp.Delimiters = [";"];

                while (!tfp.EndOfData)
                {
                    String[]? currentRow = tfp.ReadFields();

                    // return if csv has invalid format or no content
                    if (currentRow == null || currentRow.Length <= 12)
                    {
                        break;
                    }
                    // skip head row
                    if (currentRow[0] == "Session Number")
                    {
                        continue;
                    }

                    string sessionId = currentRow[1];
                    DateTime StartTime = DateTime.Parse(currentRow[5], culture);
                    DateTime EndTime = DateTime.Parse(currentRow[6], culture);
                    double Kwh = Double.Parse(currentRow[11], NumberStyles.Any, culture);
                    long SecondsCharged = (long)
                        TimeOnly.Parse(currentRow[8]).ToTimeSpan().TotalSeconds;
                    // Console.WriteLine(
                    //     "{0} - {1}: {2} / {3}",
                    //     currentRow[5],
                    //     currentRow[6],
                    //     currentRow[11],
                    //     currentRow[14]
                    // );

                    Charge charge = new()
                    {
                        SessionId = sessionId,
                        Kwh = Kwh,
                        StartTime = StartTime,
                        EndTime = EndTime,
                        SecondsCharged = SecondsCharged,
                    };
                    charges.Add(charge);
                }
            }
        }
    }
}
