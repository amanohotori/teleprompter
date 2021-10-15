using static System.Math;
// "using static" はひとつのクラスだけからメソッドをインポートする。名前空間からすべてのクラスをインポートする "using" とは対照的。

namespace TeleprompterConsole
// Program.csのMainやその他メソッドと同じ名前空間
{
    internal class TelePrompterConfig
    {
        private object lockHandle = new object();
        public int DelayInMilliseconds { get; private set; } = 200;

        public void UpdateDelay(int increment) // negative to speed up
        {
            var newDelay = Min(DelayInMilliseconds + increment, 1000);
            newDelay = Max(newDelay, 20);
            lock (lockHandle)
            {
                DelayInMilliseconds = newDelay;
            }
        }

        public bool Done { get; private set; }

        public void SetDone()
        {
            Done = true;
        }
    }
}
