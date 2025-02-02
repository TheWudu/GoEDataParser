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

        Charging.MongoStore ms = new();
        foreach (Charging.Charge charge in charges)
        {
            if (ms.Find(charge.session_id) is null)
            {
                ms.Insert(charge);
            }
        }
        Charging.Charge c = ms.First();
        c.Print();
    }
}
