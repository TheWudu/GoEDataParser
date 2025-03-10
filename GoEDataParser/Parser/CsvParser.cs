using System.Globalization;
using GoEDataParser.Models;
using Microsoft.VisualBasic.FileIO;

namespace GoEDataParser.Parser
{
    namespace Parser
    {
        public class CsvParser
        {
            private readonly List<Charge> _charges = [];
            private readonly CultureInfo _culture = CultureInfo.CreateSpecificCulture(
                Configuration.Culture()
            );

            public List<Charge> GetCharges()
            {
                return _charges;
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
                    DateTime startTime = DateTime.Parse(currentRow[5], _culture);
                    DateTime endTime = DateTime.Parse(currentRow[6], _culture);
                    double kwh = Double.Parse(currentRow[11], NumberStyles.Any, _culture);
                    long secondsCharged = (long)
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
                        Kwh = kwh,
                        StartTime = startTime,
                        EndTime = endTime,
                        SecondsCharged = secondsCharged,
                    };
                    _charges.Add(charge);
                }
            }
        }
    }
}
