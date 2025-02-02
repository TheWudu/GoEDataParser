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

        Charging.Evaluator evaluator = new Charging.Evaluator();
        evaluator.run(parser.GetCharges());

        Charging.MongoStore ms = new();
        //ms.Insert(parser.GetCharges().First());
        Charging.Charge c = ms.Find();
        c.Print();
    }
}
