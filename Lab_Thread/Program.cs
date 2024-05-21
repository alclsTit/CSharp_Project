using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Lab_Thread
{
    public static class GlobalThread
    {
        public static void PrintNumberWithDelay()
        {
            Console.WriteLine("PrintNumberWithDelay Starting...");
            try
            {
                for (int i = 1; i < 10; i++)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(500));
                    Console.WriteLine(i);
                }
            }
            catch (ThreadInterruptedException threadEx)
            {
                Console.WriteLine($"Exception in GlobalThread.PrintNumberWithDelay - {threadEx.Message} - {threadEx.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GlobalThread.PrintNumberWithDelay - {ex.Message} - {ex.StackTrace}");
            }
        }

        public static async Task PrintNumberWithJobThread()
        {
            Console.WriteLine("PrintNumberWithJobThread Starting...");
            int sum = 0;
            await Task.Run(() =>
            {
                Thread.Sleep(2000);
                for (int i = 1; i < 100000; ++i)
                {
                    sum += 1;
                }
            });
            Console.WriteLine($"PrintNumberWithJobThread job completed!!! --- Sum = {sum}");
        }

        public static void DoNothing()
        {
            Thread.Sleep(TimeSpan.FromSeconds(2));
        }

        public static void PrintNumberWithStatus()
        {
            Console.WriteLine("PrintNumberWithStatus Starting...");
            Console.WriteLine($"PrintNumberWithStatus => [{Thread.CurrentThread.ManagedThreadId.ToString()}] - {Thread.CurrentThread.ThreadState.ToString()}");
            for (int i = 1; i < 10; ++i)
            {
                Thread.Sleep(TimeSpan.FromSeconds(2));
                Console.WriteLine(i);
            }
        }
    }


    class ThreadSample
    {
        private readonly int _iterations;

        public ThreadSample(int iterations)
        {
            _iterations = iterations;
        }

        public void CountNumbers()
        {
            for (int i = 0; i < _iterations; ++i)
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
                Console.WriteLine($"{Thread.CurrentThread.Name} - {i}");
            }
        }

        public void CastingOtherBasicType(int data)
        {
            Console.WriteLine($"{nameof(data)} - {data} is casted => [Type = {data.GetType().Name}]");
        }

        public void CastingOtherType(short data)
        {

        }
    }

    class MyFakeParent
    {
        public MyFakeParent()
        {
            Console.WriteLine($"This is myFakeParent");
        }
    }

    class MyChild
    {
        public MyChild()
        {
            Console.WriteLine($"This is myChild");
        }

        public static explicit operator MyFakeParent(MyChild obj)
        {
            return new MyFakeParent();
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            ThreadSample ts = new ThreadSample(0);

            string ch = Convert.ToString(null);

            MyChild c = new MyChild();
            MyFakeParent fp = (MyFakeParent)c;

            string tmpStr = "apple";
            if (Int32.TryParse(tmpStr, out var convInt))
            {
                Console.WriteLine($"String to Int => {convInt}");
            }
            else
            {
                var result = Int32.Parse(tmpStr);
                Console.WriteLine("Fail to convert String to Int ");
            }


            Console.WriteLine($"[Main Thread ID] => {Thread.CurrentThread.ManagedThreadId}");
            ContentsMission tmpMission = new ContentsMission();
            ContentsReward tmpReward = new ContentsReward();

            var missionTask = ContentsManager.SetMissionAsync(tmpMission, "MissionMessage");
            var rewardTask = ContentsManager.SetRewardAsync(tmpReward, 10000, 10);

            Task.WaitAll(missionTask, rewardTask);

            //... 이후 미션 및 보상 데이터를 이용한 작업 진행
            Console.WriteLine($"[Main Thread ID] => {Thread.CurrentThread.ManagedThreadId}");

            ContentsManager.SetMissionAndReward(tmpMission, tmpReward);

            return;

            #region "Thread TEST 1"
            /*
            Console.WriteLine("Main Starting...");
            Thread t = new Thread(new ThreadStart(GlobalThread.PrintNumberWithDelay));
            var taskResult = GlobalThread.PrintNumberWithJobThread();

            t.Start();

            taskResult.Wait();
            t.Interrupt();

            t.Join();
            Console.WriteLine("Thread job completed!!!");
            */
            #endregion

            #region "Thread TEST 2"
            /*
            Console.WriteLine("Starting Program...");
            Thread t = new Thread(GlobalThread.PrintNumberWithStatus);
            Thread t2 = new Thread(GlobalThread.DoNothing);
            t.Name = "t";
            t2.Name = "t2";

            Console.WriteLine($"[{t.ManagedThreadId.ToString()}] - {t.ThreadState.ToString()} -------- OperatingThread[{Thread.CurrentThread.ManagedThreadId.ToString()}]");

            t2.Start();
            t.Start();

            for(int i = 1; i < 30; ++i)
            {
                Console.WriteLine($"[{i}, {t.ManagedThreadId.ToString()}]{t.ThreadState.ToString()} -------- OperatingThread[{Thread.CurrentThread.ManagedThreadId.ToString()}]");
            }

            Thread.Sleep(TimeSpan.FromSeconds(6));
            t.Interrupt();

            Console.WriteLine("A thread has been interrupted");
            Console.WriteLine(t.ThreadState.ToString());
            Console.WriteLine(t2.ThreadState.ToString());
            */
            #endregion

            #region "Thread TEST 3"
            /*
            var sampleForeground = new ThreadSample(10);
            var sampleBackground = new ThreadSample(20);

            var threadOne = new Thread(sampleForeground.CountNumbers);
            threadOne.Name = "ForegroundThread";
            threadOne.IsBackground = true;

            var threadTwo = new Thread(sampleBackground.CountNumbers);
            threadTwo.Name = "BackgroundThread";
            threadTwo.IsBackground = true;

            threadOne.Start();
            threadTwo.Start();

            threadOne.Join();
            */
            #endregion

            #region "AutoResetEvent"
            var t = new Thread(() => ThreadAutoResetEvent.Process(10));
            t.Start();

            Console.WriteLine("Waiting for another thread to complete work");

            // AutoResetEvent가 차단기가 내려간상태에서 작업 시작. WaitOne 함수 호출 시 이후의 작업에 대해서 스레드가 접근하지 못하고 대기
            // ThreadAutoResetEvent.Process에서 10초가 지나고 msWorkerEvent.Set을 호출하여 차단기가 올라갈 때까지 대기
            ThreadAutoResetEvent.msWorkerEvent.WaitOne();

            // 10초 이후 ThreadAutoResetEvent.Process의 작업[1]이 완료된 이후 해당 코드 실행
            Console.WriteLine("First operation is completed!!!");
            Console.WriteLine("Performing an operation on a main thread");

            Thread.Sleep(TimeSpan.FromSeconds(5));
            
            // 15초 경과이후 msMainEvent.Set으로 인해 차단기가 올라갔고 Process 메서드 작업[3] 이후의 작업 진행 
            ThreadAutoResetEvent.msMainEvent.Set();

            Console.WriteLine("Now running the second operation on a second thread");

            ThreadAutoResetEvent.msMainEvent.WaitOne();
            Console.WriteLine("Second operation is completed!!!");
            #endregion

            #region ManunalResetEventSlim"
            /*
            var t1_name = "Thread 1";
            var t2_name = "Thread 2";
            var t3_name = "Thread 3";

            var t1 = new Thread(() => ThreadManualResetEvent.TravelThroughGates(t1_name, 5));
            var t2 = new Thread(() => ThreadManualResetEvent.TravelThroughGates(t2_name, 6));
            var t3 = new Thread(() => ThreadManualResetEvent.TravelThroughGates(t3_name, 12));
            t1.Name = t1_name;
            t2.Name = t2_name;
            t3.Name = t3_name;

            t1.Start();
            t2.Start();
            t3.Start();

            Thread.Sleep(TimeSpan.FromSeconds(6)); // (6초 후)
            Console.WriteLine("The gates are now open!!!");

            // 이 부분에서 차단기를 올리면 대기하고 있던 모든 스레드가 Wait이후 코드를 실행
            // 5, 6초 sleep 상태에 있던 thread1,2가 접근
            ThreadManualResetEvent.msMRESlim.Set();

            Thread.Sleep(TimeSpan.FromSeconds(2)); // (8초 후)

            // Thread3이 sleep 상태에 있고 현재 임계영역 접근에 대한 차단기가 내려간 상태
            ThreadManualResetEvent.msMRESlim.Reset();

            Console.WriteLine("The gates have been closed!!!");

            Thread.Sleep(TimeSpan.FromSeconds(10)); // (18초 후)

            Console.WriteLine("The gates are now open for the second time!!!");

            // 18초가 지난 뒤 Thread3가 Reset에 의해서 wait 이후의 코드를 진행하고 있지 못함.
            // 여기서 set을 통해서 차단기를 올려 thread3의 wait 이후 코드를 진행
            ThreadManualResetEvent.msMRESlim.Set();

            Thread.Sleep(TimeSpan.FromSeconds(2)); // (20초 후)

            Console.WriteLine("The gates have been closed!!!");

            ThreadManualResetEvent.msMRESlim.Reset();
            */
            #endregion
        }
    }
}
