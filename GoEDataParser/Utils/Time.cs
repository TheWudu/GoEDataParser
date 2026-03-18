using System.Diagnostics;

namespace GoEDataParser.Utils
{
    namespace Utils
    {
        public delegate Task<T> CodeBlock<T>();
        public delegate Task CodeBlock();

        public delegate T CodeBlockT<T>();
        public delegate void CodeBlockVoid();

        public class Time
        {
            public static void MeasureTimeVoid(string text, CodeBlockVoid codeBlock)
            {
                Stopwatch sw = new();

                sw.Start();
                codeBlock.Invoke();
                sw.Stop();

                Console.WriteLine("[{1,4} ms] {0}", text, sw.ElapsedMilliseconds);
            }

            public static async Task MeasureTimeVoidAsync(string text, CodeBlock codeBlock)
            {
                Stopwatch sw = new();

                sw.Start();
                await codeBlock.Invoke();
                sw.Stop();

                Console.WriteLine("[{1,4} ms] {0}", text, sw.ElapsedMilliseconds);
            }

            public static T MeasureTime<T>(string text, CodeBlockT<T> codeBlock)
            {
                Stopwatch sw = new();

                sw.Start();
                var result = codeBlock.Invoke();
                sw.Stop();

                Console.WriteLine("[{1,4} ms] {0}", text, sw.ElapsedMilliseconds);

                return result;
            }

            public static async Task<T> MeasureTimeAsync<T>(string text, CodeBlock<T> codeBlock)
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
