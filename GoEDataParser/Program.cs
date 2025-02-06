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

        Charging.ChargeMongoStore chargeStore = new();
        Repository.GenericStore<Charging.Charge> store = new(chargeStore);
        foreach (Charging.Charge charge in charges)
        {
            if (
                !(
                    charge.session_id is null
                    || store.FindBy("session_id", charge.session_id) is not null
                )
            )
            {
                store.Insert(charge);
                storedCount++;
            }
        }

        return storedCount;
    }

    public static List<Charging.Charge> useDatabase()
    {
        Charging.ChargeMongoStore chargeStore = new();
        Repository.GenericStore<Charging.Charge> store = new(chargeStore);

        return store.ReadAll();
    }

    public static void Main(string[] args)
    {
        Console.WriteLine("Hello Charger-Data-Parser !");

        List<Charging.Charge> charges = [];

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
            int storedCount = storeCharges(charges);
            Console.WriteLine("Stored {0} charges in db", storedCount);
        }

        Console.WriteLine("Evaluate charges...\n");
        Charging.Evaluator evaluator = new Charging.Evaluator();
        evaluator.run(charges);
    }
}
