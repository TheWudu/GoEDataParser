using System.Globalization;
using GoEDataParser.Models;
using GoEDataParser.Repository;
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
                    else if (currentRow[0] == "Datum")
                    {
                        mode = 2;
                        continue;
                    }
                    else if (currentRow[0] == "Startdatum")
                    {
                        mode = 3;
                        continue;
                    }

                    DateTime endTime = DateTime.Now;
                    DateTime startTime = DateTime.Now;
                    double kwh = 0;
                    double kwhFromEg = 0;
                    double kwhFromNet = 0;

                    if (mode == 1)
                    {
                        endTime = DateTime
                            .Parse(currentRow[0], _culture, DateTimeStyles.AssumeUniversal)
                            .ToUniversalTime();
                        startTime = endTime.Subtract(new TimeSpan(0, 15, 0));
                        kwh = Double.Parse(currentRow[3], NumberStyles.Any, _culture);
                        kwhFromNet = kwh;
                        kwhFromEg = 0;

                        var consumption = new Consumption()
                        {
                            Kwh = kwh,
                            KwhFromEg = kwhFromEg,
                            KwhFromNet = kwhFromNet,
                            StartTime = startTime,
                            EndTime = endTime,
                        };
                        _consumptions.Add(consumption);
                    }
                    else if (mode == 2)
                    {
                        startTime = DateTime
                            .Parse(currentRow[0], _culture, DateTimeStyles.AssumeLocal)
                            .ToUniversalTime();
                        endTime = startTime.Add(new TimeSpan(0, 15, 0));
                        kwh = Double.Parse(currentRow[1], NumberStyles.Any, _culture);
                        kwhFromNet = kwh;
                        kwhFromEg = 0;

                        var consumption = new Consumption()
                        {
                            Kwh = kwh,
                            KwhFromEg = kwhFromEg,
                            KwhFromNet = kwhFromNet,
                            StartTime = startTime,
                            EndTime = endTime,
                        };
                        _consumptions.Add(consumption);
                    }
                    else if (mode == 3)
                    {
                        startTime = DateTime
                            .Parse(currentRow[0], _culture, DateTimeStyles.AssumeLocal)
                            .ToUniversalTime();
                        endTime = startTime.Add(new TimeSpan(0, 15, 0));

                        var consumption = _consumptions.FirstOrDefault(c =>
                            c.StartTime == startTime
                        );
                        if (consumption == null)
                        {
                            consumption = new()
                            {
                                Kwh = kwh,
                                KwhFromEg = kwhFromEg,
                                KwhFromNet = kwhFromNet,
                                StartTime = startTime,
                                EndTime = endTime,
                            };
                            _consumptions.Add(consumption);
                        }

                        if (currentRow[6] == "Gesamtverbrauch laut Messung")
                            consumption.Kwh = Double.Parse(
                                currentRow[7],
                                NumberStyles.Any,
                                _culture
                            );
                        else if (currentRow[6] == "Bezug aus der Energiegemeinschaft")
                            consumption.KwhFromEg = Double.Parse(
                                currentRow[7],
                                NumberStyles.Any,
                                _culture
                            );
                        else if (currentRow[6] == "Bezug vom Energielieferanten")
                            consumption.KwhFromNet = Double.Parse(
                                currentRow[7],
                                NumberStyles.Any,
                                _culture
                            );
                    }

                    // Console.WriteLine(
                    //     "C: {3} == {0} / {2}: {1}",
                    //     startTime,
                    //     kwh,
                    //     endTime,
                    //     currentRow[0]
                    // );
                }
            }

            public (double, double) ConsumpationFromList(DateTime chargeStart, DateTime chargeEnd)
            {
                var list = _consumptions.FindAll(c =>
                    (c.StartTime > chargeStart && c.StartTime < chargeEnd)
                    || (c.EndTime > chargeStart && c.EndTime < chargeEnd)
                );
                return (list.Sum(c => c.Kwh), list.Sum(c => c.KwhFromEg));
            }

            public (double, double) ConsumptionFromDb(DateTime chargeStart, DateTime chargeEnd)
            {
                var list = _consumptionStore.FindConsumptions(chargeStart, chargeEnd);

                return (list.Sum(c => c.Kwh), list.Sum(c => c.KwhFromEg));
            }

            public void ReadConsumptionsFromDb()
            {
                _consumptions.AddRange(_consumptionStore.ReadAll());
            }

            public (double, double) ConsumpationWhile(DateTime chargeStart, DateTime chargeEnd)
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
