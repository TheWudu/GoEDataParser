using GoEDataParser.Models;
using GoEDataParser.Parser;
using GoEDataParser.Parser.Parser;
using GoEDataParser.Repository;
using GoEDataParser.Utils.Utils;

namespace GoEDataParser;

public abstract class ChargeData
{
    private static List<Charge> LoadViaCsv()
    {
        Console.WriteLine("Using CSV downloader and parser");

        CsvDownloader downloader = new CsvDownloader();
        downloader.Run();
        string? filepath = downloader.Filepath;

        CsvParser csvParser = new CsvParser();
        csvParser.Parse(filepath);

        return csvParser.GetCharges();
    }

    private static List<Charge> LoadViaJson()
    {
        Console.WriteLine("Using JSON downloader and parser");

        JsonParser parser = new(new HttpClient());
        parser.Load();

        return parser.GetCharges();
    }

    private static async Task<List<Charge>> ReadCharges(IChargeStore chargeStore)
    {
        return await chargeStore.ReadAll();
    }

    private static async Task StoreCharges(IChargeStore chargeStore, List<Charge> charges)
    {
        int storedCount = 0;
        int updatedCount = 0;

        foreach (Charge charge in charges)
        {
            Charge? storedCharge = await chargeStore.FindBy(c => c.SessionId == charge.SessionId);
            if (storedCharge is null)
            {
                charge.Id ??= Guid.NewGuid().ToString();
                charge.Version = 1;
                await chargeStore.Insert(charge);
                storedCount++;
            }
            else
            {
                charge.Id = storedCharge.Id;
                charge.Version = storedCharge.Version;
                if (!charge.Equals(storedCharge))
                {
                    storedCharge.Kwh = charge.Kwh;
                    storedCharge.MeterStart = charge.MeterStart;
                    storedCharge.MeterEnd = charge.MeterEnd;
                    storedCharge.EndTime = charge.EndTime;
                    storedCharge.SecondsCharged = charge.SecondsCharged;
                    await chargeStore.Update(storedCharge);
                    updatedCount++;
                }
            }
        }

        Console.WriteLine("Stored {0} and updated {1} charges in db", storedCount, updatedCount);
    }

    private static IChargeStore MongoStore()
    {
        var dbHost = Configuration.MongoDbHost();
        var dbName = Configuration.MongoDbName();

        ChargeMongoStore chargeStore = new(dbHost, dbName);

        return chargeStore;
    }

    private static IChargeStore MysqlStore()
    {
        var dbHost = Configuration.MysqlDbHost();
        var dbName = Configuration.MysqlDbName();
        var dbUser = Configuration.MysqlDbUser();
        var dbPassword = Configuration.MysqlDbPassword();

        ChargeMysqlStore chargeStore = new(dbHost, dbName, dbUser, dbPassword);

        return chargeStore;
    }

    private static IChargeStore CosmosStore()
    {
        var dbHost = "https://localhost:8081/";
        var dbName = "goedataparser";

        ChargeCosmosStore chargeStore = new(dbHost, dbName);

        return chargeStore;
    }

    private static Task ListChargesWithConsumptions(List<Charge> charges, ConsumptionParser cp)
    {
        foreach (Charge charge in charges)
        {
            var (consumption, consumptionFromEg) = cp.ConsumpationWhile(
                charge.StartTime,
                charge.StartTime.Add(new TimeSpan(0, 0, (int)charge.SecondsCharged))
            );
            charge.PrintWithConsumption(consumption, consumptionFromEg);
        }
        Console.WriteLine("");
        return Task.CompletedTask;
    }

    private static Task ListCharges(List<Charge> charges)
    {
        foreach (Charge charge in charges)
        {
            charge.Print();
        }
        Console.WriteLine("");

        return Task.CompletedTask;
    }

    // public static void parseDate(string dateString, string text)
    // {
    //     CultureInfo _culture = CultureInfo.CreateSpecificCulture(Configuration.Culture());
    //
    //     Console.WriteLine(
    //         "{0}: string: {1} - AssumeLocal (in UTC): {2} - AssumeUtc (in UTC): {3}",
    //         text,
    //         dateString,
    //         DateTime.Parse(dateString, _culture, DateTimeStyles.AssumeLocal).ToUniversalTime(),
    //         DateTime.Parse(dateString, _culture, DateTimeStyles.AssumeUniversal).ToUniversalTime()
    //     );
    // }

