using System.Globalization;
using GoEDataParser.Models;
using GoEDataParser.Repository;
using GoEDataParser.Utils.Utils;
using Microsoft.VisualBasic.FileIO;

namespace GoEDataParser.Parser
{
    namespace Parser
    {
        public class ConsumptionParser
        {
            public ConsumptionParser()
            {
                string dbHost = Configuration.MongoDbHost();
                string dbName = Configuration.MongoDbName();
                _consumptionStore = new ConsumptionMongoStore(dbHost, dbName);
            }

            private ConsumptionMongoStore _consumptionStore;
            private readonly List<Consumption> _consumptions = [];
            private readonly CultureInfo _culture = CultureInfo.CreateSpecificCulture(
                Configuration.Culture()
            );

            private readonly string _defaultFilePath = "manager_data_";

            public List<Consumption> GetConsumptions()
            {
                return _consumptions;
            }

            public void ParseFiles(int startYear = 2023)
            {
                int endYear = DateTime.Today.Year;

                for (int year = startYear; year <= endYear; year++)
                {
                    ReadFile(_defaultFilePath + year + ".csv");
                }
            }

            public void ReadFile(string filepath)
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
                int mode = 0;

                while (!tfp.EndOfData)
                {
                    String[]? currentRow = tfp.ReadFields();

                    // return if csv has invalid format or no content
                    if (currentRow == null || currentRow.Length <= 3)
                    {
                        break;
                    }
                    // skip head row
                    if (currentRow[0] == "Ende Ablesezeitraum")
                    {
                        mode = 1;
                        continue;
                    }
                    if (currentRow[0] == "Datum")
                    {
                        mode = 2;
                        continue;
                    }

                    DateTime endTime = DateTime.Now;
                    DateTime startTime = DateTime.Now;
                    double kwh = 0;

                    if (mode == 1)
                    {
                        endTime = DateTime
                            .Parse(currentRow[0], _culture, DateTimeStyles.AssumeUniversal)
                            .ToUniversalTime();
                        startTime = endTime.Subtract(new TimeSpan(0, 15, 0));
                        kwh = Double.Parse(currentRow[3], NumberStyles.Any, _culture);
                    }
                    else if (mode == 2)
                    {
                        startTime = DateTime
                            .Parse(currentRow[0], _culture, DateTimeStyles.AssumeLocal)
                            .ToUniversalTime();
                        endTime = startTime.Add(new TimeSpan(0, 15, 0));
                        kwh = Double.Parse(currentRow[1], NumberStyles.Any, _culture);
                    }

                    // Console.WriteLine(
                    //     "C: {3} == {0} / {2}: {1}",
                    //     startTime,
                    //     kwh,
                    //     endTime,
                    //     currentRow[0]
                    // );

                    Consumption consumption = new()
                    {
                        Kwh = kwh,
                        StartTime = startTime,
                        EndTime = endTime,
                    };
                    _consumptions.Add(consumption);
                }
            }

            public double ConsumpationFromList(DateTime chargeStart, DateTime chargeEnd)
            {
                var list = _consumptions.FindAll(c =>
                    (c.StartTime > chargeStart && c.StartTime < chargeEnd)
                    || (c.EndTime > chargeStart && c.EndTime < chargeEnd)
                );
                return list.Sum(c => c.Kwh);
            }

            public double ConsumptionFromDb(DateTime chargeStart, DateTime chargeEnd)
            {
                var list = _consumptionStore.FindConsumptions(chargeStart, chargeEnd);

                return list.Sum(c => c.Kwh);
            }

            public void ReadConsumptionsFromDb()
            {
                _consumptions.AddRange(_consumptionStore.ReadAll());
            }

            public double ConsumpationWhile(DateTime chargeStart, DateTime chargeEnd)
            {
                if (_consumptions.Count > 0)
                {
                    return ConsumpationFromList(chargeStart, chargeEnd);
                }
                else
                {
                    return ConsumptionFromDb(chargeStart, chargeEnd);
                }
            }

            public void StoreConsumptions()
            {
                foreach (var consumption in (_consumptions))
                {
                    _consumptionStore.Upsert(consumption);
                }
            }
        }
    }
}
