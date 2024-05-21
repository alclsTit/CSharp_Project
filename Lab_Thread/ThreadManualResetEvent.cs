using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_Thread
{
    public static class ThreadManualResetEvent
    {
        public static readonly ManualResetEventSlim msMRESlim = new ManualResetEventSlim(false);

        public static void TravelThroughGates(string name, int seconds)
        {
            Console.WriteLine($"{name} falls to sleep");
            Thread.Sleep(TimeSpan.FromSeconds(seconds));

            Console.WriteLine($"{name} waits for the gates to open");
            msMRESlim.Wait();
            Console.WriteLine($"{name} enters the gates!!!");
        }
    }
}
