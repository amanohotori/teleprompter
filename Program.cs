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
            RunTeleprompter().Wait();
            /*
            ※詳しい引用 https://docs.microsoft.com/ja-jp/dotnet/csharp/tutorials/console-teleprompter
            "ここで、Main では、コードは同期的に待機します。 可能なときは同期的に待機しないで、await 演算子を使用します。 しかし、コンソール アプリケーションの Main メソッドでは、await 演算子を使うことはできません。 それでは、すべてのタスクが完了する前にアプリケーションが終了することになります。"
            ※追記・だけど C# の現バージョンでは async Main メソッドが使える（Mainメソッドを非同期にできる）らしい。どういう状況で使うのか、いまはよくわからない。
            */
        }
        private static async Task RunTeleprompter()
        // config.cs に従って、ShowTeleprompter メソッドと GetInput メソッドを開始して、最初のタスクが終了する時に終了させる、全体の制御のためのメソッド。
        {
            var config = new TelePrompterConfig();
            // config.cs の TelePrompterConfigを config に代入。
            var displayTask = ShowTeleprompter(config);
            // config を引数に ShowTeleprompter を実行して、戻り値を displayTask に代入。

            var speedTask = GetInput(config);
            // config を引数に GetInput を実行して、戻り値を speetTask に代入。
            await Task.WhenAny(displayTask, speedTask);
            // WhenAny(Task[]) の呼び出し。WhenAnyはリスト内の任意のタスクが完了したらすぐに終了する。
        }

        private static async Task ShowTeleprompter(TelePrompterConfig config)
        {
            var words = ReadFrom("sampleQuotes.txt");
            // ファイル名を引数にReadFromメソッド（Mainメソッドの次に書いてある）の戻り値をwordsに返す。
            foreach (var word in words)
            // wordsからwordを取り出し、
            {
                Console.Write(word);
                // wordをConsoleに出力
                if (!string.IsNullOrWhiteSpace(word))
                // wordがnullかスペースだったら
                {
                    await Task.Delay(config.DelayInMilliseconds);
                    // config.DelayInMillisecondsの値の数ms待ってから続ける

                    /*
                    ※重要なメモ※ "await" 演算子は "return" のようにメソッドの戻り値として Task が呼び出し元に返される。 "Task.Delay(200)" （200ms止まれ） Task が、この "ShowTeleprompter" メソッドの戻り値となる。
                    ※詳しい引用 https://docs.microsoft.com/ja-jp/dotnet/csharp/tutorials/console-teleprompter
                    "await キーワードを使用するためには、async 修飾子をメソッド シグネチャに追加する必要があります。 このメソッドは Task を返します。 Task オブジェクトを返す return ステートメントがないことに注意してください。 かわりに、そのTask オブジェクトは await 演算子を使用した時にコンパイラが生成したコードによって作成されます。 このメソッドは await に達するときに戻ることが想像できます。 Task が返されたということは、作業が完了していないことを表しています。 このメソッドは、待機中のタスクが完了したときに再開します。 それが完了すると Task が返され、タスクが完了したことがわかります。 コード呼び出しでは、その返された Task を監視して、いつ完了したのか判断します。"
                    */
                }
            }
            config.SetDone();
            // foreach が最後まで終わったら Done フラグに true を設定
        }

        static IEnumerable<string> ReadFrom(string file)
        // IEnumerable<string> は指定された型（ここではstring）に対する反復処理をサポートする列挙子
        // 以下に書くReadFromメソッドは、引数にfile名のstringを受け取り、fileの内容を読みながらスペースで単語に区切ったり、他色々処理して返していくメソッド
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
                        /* ※yield参照ドキュメント https://docs.microsoft.com/ja-jp/dotnet/csharp/language-reference/keywords/yield
                        // ※ "yield return <なんらかの値>"というのは「戻り値を返すけど普通のreturnのようにまだそれで処理は終わりじゃなくて、次にもどんどん戻り値が列挙されて戻されていくよ」という戻り値の返し方らしい。 "yield break;" が返されるか、親ループ終端まで列挙は続く。
                        */
                        lineLength += word.Length + 1;
                        // カレントwordのLengthにスペースの1をプラスして
                        if (lineLength > 70)
                        // lineLength（返した文字数）が70以上になったら
                        {
                            yield return Environment.NewLine;
                            // 環境依存でうまくやってくれるEnvironmentメソッドでNewLine（改行）を返して
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
        private static async Task GetInput(TelePrompterConfig config)
        {
            Action work = () =>
            /*
            ※ わかりづらいので詳しくメモする。
            この行は、 delegate 型の 宣言を省略して、"Action" （パラメーターを持たず、値を返さないメソッドをカプセル化しまするメソッド）として、デリゲート変数（関数を入れる変数）の "work" を宣言し、ラムダ式によってそれが引数を持たず "()" 、メソッド "do" の無限繰り返し "while(true)" によって "ReadKey" で入力を待機しっぱなしにするメソッド。
             
             "await Task.Run(work)" によって非同期メソッドとして呼び出され、親メソッドのGetInputの呼び出し先が終了させるまで入力を待ち続け、入力が行われても "while (true)" でwhileの条件式が false になることはないので、何度でも入力の待機に戻る。あるいは "X" or "x" の入力を受け取った場合だけ "break" で実行を終える。

            ※引用 https://docs.microsoft.com/ja-jp/dotnet/csharp/language-reference/operators/lambda-expressions#input-parameters-of-a-lambda-expression
            "ラムダ式の入力パラメーターをかっこで囲みます。 入力パラメーターがないことを指定するには、次のように空のかっこを使用します。"

            ※ラムダ式を使ったActionデリゲート宣言の例
            "Action line = () => Console.WriteLine();"

            ※メモ・かなり特殊な書き方のような気がする。型も宣言もなくいきなりActionデリゲートでworkというデリゲートが宣言されている。チュートリアルで使うもんじゃない気がする。でなきゃ説明してほしい。調べないと全然意味がわからなかった。
            */
            {
                do {
                    var key = Console.ReadKey(true);
                    // コンソールのキー入力を読み取り key に代入。 true オプションは入力をコンソールに表示しない。
                    if (key.KeyChar == '>')
                    {
                        config.UpdateDelay(-10);
                        // 10ms だけ Delay が短くなる。（早くなる）
                    }
                    else if (key.KeyChar == '<')
                    {
                        config.UpdateDelay(10);
                        // 10ms だけ Delay が長くなる。（遅くなる）
                    }
                    else if (key.KeyChar == 'X' || key.KeyChar == 'x')
                    {
                        config.SetDone();
                        // Done フラグを true に
                    }
                } while (!config.Done);
                // Done フラグが false であれば繰り返し。 true であれば抜ける。
            };
            await Task.Run(work);
            // カプセル化されたデリゲートメソッドworkをawait（非同期）で実行
        }
        
    }
}
