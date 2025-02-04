using System.Configuration;

public class GoEDataParser
{
    public static void Main()
    {
        Console.WriteLine("Hello Charger-Data-Parser !");

        Charging.JsonParser parser = new(new HttpClient());
        parser.load();

        // Charging.CsvDownloader downloader = new Charging.CsvDownloader();
        // downloader.run();
        // string filepath = downloader.filepath;

        // string filepath = "/home/martin/github/GoEDataParser/GoEDataParser/tmp.csv";
        // Charging.CsvParser parser = new Charging.CsvParser();
        // parser.Parse(filepath);
        List<Charging.Charge> charges = parser.GetCharges();

        Charging.Evaluator evaluator = new Charging.Evaluator();
        evaluator.run(charges);

        Charging.Store.ChargeStore store = new();
        foreach (Charging.Charge charge in charges)
        {
            if (charge.session_id is not null && store.FindBySessionId(charge.session_id) is null)
            {
                store.Insert(charge);
            }
        }
    }
}
