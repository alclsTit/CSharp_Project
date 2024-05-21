using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Net;   
using Lab_SimpleServer.Network;

namespace Lab_SimpleServer
{
    public static class GSocketStateMask
    {
        public const int EMPTY_MASK = 0x00000000;
        public const int OLD_MASK = 0x7FFF0000;
        public const int NEW_MASK = 0x0000FFFF;
    }

    public static class GSocketState
    {
        public const int Empty = 0;

        public const int Initialized = 1;   //0001

        public const int InClosing = 48;     //0110

        public const int Connected = 2;     //0010

        public const int Disconnected = 3;  //0011

        public const int Sending = 4;       //0100

        public const int Receiving = 5;     //0101

    }

    internal class Program
    {
        /*
        public static int myState = GSocketState.Initialized;

        public static bool ChageStateNormal(int state)
        {
            var oldNewState = myState;
            var oldState = (oldNewState & GSocketStateMask.NEW_MASK) << 16;
            
            Console.WriteLine($"{oldState.ToString("X8")}");

            var newState = state & GSocketStateMask.NEW_MASK;

            Console.WriteLine($"[new_state] = {newState.ToString("X8")}");

            var result = oldState | newState;

            Console.WriteLine($"[new_state] = {result.ToString("X8")}");

            if (Interlocked.CompareExchange(ref myState, result, oldNewState) == oldNewState)
            {
                Console.WriteLine($"[Result] = {result.ToString("X8")}(16진수), {Convert.ToInt32(result.ToString("X8"), 16)}");
                return true;
            }

            return false;   
        }


        public static object gLockObj = new object();
        public static bool ChangeState(int state)
        {
           
            //lock(gLockObj)
            //{
            //    Console.WriteLine($"{Thread.CurrentThread.Name} - {state} ongoing...");
            //}
            
            Console.WriteLine($"{Thread.CurrentThread.Name} - {state} ongoing...");

            var oldNewState = myState;
            var oldState = (oldNewState & GSocketStateMask.NEW_MASK) << 16;

            var result = oldState | (state & GSocketStateMask.NEW_MASK);
            if (oldNewState == Interlocked.CompareExchange(ref myState, result, oldNewState))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool CheckMyState(int state)
        {
            return (myState & GSocketStateMask.NEW_MASK) == state;
        }
        */

        static void Main(string[] args)
        {
            #region "Task Test"
            /*
            var Task1State = GSocketState.Sending;
            var result = Task.Run(() =>
            {
                Thread.CurrentThread.Name = "[Thread_1]";
                var doCount = 5;
                while(doCount > 0)
                {
                    ChangeState(Task1State);
                    Thread.Sleep(1000);
                    Console.WriteLine($"[{Thread.CurrentThread.Name} done - [{myState}]");
                }
            });

            var Task2State = GSocketState.Receiving;
            var result2 = Task.Run(() =>
            {
                Thread.CurrentThread.Name = "[Thread_1]";
                var doCount = 5;
                while (doCount > 0) 
                {
                    ChangeState(Task2State);
                    Thread.Sleep(500);
                    Console.WriteLine($"[{Thread.CurrentThread.Name} done - [{myState}]");
                }
            });
            */
            #endregion

            #region "StateCompare"
            /*
            int maxLoopCount = 100;
            bool finishFlag = false;
            bool finishFlag2 = false;

            ChangeState(GSocketState.Receiving);
            var isSame = CheckMyState(GSocketState.Receiving);
            Console.WriteLine($"Is same? [true/false] = {isSame}");

            var Task1State = GSocketState.Sending;
            Thread t1 = new Thread(() =>
            {
                Thread.CurrentThread.Name = "[Thread_1]";
                var doCount = maxLoopCount;
                while (doCount > 0)
                {
                    var result = ChangeState(Task1State);
                    doCount--;
                    Console.WriteLine($"[{Thread.CurrentThread.Name} done({result}) - [{myState.ToString("X8")}]");
                }

                finishFlag = true;
            });


            var Task2State = GSocketState.Receiving;
            Thread t2 = new Thread(() =>
            {
                Thread.CurrentThread.Name = "[Thread_2]";
                var doCount = maxLoopCount;
                while (doCount > 0)
                {
                    var result = ChangeState(Task2State);
                    Task2State = GSocketState.InClosing;
                    doCount--;
                    Console.WriteLine($"[{Thread.CurrentThread.Name} done({result}) - [{myState.ToString("X8")}]");
                }

                finishFlag2 = true;
            });

            t1.Start();
            t2.Start();

            var timer = new Stopwatch();
            timer.Start();
            while (true) 
            { 
                if (finishFlag && finishFlag2)
                {
                    timer.Stop();
                    Console.WriteLine($"Result state = {myState.ToString("X8")}...[{timer.Elapsed.TotalSeconds}] seconds");
                    break;
                }
            }
            */
            #endregion

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("192.168.0.12"), 8800);
            List<Listener> Listeners = new List<Listener>(3);
            List<Thread> ModuleThreads = new List<Thread>(3);

            for(int i = 0; i < 1; ++i)
            {
                Listener listener = new Listener();
                Listeners.Add(listener);
            }

            foreach(var listener in Listeners)
            {
                Thread thread = new Thread(() => { listener.Start(endPoint, 4096, 4096, 100, 1000); });
                ModuleThreads.Add(thread);
            }

            foreach (var module in ModuleThreads)
                module.Start();

            while(true)
            {
                Thread.Sleep(1000);
            }
        }
    }
}
