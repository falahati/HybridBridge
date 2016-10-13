using System;
using System.Timers;

namespace HybridBridge.Android.Samples.Controllers
{
    internal class EventSamplesController
    {
        public static void StartTimer(int countTo)
        {
            var timer = new Timer(1000);
            var times = 0;
            timer.Elapsed += (sender, args) =>
            {
                times++;
                if (times > countTo)
                {
                    timer.Stop();
                    timer.Dispose();
                }
                else
                {
                    IntEvent?.Invoke(times);
                }
            };
            timer.Start();
        }

        public static event Action<int> IntEvent;
    }
}