    private static void PrintHelp()
    {
        Console.WriteLine(
            """
            Usage: dotnet run --project GoEDataParser/GoEDataParser.csproj <options>
                    
            Options: 
                -csv    Download charge data as CSV file and parse it
                -json   Download charge data as JSON and parse it
                -mysql                          Use MySQL instead of mongo to store charges
                -no-store                        Do not store charges in db    
                -import-consumption-file <file>  Import consumption file and store in db
                -list                           List charges
                -list-eg                         List charges with consumptions data (slow)
                -read-consumptions              Read consumptions from file instead of db
                -update-consumptions            Update read consumptions from file in db
            """
        );
    }

    public static async Task Main(string[] args)
    {
        Console.WriteLine("Hello Charger-Data-Parser !");

        IChargeStore store;
        List<Charge> charges = new();

        // args = args.Append("-csv").ToArray();
        // args = args.Append("-json").ToArray();
        // args = args.Append("-mysql").ToArray();

        if (args.Contains("-help"))
        {
            PrintHelp();
            return;
        }

        if (args.Contains("-mysql"))
        {
            Console.WriteLine("Use MySQL database");
            store = MysqlStore();
        }
        else if (args.Contains("-cosmos"))
        {
            Console.WriteLine("Use Cosmos database");
            store = CosmosStore();
        }
        else
        {
            Console.WriteLine("Use Mongo database");
            store = MongoStore();
        }

        // Read charges from csv, JSON mysql or mongo
        if (args.Contains("-csv"))
        {
            charges = LoadViaCsv();
        }
        else if (args.Contains("-json"))
        {
            charges = LoadViaJson();
        }

        if (charges.Count != 0)
        {
            if (!args.Contains("-no-store"))
            {
                await Time.MeasureTimeVoid(
                    "Store charges in DB... ",
                    codeBlock: async Task () => await StoreCharges(store, charges)
                );
            }
        }

        charges = await Time.MeasureTime(
            "Read charges from database ...",
            codeBlock: async Task<List<Charge>> () => await ReadCharges(store)
        );

        ConsumptionParser cp = new();

        if (args.Contains("-list"))
        {
            await Time.MeasureTimeVoid(
                "List charges ... ",
                codeBlock: async Task () => await ListCharges(charges)
            );
        }
        if (args.Contains("-list-eg"))
        {
            await Time.MeasureTimeVoid(
                "List charges ... ",
                codeBlock: () => ListChargesWithConsumptions(charges, cp)
            );
        }

        if (args.Contains("-import-consumption-file"))
        {
            var index = args.ToList().IndexOf("-import-consumption-file");
            var filename = args[index + 1];
            // var filename = "/home/martin/Downloads/manager_eg_202509.csv";

            await Time.MeasureTimeVoid(
                "Read consumptions from file",
                codeBlock: () => cp.ReadFile(filename)
            );
            await Time.MeasureTimeVoid(
                "Store consumptions",
                codeBlock: async Task () => await cp.StoreConsumptions()
            );
        }

        if (args.Contains("-read-consumptions"))
        {
            await Time.MeasureTimeVoid(
                "Read consumptions from files",
                codeBlock: () => cp.ParseFiles()
            );

            if (args.Contains("-update-consumptions"))
            {
                await Time.MeasureTimeVoid(
                    "Store consumptions",
                    codeBlock: async Task () => await cp.StoreConsumptions()
                );
            }
        }
        else
        {
            await Time.MeasureTimeVoid(
                "Read consumptions from DB",
                codeBlock: async Task () => await cp.ReadConsumptionsFromDb()
            );
        }

        Evaluator evaluator = new Evaluator(cp);
        var monthly = await Time.MeasureTime(
            "Group monthly ... ",
            codeBlock: async () => await evaluator.GroupMonthly(charges)
        );
        await evaluator.PrintGroup(monthly, "month");

        var yearly = await Time.MeasureTime(
            "Group yearly ... ",
            codeBlock: () => evaluator.GroupYearly(charges)
        );
        await evaluator.PrintGroup(yearly, "year");

        // var docs = Time.MeasureTime("Get stats by DB ... ", codeBlock: store.GroupMonthly);
        // evaluator.PrintGroup(docs, "month");
    }
}
