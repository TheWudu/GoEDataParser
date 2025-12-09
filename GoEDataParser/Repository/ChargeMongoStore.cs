using GoEDataParser.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Repository;

namespace GoEDataParser.Repository
{
    public interface IChargeStore : IGenericStore<Charge>
    {
        List<Charge> FindByStartDate(DateTime dateTime);
        Dictionary<string, ChargeInfo> GroupMonthly();
    }

    public class ChargeMongoStore(string dbHost, string dbName)
        : GenericMongoStore<Charge>(dbHost, dbName, "charges"),
            IChargeStore
    {
        public List<Charge> FindByStartDate(DateTime dateTime)
        {
            var start = dateTime.Date;
            var end = start.AddDays(1);
            var filter =
                Builders<Charge>.Filter.Gte(c => c.StartTime, start)
                & Builders<Charge>.Filter.Lt(c => c.StartTime, end);

            var documents = Collection.Find(filter);

            return documents.ToList();
        }

        public Dictionary<string, ChargeInfo> GroupMonthly()
        {
            // var pipeline = new EmptyPipelineDefinition<Charge>()
            //     .Project(x => new
            //     {
            //         Start = x.StartTime.ToString("%Y.%m"),
            //         x.Kwh,
            //         x.SecondsCharged,
            //     })
            //     .Group(
            //         c => c.Start,
            //         g => new ChargeInfo()
            //         {
            //             TimeKey = g.Key,
            //             KwhSum = g.Sum(c => c.Kwh),
            //             TimeSum = g.Sum(c => c.SecondsCharged),
            //             Count = g.Count(),
            //             KwhValues = g.Select(c => c.Kwh).ToList(),
            //         }
            //     )
            //     .Sort(Builders<ChargeInfo>.Sort.Ascending(x => x.TimeKey));

            // Console.WriteLine(pipeline);

            /* The following query fetches and groups the charges per month,
               and also calculates the missing kwh (meterStart - meterEnd per entry)
               
               The c# implementation using BSON documents is pretty huge.
               
               db.charges.aggregate([
                 { "$sort": { StartTime: 1 }},
                 {"$group": {
                   "_id": null,
                   "docs": {
                     "$push": {
                       "StartTime": "$StartTime",
                       "Kwh": "$Kwh",
                       "SecondsCharged": "$SecondsCharged",
                       "MeterEnd": "$MeterEnd",
                       "MeterStart": "$MeterStart"
                     }
                    }}
                 },
                 {
                   "$set": {
                     "docs": {
                       "$map": {
                         "input": { "$range": [0, {"$size": "$docs"}]},
                         "as": "idx",
                         "in": {
                           "$let": {
                             "vars": {
                               "d0": {"$arrayElemAt": ["$docs", {"$max": [0, {"$subtract": ["$$idx", 1]}]}]},
                               "d1": {"$arrayElemAt": ["$docs", "$$idx"]}
                             },
                             "in": {
                               "StartTime":  "$$d1.StartTime",
                               "Kwh":        "$$d1.Kwh",
                               "SecondsCharged": "$$d1.SecondsCharged",
                               "MeterStart": "$$d1.MeterStart",
                               "MeterEnd":   "$$d1.MeterEnd",
                               "MeterDiffToPrev": { "$subtract": ["$$d1.MeterStart", "$$d0.MeterEnd"]},
                             }}
                           }
                        }
                     }
                   }
                 },
                 {"$unwind": "$docs"},
                 {
                   "$project": {
                     _id: -1,
                     Start: { $dateToString: { date: "$docs.StartTime", format: "%Y-%m" } },
                     Kwh: "$docs.Kwh",
                     SecondsCharged: "$docs.SecondsCharged",
                     Missing: "$docs.MeterDiffToPrev"
                   }
                 },
                 {
                   $group: {
                     _id: "$Start",
                     KwhSum: { $sum: "$Kwh" },
                     TimeSum: { $sum: "$SecondsCharged" },
                     Count: { $sum: 1 },
                     Missing: { $sum: "$Missing" }
                   }
                 },
                 {
                   $sort: { "_id": 1 }
                 }
               ])
               
            */

            var pipeline = new[]
            {
                // Sort by StartTime
                new BsonDocument
                {
                    {
                        "$sort",
                        new BsonDocument { { "StartTime", 1 } }
                    },
                },
                // Group documents and push into an array
                new BsonDocument
                {
                    {
                        "$group",
                        new BsonDocument
                        {
                            { "_id", BsonNull.Value },
                            {
                                "docs",
                                new BsonDocument
                                {
                                    {
                                        "$push",
                                        new BsonDocument
                                        {
                                            { "StartTime", "$StartTime" },
                                            { "Kwh", "$Kwh" },
                                            { "SecondsCharged", "$SecondsCharged" },
                                            { "MeterEnd", "$MeterEnd" },
                                            { "MeterStart", "$MeterStart" },
                                        }
                                    },
                                }
                            },
                        }
                    },
                },
                // Set operation to calculate MeterDiffToPrev
                new BsonDocument
                {
                    {
                        "$set",
                        new BsonDocument
                        {
                            {
                                "docs",
                                new BsonDocument
                                {
                                    {
                                        "$map",
                                        new BsonDocument
                                        {
                                            {
                                                "input",
                                                new BsonDocument
                                                {
                                                    {
                                                        "$range",
                                                        new BsonArray
                                                        {
                                                            0,
                                                            new BsonDocument
                                                            {
                                                                { "$size", "$docs" },
                                                            },
                                                        }
                                                    },
                                                }
                                            },
                                            { "as", "idx" },
                                            {
                                                "in",
                                                new BsonDocument
                                                {
                                                    {
                                                        "$let",
                                                        new BsonDocument
                                                        {
                                                            {
                                                                "vars",
                                                                new BsonDocument
                                                                {
                                                                    {
                                                                        "d0",
                                                                        new BsonDocument
                                                                        {
                                                                            {
                                                                                "$arrayElemAt",
                                                                                new BsonArray
                                                                                {
                                                                                    "$docs",
                                                                                    new BsonDocument
                                                                                    {
                                                                                        {
                                                                                            "$max",
                                                                                            new BsonArray
                                                                                            {
                                                                                                0,
                                                                                                new BsonDocument
                                                                                                {
                                                                                                    {
                                                                                                        "$subtract",
                                                                                                        new BsonArray
                                                                                                        {
                                                                                                            "$$idx",
                                                                                                            1,
                                                                                                        }
                                                                                                    },
                                                                                                },
                                                                                            }
                                                                                        },
                                                                                    },
                                                                                }
                                                                            },
                                                                        }
                                                                    },
                                                                    {
                                                                        "d1",
                                                                        new BsonDocument
                                                                        {
                                                                            {
                                                                                "$arrayElemAt",
                                                                                new BsonArray
                                                                                {
                                                                                    "$docs",
                                                                                    "$$idx",
                                                                                }
                                                                            },
                                                                        }
                                                                    },
                                                                }
                                                            },
                                                            {
                                                                "in",
                                                                new BsonDocument
                                                                {
                                                                    {
                                                                        "StartTime",
                                                                        "$$d1.StartTime"
                                                                    },
                                                                    { "Kwh", "$$d1.Kwh" },
                                                                    {
                                                                        "SecondsCharged",
                                                                        "$$d1.SecondsCharged"
                                                                    },
                                                                    {
                                                                        "MeterStart",
                                                                        "$$d1.MeterStart"
                                                                    },
                                                                    { "MeterEnd", "$$d1.MeterEnd" },
                                                                    {
                                                                        "MeterDiffToPrev",
                                                                        new BsonDocument
                                                                        {
                                                                            {
                                                                                "$subtract",
                                                                                new BsonArray
                                                                                {
                                                                                    "$$d1.MeterStart",
                                                                                    "$$d0.MeterEnd",
                                                                                }
                                                                            },
                                                                        }
                                                                    },
                                                                }
                                                            },
                                                        }
                                                    },
                                                }
                                            },
                                        }
                                    },
                                }
                            },
                        }
                    },
                },
                // Unwind the docs array
                new BsonDocument { { "$unwind", "$docs" } },
                // Project the required fields
                new BsonDocument
                {
                    {
                        "$project",
                        new BsonDocument
                        {
                            { "_id", 0 },
                            {
                                "Start",
                                new BsonDocument
                                {
                                    {
                                        "$dateToString",
                                        new BsonDocument
                                        {
                                            { "date", "$docs.StartTime" },
                                            { "format", "%Y.%m" },
                                        }
                                    },
                                }
                            },
                            { "Kwh", "$docs.Kwh" },
                            { "SecondsCharged", "$docs.SecondsCharged" },
                            { "Missing", "$docs.MeterDiffToPrev" },
                        }
                    },
                },
                // Group by the "Start" field and calculate the sums
                new BsonDocument
                {
                    {
                        "$group",
                        new BsonDocument
                        {
                            { "_id", "$Start" },
                            {
                                "KwhSum",
                                new BsonDocument { { "$sum", "$Kwh" } }
                            },
                            {
                                "TimeSum",
                                new BsonDocument { { "$sum", "$SecondsCharged" } }
                            },
                            {
                                "Count",
                                new BsonDocument { { "$sum", 1 } }
                            },
                            {
                                "Missing",
                                new BsonDocument { { "$sum", "$Missing" } }
                            },
                            {
                                "KwhValues",
                                new BsonDocument { { "$addToSet", "$Kwh" } }
                            },
                        }
                    },
                },
                // Sort by the Start field
                new BsonDocument
                {
                    {
                        "$sort",
                        new BsonDocument { { "_id", 1 } }
                    },
                },
                new BsonDocument
                {
                    {
                        "$project",
                        new BsonDocument
                        {
                            { "_id", 0 },
                            { "TimeKey", "$_id" },
                            { "KwhSum", 1 },
                            { "TimeSum", 1 },
                            { "Count", 1 },
                            { "Missing", 1 },
                            { "KwhValues", 1 },
                        }
                    },
                },
            };

            var result = Collection
                .Aggregate<ChargeInfo>(pipeline)
                .ToList()
                .ToDictionary(e => e.TimeKey, e => e);

            return result;
        }
    }
}
