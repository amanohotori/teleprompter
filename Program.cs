using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace TeleprompterConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            static IEnumerable<string> ReadFrom(string file)
            {
                string line;
                using (var reader = File.OpenText(file))
                {
                    while ((line = reader.ReadLine()) != null)
                    {
                        var words = line.Split(' ');
                        var lineLength = 0;
                        foreach (var word in words)
                        {
                            yield return word + " ";
                            lineLength += word.Length + 1;
                            if (lineLength > 70)
                            {
                                yield return Environment.NewLine;
                                lineLength = 0;
                            }
                        }
                        yield return Environment.NewLine;
                    }
                }
            }
            
            var lines = ReadFrom("sampleQuotes.txt");
            foreach (var line in lines)
            {
                Console.Write(line);
                if (!string.IsNullOrWhiteSpace(line))
                {
                    var pause = Task.Delay(200);
                    // タスクを同期的に待つことは、アンチパターン（推奨されないこと）です。これは後のステップで修正されます。
                    pause.Wait();
                }
            }
        }
    }
}
