using System.Diagnostics;

namespace GoEDataParser.Utils
{
    namespace Utils
    {
        public delegate Task<T> CodeBlock<T>();
        public delegate Task CodeBlock();

        public class Time
        {
            public static async Task MeasureTimeVoid(string text, CodeBlock codeBlock)
            {
                Stopwatch sw = new();

                sw.Start();
                await codeBlock.Invoke();
                sw.Stop();

                Console.WriteLine("[{1,4} ms] {0}", text, sw.ElapsedMilliseconds);
            }

            public static async Task<T> MeasureTime<T>(string text, CodeBlock<T> codeBlock)
            {
                Stopwatch sw = new();

                sw.Start();
                var result = await codeBlock.Invoke();
                sw.Stop();

                Console.WriteLine("[{1,4} ms] {0}", text, sw.ElapsedMilliseconds);

                return result;
            }
        }
    }
}
