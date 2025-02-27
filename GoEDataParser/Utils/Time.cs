using System.Diagnostics;

namespace Charging
{
    namespace Utils
    {
        public delegate T CodeBlock<T>();
        public delegate void CodeBlock();

        public class Time
        {
            public static void MeasureTimeVoid(string text, CodeBlock codeBlock)
            {
                Stopwatch sw = new();

                sw.Start();
                codeBlock.Invoke();
                sw.Stop();

                Console.WriteLine("[{1,4} ms] {0}", text, sw.ElapsedMilliseconds);
            }

            public static T MeasureTime<T>(string text, CodeBlock<T> codeBlock)
            {
                Stopwatch sw = new();

                sw.Start();
                var result = codeBlock.Invoke();
                sw.Stop();

                Console.WriteLine("[{1,4} ms] {0}", text, sw.ElapsedMilliseconds);

                return result;
            }
        }
    }
}
