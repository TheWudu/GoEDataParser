namespace GoEDataParserTest
{
    public class Base
    {
        public static string AppDirectory(
            [System.Runtime.CompilerServices.CallerFilePath] string fileName = ""
        )
        {
            string? directory = Path.GetDirectoryName(fileName);
            if (directory is null)
            {
                directory = AppDomain.CurrentDomain.BaseDirectory;
            }
            return directory;
        }
    }
}
