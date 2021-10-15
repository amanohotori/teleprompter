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
            var lines = ReadFrom("sampleQuotes.txt");
            // ファイル名を引数にReadFromメソッド（Mainメソッドの次に書いてある）の戻り値をlinesに返す。
            foreach (var line in lines)
            // linesからlineを取り出し、
            {
                Console.Write(line);
                // lineをConsoleに出力
                if (!string.IsNullOrWhiteSpace(line))
                // lineにnullかスペースだったら
                {
                    var pause = Task.Delay(200);
                    // 引用："タスクを同期的に待つことは、アンチパターンです。これは後のステップで修正します。"
                    pause.Wait();
                    // 200ms待ってから続ける
                }
            }
        }
        static IEnumerable<string> ReadFrom(string file)
        // IEnumerable<string> は指定された型（ここではstring）に対する反復処理をサポートする列挙子
        // 以下に書くReadFromメソッドは、引数にfile名のstringを受け取り、fileの内容を読みながらスペースで単語に区切ったり、他色々処理して返していくメソッドです。
        {
            string line;
            using (var reader = File.OpenText(file))
            // File.OpenText(ファイル名)を使って、ファイルの内容全部をreaderオブジェクトにする。
            {
                while ((line = reader.ReadLine()) != null)
                // readerオブジェクトから順にlineをとりだしていく。
                // 行がnull（ファイルのおしまい）じゃなかったら
                {
                    var words = line.Split(' ');
                    // lineをスペースで区切ってwordsリストにして
                    var lineLength = 0;
                    // lineLength 変数を宣言、0で初期化して
                    foreach (var word in words)
                    // wordsリストからwordを取り出していき
                    {
                        yield return word + " ";
                        // wordとスペースを返す
                        // ※ "yield return"というのは「戻り値を返すけど普通のreturnのようにまだそれで処理は終わりじゃなくて、次にもどんどん戻り値が列挙されて戻されていくよ」という戻り値の返し方らしい。
                        lineLength += word.Length + 1;
                        // カレントwordのLengthにスペースの1をプラスして
                        if (lineLength > 70)
                        // 返した文字数が70以上になったら
                        {
                            yield return Environment.NewLine;
                            // Environment.NewLine（環境依存の改行）を返して
                            lineLength = 0;
                            // lineLengthを0に初期化してまた数え直し
                        }
                    }
                    yield return Environment.NewLine;
                    // wordsをforeachし終わったら改行を返す。
                    // whileに戻って次のlineの処理へ（lineがnullになるまで）
                }
            }
        }
    }
}
