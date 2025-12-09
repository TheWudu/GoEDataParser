using GoEDataParser.Models;
using GoEDataParser.Parser;
using GoEDataParser.Parser.Parser;
using GoEDataParser.Repository;
using GoEDataParser.Utils.Utils;

namespace GoEDataParser;

public class ChargeData
{
    public static List<Charge> LoadViaCsv()
    {
        Console.WriteLine("Using CSV downloader and parser");

        CsvDownloader downloader = new CsvDownloader();
        downloader.Run();
        string? filepath = downloader.Filepath;

        CsvParser csvParser = new CsvParser();
        csvParser.Parse(filepath);

        return csvParser.GetCharges();
    }

    public static List<Charge> LoadViaJson()
    {
        Console.WriteLine("Using JSON downloader and parser");

        JsonParser parser = new(new HttpClient());
        parser.Load();

        return parser.GetCharges();
    }

    public static int StoreCharges(IChargeStore chargeStore, List<Charge> charges)
    {
        int storedCount = 0;
        int updatedCount = 0;

        foreach (Charge charge in charges)
        {
            Charge? storedCharge = chargeStore.FindBy("SessionId", charge.SessionId);
            if (storedCharge is null)
            {
                charge.Id ??= Guid.NewGuid().ToString();
                charge.Version = 1;
                chargeStore.Insert(charge);
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
                    chargeStore.Update(storedCharge);
                    updatedCount++;
                }
            }
        }

        Console.WriteLine("Stored {0} and updated {1} charges in db", storedCount, updatedCount);

        return storedCount;
    }

    public static IChargeStore MongoStore()
    {
        string dbHost = Configuration.MongoDbHost();
        string dbName = Configuration.MongoDbName();

        ChargeMongoStore chargeStore = new(dbHost, dbName);

        return chargeStore;
    }

    public static IChargeStore MysqlStore()
    {
        string dbHost = Configuration.MysqlDbHost();
        string dbName = Configuration.MysqlDbName();
        string dbUser = Configuration.MysqlDbUser();
        string dbPassword = Configuration.MysqlDbPassword();

        ChargeMysqlStore chargeStore = new(dbHost, dbName, dbUser, dbPassword);

        return chargeStore;
    }

    public static void ListChargesWithConsumptions(List<Charge> charges, ConsumptionParser cp)
    {
        foreach (Charge charge in charges)
        {
            (var consumption, var consumptionFromEg) = cp.ConsumpationWhile(
                charge.StartTime,
                charge.StartTime.Add(new TimeSpan(0, 0, (int)charge.SecondsCharged))
            );
            charge.PrintWithConsumption(consumption, consumptionFromEg);
        }
        Console.WriteLine("");
    }

    public static void ListCharges(List<Charge> charges)
    {
        foreach (Charge charge in charges)
        {
            charge.Print();
        }
        Console.WriteLine("");
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
                -nostore                        Do not store charges in db    
                -import-consumptionfile <file>  Import consumption file and store in db
                -list                           List charges
                -listeg                         List charges with consumptions data (slow)
                -read-consumptions              Read consumptions from file instead of db
                -update-consumptions            Update read consumptions from file in db
            """
        );
    }

    public static void Main(string[] args)
    {
        Console.WriteLine("Hello Charger-Data-Parser !");

        IChargeStore store = null;
        List<Charge> charges;

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
        else
        {
            Console.WriteLine("Use Mongo database");
            store = MongoStore();
        }

        // Read charges from csv, json mysql or mongo
        if (args.Contains("-csv"))
        {
            charges = LoadViaCsv();
        }
        else if (args.Contains("-json"))
        {
            charges = LoadViaJson();
        }
        else
        {
            charges = Time.MeasureTime("Read charges from database ...", codeBlock: store.ReadAll);
        }

        if (!args.Contains("-nostore"))
        {
            Time.MeasureTimeVoid(
                "Store charges in DB... ",
                codeBlock: () => StoreCharges(store, charges)
            );
        }

        ConsumptionParser cp = new();

        if (args.Contains("-list"))
        {
            Time.MeasureTimeVoid("List charges ... ", codeBlock: () => ListCharges(charges));
        }
        if (args.Contains("-listeg"))
        {
            Time.MeasureTimeVoid(
                "List charges ... ",
                codeBlock: () => ListChargesWithConsumptions(charges, cp)
            );
        }

        if (args.Contains("-import-consumptionfile"))
        {
            var index = args.ToList().IndexOf("-import-consumptionfile");
            var filename = args[index + 1];
            // var filename = "/home/martin/Downloads/manager_eg_202509.csv";

            Time.MeasureTimeVoid(
                "Read consumptions from file",
                codeBlock: () => cp.ReadFile(filename)
            );
            Time.MeasureTimeVoid("Store consumptions", codeBlock: () => cp.StoreConsumptions());
        }

        if (args.Contains("-read-consumptions"))
        {
            Time.MeasureTimeVoid("Read consumptions from files", codeBlock: () => cp.ParseFiles());

            if (args.Contains("-update-consumptions"))
            {
                Time.MeasureTimeVoid("Store consumptions", codeBlock: () => cp.StoreConsumptions());
            }
        }
        else
        {
            Time.MeasureTimeVoid(
                "Read consumptions from DB",
                codeBlock: () => cp.ReadConsumptionsFromDb()
            );
        }

        Evaluator evaluator = new Evaluator(cp);
        var monthly = Time.MeasureTime(
            "Group monthly ... ",
            codeBlock: () => evaluator.GroupMonthly(charges)
        );
        evaluator.PrintGroup(monthly, "month");

        var yearly = Time.MeasureTime(
            "Group yearly ... ",
            codeBlock: () => evaluator.GroupYearly(charges)
        );
        evaluator.PrintGroup(yearly, "year");

        var docs = Time.MeasureTime("Get stats by DB ... ", codeBlock: store.GroupMonthly);
        evaluator.PrintGroup(docs, "month");
    }
}
