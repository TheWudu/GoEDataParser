using System.Globalization;
using GoEDataParser.Models;
using Microsoft.VisualBasic.FileIO;

namespace GoEDataParser.Parser
{
    namespace Parser
    {
        public class ConsumptionParser
        {
            private readonly List<Consumption> _consumptions = [];
            private readonly CultureInfo _culture = CultureInfo.CreateSpecificCulture(
                Configuration.Culture()
            );

            public List<Consumption> GetConsumptions()
            {
                return _consumptions;
            }

            public void Parse(string? filepath)
            {
                if (!File.Exists(filepath))
                {
                    Console.WriteLine(
                        "File {0} does not exist in {1}",
                        filepath,
                        Directory.GetCurrentDirectory()
                    );
                    return;
                }

                TextFieldParser tfp = new(filepath);
                tfp.Delimiters = [";"];

                while (!tfp.EndOfData)
                {
                    String[]? currentRow = tfp.ReadFields();

                    // return if csv has invalid format or no content
                    if (currentRow == null || currentRow.Length <= 3)
                    {
                        break;
                    }
                    // skip head row
                    if (currentRow[0] == "Datum")
                    {
                        continue;
                    }

                    DateTime startTime = DateTime
                        .Parse(currentRow[0], _culture, DateTimeStyles.AssumeLocal)
                        .ToUniversalTime();
                    DateTime endTime = startTime.Add(new TimeSpan(0, 15, 0));
                    double kwh = Double.Parse(currentRow[1], NumberStyles.Any, _culture);
                    // Console.WriteLine("{0} - {1}: {2}", startTime, endTime, kwh);

                    Consumption consumption = new()
                    {
                        Kwh = kwh,
                        StartTime = startTime,
                        EndTime = endTime,
                    };
                    _consumptions.Add(consumption);
                }
            }

            public double ConsumpationWhile(DateTime chargeStart, DateTime chargeEnd)
            {
                //
                // ---------CS---------CE---------
                // -------S---S---S---S---S-------
                // ---|---|---|---|---|---|---|---
                var list = _consumptions.FindAll(c =>
                    (c.StartTime > chargeStart && c.StartTime < chargeEnd)
                    || (c.EndTime > chargeStart && c.EndTime < chargeEnd)
                );
                double val = list.Sum(c => c.Kwh);
                // if (
                //     list.Count > 0
                //     && list.ElementAt(0).StartTime.Month == 6
                //     && list.ElementAt(0).StartTime.Day == 6
                // )
                // {
                //     Console.WriteLine("Found {0} entries: {1}", list.Count, val);
                // }

                return val;
            }
        }
    }
}
