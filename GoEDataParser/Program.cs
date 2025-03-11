using GoEDataParser.Models;
using GoEDataParser.Parser;
using GoEDataParser.Parser.Parser;
using GoEDataParser.Repository;
using GoEDataParser.Utils.Utils;

namespace GoEDataParser;

public class ChargeData
{
    public static List<Charge> UseCsv()
    {
        Console.WriteLine("Using CSV downloader and parser");

        CsvDownloader downloader = new CsvDownloader();
        downloader.Run();
        string filepath = downloader.Filepath;

        CsvParser csvParser = new CsvParser();
        csvParser.Parse(filepath);

        return csvParser.GetCharges();
    }

    public static List<Charge> UseJson()
    {
        Console.WriteLine("Using JSON downloader and parser");

        JsonParser parser = new(new HttpClient());
        parser.Load();

        return parser.GetCharges();
    }

    public static int StoreChargesInMongo(List<Charge> charges)
    {
        int storedCount = 0;
        int updatedCount = 0;
        string dbHost = Configuration.MongoDbHost();
        string dbName = Configuration.MongoDbName();

        ChargeMongoStore chargeStore = new(dbHost, dbName);

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
                if (charge.Equals(storedCharge))
                    continue;

                chargeStore.Update(charge);
                updatedCount++;
            }
        }

        Console.WriteLine("Stored {0} and updated {1} charges in db", storedCount, updatedCount);

        return storedCount;
    }

    public static int StoreChargesInMySql(List<Charge> charges)
    {
        int storedCount = 0;
        int updatedCount = 0;
        string dbHost = Configuration.MysqlDbHost();
        string dbName = Configuration.MysqlDbName();
        string dbUser = Configuration.MysqlDbUser();
        string dbPassword = Configuration.MysqlDbPassword();

        ChargeMysqlStore chargeStore = new(dbHost, dbName, dbUser, dbPassword);
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

    public static List<Charge> UseMongo()
    {
        string dbHost = Configuration.MongoDbHost();
        string dbName = Configuration.MongoDbName();

        ChargeMongoStore chargeStore = new(dbHost, dbName);

        return Time.MeasureTime("Read from database ... ", codeBlock: chargeStore.ReadAll);
    }

    public static List<Charge> UseMysql()
    {
        string dbHost = Configuration.MysqlDbHost();
        string dbName = Configuration.MysqlDbName();
        string dbUser = Configuration.MysqlDbUser();
        string dbPassword = Configuration.MysqlDbPassword();

        ChargeMysqlStore chargeStore = new(dbHost, dbName, dbUser, dbPassword);

        return chargeStore.ReadAll();
    }

    public static Dictionary<string, ChargeInfo> StatsViaMongo()
    {
        string dbHost = Configuration.MongoDbHost();
        string dbName = Configuration.MongoDbName();

        ChargeMongoStore chargeStore = new(dbHost, dbName);

        return chargeStore.GroupMonthly();
    }

    public static void ListCharges(List<Charge> charges)
    {
        foreach (Charge charge in charges)
        {
            charge.Print();
        }
        Console.WriteLine("");
    }

    public static void Main(string[] args)
    {
        Console.WriteLine("Hello Charger-Data-Parser !");

        List<Charge> charges = [];
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
            charges = Time.MeasureTime("Read charges from database ...", codeBlock: UseMongo);
        }

        if (!args.Contains("-nostore"))
        {
            if (args.Contains("-mysql"))
            {
                Time.MeasureTimeVoid(
                    "Store charges in MySQL... ",
                    codeBlock: () => StoreChargesInMySql(charges)
                );
            }
            else
            {
                Time.MeasureTimeVoid(
                    "Store charges in MongoDB... ",
                    codeBlock: () => StoreChargesInMongo(charges)
                );
            }
        }

        if (args.Contains("-list"))
        {
            Time.MeasureTimeVoid("List charges ... ", codeBlock: () => ListCharges(charges));
        }

        Evaluator evaluator = new Evaluator();
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

        var docs = Time.MeasureTime("Get stats by mongo ... ", codeBlock: StatsViaMongo);
        evaluator.PrintGroup(docs, "month");
    }
}
