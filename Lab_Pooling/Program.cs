using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

using Lab_Pooling.ObjectPooling;

namespace Lab_Pooling
{

    public class CPerformanceChecker
    {
        private Stopwatch m_stopwatch = new Stopwatch();
        public delegate void WorkForTimeCheck<T>(T element);

        public CPerformanceChecker()
        {
            m_stopwatch.Start();
        }

        public void Start()
        {
            m_stopwatch.Start();
        }

        // 시간 경과 측정을 중지
        public void Stop()
        {
            if (m_stopwatch.IsRunning)
                m_stopwatch.Stop();
        }

        // 시간 경과값을 0으로 초기화 하고 다음 경과 시간측정을 시작 
        public void Restart()
        {
            m_stopwatch.Restart();
        }

        public void Reset()
        {
            if (m_stopwatch.IsRunning)
                m_stopwatch.Reset();
        }

        // 경과 시간 측정 
        public double TimeElaspedSeconds()
        {
            this.Stop();
            return m_stopwatch.Elapsed.TotalSeconds;
        }

        // 특정 메서드(void 반환타입 메서드)의 경과 시간체크
        public double GetWorkOperationTimeDiffSeconds<T>(WorkForTimeCheck<T> work, T element, int workcount) 
        {
            while(workcount > 0)
            {
                work.Invoke(element);
                
                Console.WriteLine($"{workcount} Task done!!!");
                workcount--;
            }
            
            return this.TimeElaspedSeconds();
        }
    }


    public class Program
    {
        // ThreadPool의 스레드는 background 스레드 
        public static void DoSomething(object state)
        {
            Console.WriteLine($"{state} --- Thread[{Thread.CurrentThread.ManagedThreadId.ToString()}] is operated...");
        }

        static void Main(string[] args)
        {
            var loopCount = 1000000;
            var pfCheckCount = 10;
            var startTime = DateTime.Now;
            double pfCheckResultTime = 0.0;

            CPerformanceChecker pfCheck = new CPerformanceChecker();

            // 1. Object Pool (queue / lock)
            CObjectPool<CHollowObject> myObjectPool = new CObjectPool<CHollowObject>(loopCount, () => { return new CHollowObject(); });  
            pfCheck.Restart();
            pfCheckResultTime = pfCheck.GetWorkOperationTimeDiffSeconds<int>((objCount) =>
            {
                while (objCount > 0)
                {
                    var target = myObjectPool.Pop();
                    myObjectPool.Push(target);
                    --objCount;
                }
            }, loopCount, pfCheckCount);
            Console.WriteLine($"1.[ObjectPool] - {loopCount} logicloop // {pfCheckCount} operated // {pfCheckResultTime} seconds");


            // 2. Object Pool (concurrentqueue / lock)
            CObjectPoolConcurrent<CHollowObject> myObjectPoolConcurrent = new CObjectPoolConcurrent<CHollowObject>(loopCount, () => { return new CHollowObject(); });
            pfCheck.Restart();
            pfCheckResultTime = pfCheck.GetWorkOperationTimeDiffSeconds<int>((objCount) =>
            {
                while (objCount > 0)
                {
                    var target = myObjectPoolConcurrent.Pop();
                    myObjectPoolConcurrent.Push(target);
                    --objCount;
                }
            }, loopCount, pfCheckCount);
            Console.WriteLine($"2.[Concurrent ObjectPool] - {loopCount} logicloop // {pfCheckCount} operated // {pfCheckResultTime} seconds");

            // 3. Just use normal allocator, deallocator
            Queue<CHollowObject> localQ = new Queue<CHollowObject>(loopCount);
            pfCheck.Restart();
            pfCheckResultTime = pfCheck.GetWorkOperationTimeDiffSeconds<int>((objCount) =>
            {
                for (int i = 0; i < objCount; ++i)
                    localQ.Enqueue(new CHollowObject());

                for (int i = 0; i < objCount; ++i)
                    localQ.Dequeue();
            }, loopCount, pfCheckCount);
            Console.WriteLine($"3.[Normal Allocator]  - {loopCount} logicloop // {pfCheckCount} operated // {pfCheckResultTime} seconds");
       }
    }
}

#region "테스트코드"
/*CObjectPool<CHollowObject> myObjectPool = new CObjectPool<CHollowObject>(element_size, () => { return new CHollowObject(); });
Thread t1 = new Thread(() => {
    while(myObjectPool.Count > 0)
    {
        myObjectPool.Pop();
        Thread.Sleep(1000);
    }
});

Thread t2 = new Thread(() =>
{
    Console.WriteLine($"[Before Start - ObjectPool Count] - {myObjectPool.Count}");
    while (myObjectPool.Count > 0)
    {
        Console.WriteLine($"[ObjectPool Count] = {myObjectPool.Count}");
    }
});

t1.Start();
t2.Start();

Console.WriteLine("The End...");
*/
#endregion