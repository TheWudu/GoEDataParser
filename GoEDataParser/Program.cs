using System.Configuration;

public class GoEDataParser
{
    public static List<Charging.Charge> UseCsv()
    {
        Console.WriteLine("Using CSV downloader and parser");

        Charging.CsvDownloader downloader = new Charging.CsvDownloader();
        downloader.Run();
        string filepath = downloader.Filepath;

        Charging.Parser.CsvParser csvParser = new Charging.Parser.CsvParser();
        csvParser.Parse(filepath);

        return csvParser.GetCharges();
    }

    public static List<Charging.Charge> UseJson()
    {
        Console.WriteLine("Using JSON downloader and parser");

        Charging.JsonParser parser = new(new HttpClient());
        parser.Load();

        return parser.GetCharges();
    }

    public static int StoreChargesInMongo(List<Charging.Charge> charges)
    {
        int storedCount = 0;
        int updatedCount = 0;
        string dbHost = Charging.Configuration.MongoDbHost();
        string dbName = Charging.Configuration.MongoDbName();

        Charging.ChargeMongoStore chargeStore = new(dbHost, dbName);
        Repository.GenericStore<Charging.Charge> store = new(chargeStore);
        foreach (Charging.Charge charge in charges)
        {
            if (charge.SessionId is null)
            {
                continue;
            }
            Charging.Charge? storedCharge = store.FindBy("SessionId", charge.SessionId);
            if (storedCharge is null)
            {
                charge.Id ??= Guid.NewGuid().ToString();
                charge.Version = 1;
                store.Insert(charge);
                storedCount++;
            }
            else
            {
                charge.Id = storedCharge.Id;
                charge.Version = storedCharge.Version;
                if (!charge.Equals(storedCharge))
                {
                    // charge.Version += 1;
                    store.Update(charge);
                    updatedCount++;
                }
            }
        }

        Console.WriteLine("Stored {0} and updated {1} charges in db", storedCount, updatedCount);

        return storedCount;
    }

    public static int StoreChargesInMySql(List<Charging.Charge> charges)
    {
        int storedCount = 0;
        int updatedCount = 0;
        string dbHost = Charging.Configuration.MysqlDbHost();
        string dbName = Charging.Configuration.MysqlDbName();
        string dbUser = Charging.Configuration.MysqlDbUser();
        string dbPassword = Charging.Configuration.MysqlDbPassword();

        Charging.ChargeMysqlStore chargeStore = new(dbHost, dbName, dbUser, dbPassword);
        Repository.GenericStore<Charging.Charge> store = new(chargeStore);
        foreach (Charging.Charge charge in charges)
        {
            if (charge.SessionId is null)
            {
                continue;
            }
            Charging.Charge? storedCharge = store.FindBy("SessionId", charge.SessionId);
            if (storedCharge is null)
            {
                charge.Id ??= Guid.NewGuid().ToString();
                charge.Version = 1;
                store.Insert(charge);
                storedCount++;
            }
            else
            {
                charge.Id = storedCharge.Id;
                charge.Version = storedCharge.Version;
                if (!charge.Equals(storedCharge))
                {
                    // storedCharge.Version += 1;
                    storedCharge.Kwh = charge.Kwh;
                    storedCharge.MeterStart = charge.MeterStart;
                    storedCharge.MeterEnd = charge.MeterEnd;
                    storedCharge.EndTime = charge.EndTime;
                    storedCharge.SecondsCharged = charge.SecondsCharged;
                    store.Update(storedCharge);
                    updatedCount++;
                }
            }
        }

        Console.WriteLine("Stored {0} and updated {1} charges in db", storedCount, updatedCount);

        return storedCount;
    }

    public static List<Charging.Charge> UseMongo()
    {
        string dbHost = Charging.Configuration.MongoDbHost();
        string dbName = Charging.Configuration.MongoDbName();

        Charging.ChargeMongoStore chargeStore = new(dbHost, dbName);
        Repository.GenericStore<Charging.Charge> store = new(chargeStore);

        return Charging.Utils.Time.MeasureTime("Read from database ... ", codeBlock: store.ReadAll);
    }

    public static List<Charging.Charge> UseMysql()
    {
        List<Charging.Charge> list = new();

        return list;
    }

    public static Dictionary<string, Charging.ChargeInfo> StatsViaMongo()
    {
        string dbHost = Charging.Configuration.MongoDbHost();
        string dbName = Charging.Configuration.MongoDbName();

        Charging.ChargeMongoStore chargeStore = new(dbHost, dbName);
        Repository.GenericStore<Charging.Charge> store = new(chargeStore);

        return chargeStore.GroupMonthly();
    }

    public static void ListCharges(List<Charging.Charge> charges)
    {
        foreach (Charging.Charge charge in charges)
        {
            charge.Print();
        }
        Console.WriteLine("");
    }

    public static void Main(string[] args)
    {
        Console.WriteLine("Hello Charger-Data-Parser !");

        List<Charging.Charge> charges = [];
        // args = args.Append("-csv").ToArray();
        // args = args.Append("-json").ToArray();
        // args = args.Append("-mysql").ToArray();

        if (args.Contains("-csv"))
        {
            charges = UseCsv();
        }
        else if (args.Contains("-json"))
        {
            charges = UseJson();
        }
        else if (args.Contains("-mysql"))
        {
            charges = UseMysql();
        }
        else
        {
            charges = Charging.Utils.Time.MeasureTime(
                "Read charges from database ...",
                codeBlock: UseMongo
            );
        }

        if (!args.Contains("-nostore"))
        {
            if (args.Contains("-mysql"))
            {
                Charging.Utils.Time.MeasureTimeVoid(
                    "Store charges in MySQL... ",
                    codeBlock: () => StoreChargesInMySql(charges)
                );
            }
            else
            {
                Charging.Utils.Time.MeasureTimeVoid(
                    "Store charges in MongoDB... ",
                    codeBlock: () => StoreChargesInMongo(charges)
                );
            }
        }

        if (args.Contains("-list"))
        {
            Charging.Utils.Time.MeasureTimeVoid(
                "List charges ... ",
                codeBlock: () => ListCharges(charges)
            );
        }

        Charging.Evaluator evaluator = new Charging.Evaluator();
        var monthly = Charging.Utils.Time.MeasureTime(
            "Group monthly ... ",
            codeBlock: () => evaluator.GroupMonthly(charges)
        );
        evaluator.PrintGroup(monthly, "month");

        var yearly = Charging.Utils.Time.MeasureTime(
            "Group yearly ... ",
            codeBlock: () => evaluator.GroupYearly(charges)
        );
        evaluator.PrintGroup(yearly, "year");

        var docs = Charging.Utils.Time.MeasureTime(
            "Get stats by mongo ... ",
            codeBlock: StatsViaMongo
        );
        evaluator.PrintGroup(docs, "month");
    }
}
