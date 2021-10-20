using static System.Math;
// "using static" はひとつのクラスだけからメソッドをインポートする。名前空間からすべてのクラスをインポートする "using" とは対照的。

namespace TeleprompterConsole
// Program.csのMainやその他メソッドと同じ名前空間
{
    internal class TelePrompterConfig
    // TelePrompterの実行を制御するオブジェクトクラス
    {
        private object lockHandle = new object();
        // lockステートメントに渡す新しいオブジェクト、lockHandleを宣言
        public int DelayInMilliseconds { get; private set; } = 200;
        // まず get,set について。これはget/setアクセサーというC#に特有の概念で、このプロパティに外部からアクセスする場合は、 "get { return hoge; }"によって プロパティ hoge を取得するか、 "set { hoge = value }" によってプロパティ hoge の値を設定することしかできない。ここでの get/set はオブジェクト hoge のメンバ変数ではなく、アクセサーという概念らしい。プロパティは、そのクラス（実装側）からはメソッドのように扱える。利用する外部のクラスからは、メンバー変数のように見える。 get アクセサーには、プロパティを読み取るためのメソッド、 set アクセサーにはプロパティを値を設定するためのメソッドが書かれる。
        // ※引用："get アクセサーのコード ブロックはプロパティが読み取られる時に実行され、set アクセサーのコード ブロックはプロパティに新しい値が割り当てられるときに実行されます。"

        // その上で、この大括弧に = で値を指定する書き方は、 C#6 からの新機能でできるうようになった自動プロパティに初期値を設定する書き方であるう。ここではプロパティ DelayMillisoconds を宣言し、自動実装プロパティ（ロジックの不要なget,setの簡易な記述のしかた）で、 "hoge" の値を宣言する。
        // 結局、ここで省略して書かれていることは、 DelayMillisoconds が get と private アクセス識別子を持つ set アクセサー（結局この場合はもう、外部から set にはアクセスできないので、プロパティは読み取り専用で書き換えられない）を備え、 set アクセサーは private のアクセス識別子を持ち、このプロパティの値を "set { DelayMillisoconds = 200; } "で初期化するということである。

        public void UpdateDelay(int increment)
        // Delayの更新に伴い、その最小と最大の限度を返す。
        {
            var newDelay = Min(DelayInMilliseconds + increment, 1000);
            // Min(a, b) は、ふたつの数a,bのうち、小さい方を返す。ここでは1000より大きな数は返さなくしてる。
            newDelay = Max(newDelay, 20);
            // Max(a, b) は、ふたつの数a,bのうち、大きい方を返す。ここでは20より小さな数は返さなくしてる。
            lock (lockHandle)
            // このステートメントの間 lockHadle オブジェクトが存在する、このインスタンスをロックする。他のスレッドが書き換えをできないようにした状態で DelayInMilliseconds を更新するため。
            {
                DelayInMilliseconds = newDelay;
                // DelayInMilliseconds を newDelay で更新。
            }
        }

        public bool Done { get; private set; }
        // ファイルが読み込み終わったことを示す真偽フラグ Done

        public void SetDone()
        // Done に true を設定して読み込み終わりとするメソッド
        {
            Done = true;
        }
    }
}
