using System.Configuration;

public class GoEDataParser
{
    public static List<Charging.Charge> useCsv()
    {
        Console.WriteLine("Using CSV downloader and parser");
        Charging.CsvDownloader downloader = new Charging.CsvDownloader();
        downloader.run();
        string filepath = downloader.filepath;

        Charging.Parser.CsvParser csvParser = new Charging.Parser.CsvParser();
        csvParser.Parse(filepath);

        return csvParser.GetCharges();
    }

    public static List<Charging.Charge> useJson()
    {
        Console.WriteLine("Using JSON downloader and parser");

        Charging.JsonParser parser = new(new HttpClient());
        parser.load();

        return parser.GetCharges();
    }

    public static int storeCharges(List<Charging.Charge> charges)
    {
        int storedCount = 0;
        int updatedCount = 0;
        string dbHost = Charging.Configuration.MongoDbHost();
        string dbName = Charging.Configuration.MongoDbName();

        Charging.ChargeMongoStore chargeStore = new(dbHost, dbName);
        Repository.GenericStore<Charging.Charge> store = new(chargeStore);
        foreach (Charging.Charge charge in charges)
        {
            if (charge.session_id is null)
            {
                continue;
            }
            Charging.Charge? storedCharge = store.FindBy("session_id", charge.session_id);
            if (storedCharge is null)
            {
                store.Insert(charge);
                storedCount++;
            }
            else
            {
                charge.Id = storedCharge.Id;
                charge.Version = storedCharge.Version;
                if (!charge.Equals(storedCharge))
                {
                    store.Update(storedCharge);
                    updatedCount++;
                }
            }
        }

        Console.WriteLine("Stored {0} and updated {1} charges in db", storedCount, updatedCount);

        return storedCount;
    }

    public static List<Charging.Charge> useDatabase()
    {
        string dbHost = Charging.Configuration.MongoDbHost();
        string dbName = Charging.Configuration.MongoDbName();

        Charging.ChargeMongoStore chargeStore = new(dbHost, dbName);
        Repository.GenericStore<Charging.Charge> store = new(chargeStore);

        return store.ReadAll();
    }

    public static void Main(string[] args)
    {
        Console.WriteLine("Hello Charger-Data-Parser !");

        List<Charging.Charge> charges = [];
        // args = args.Append("-json").ToArray();

        if (args.Contains("-csv"))
        {
            charges = useCsv();
        }
        else if (args.Contains("-json"))
        {
            charges = useJson();
        }
        else
        {
            charges = useDatabase();
        }

        if (!args.Contains("-nostore"))
        {
            storeCharges(charges);
        }

        Console.WriteLine("Evaluate charges...\n");
        Charging.Evaluator evaluator = new Charging.Evaluator();
        evaluator.run(charges);
    }
}
