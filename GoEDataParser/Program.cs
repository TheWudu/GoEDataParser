using System.Configuration;

public class GoEDataParser
{
    List<Charging.Charge> useCsv()
    {
        Console.WriteLine("Using CSV downloader and parser");
        Charging.CsvDownloader downloader = new Charging.CsvDownloader();
        downloader.run();
        string filepath = downloader.filepath;

        // string filepath = "/home/martin/github/GoEDataParser/GoEDataParser/tmp.csv";
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

        Charging.Store.ChargeStore store = new();
        foreach (Charging.Charge charge in charges)
        {
            if (charge.session_id is not null && store.FindBySessionId(charge.session_id) is null)
            {
                store.Insert(charge);
                storedCount++;
            }
        }

        return storedCount;
    }

    public static void Main()
    {
        Console.WriteLine("Hello Charger-Data-Parser !");

        List<Charging.Charge> charges = useJson();

        Charging.Evaluator evaluator = new Charging.Evaluator();
        evaluator.run(charges);

        int storedCount = storeCharges(charges);
        Console.WriteLine("Stored {0} charges in db", storedCount);
    }
}
