using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using Microsoft.Extensions.ObjectPool;
using System.Net.Sockets;

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
            /*var loopCount = 1000;
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

            // 4. Microsoft.Net.ObjectPool
            // Microsoft.Extensions.ObjectPool의 ObjectPool은 추상클래스
            // 하지만, Microsoft는 개체에 대한 생성자 이외에 작업은 모두 구현해둔 상태. 이를 DefaultObjectPool<T>로 사용할 수 있다
            SomethingObject mySomethingObject = new SomethingObject();
            IPooledObjectPolicy<SocketAsyncEventArgs> policy = new SocketAsyncEventArgsPoolPolicy(mySomethingObject.OnSomethingHandler);
            DefaultObjectPool<SocketAsyncEventArgs> MSObjectPool = new DefaultObjectPool<SocketAsyncEventArgs>(policy, loopCount);
            pfCheck.Restart();
            pfCheckResultTime = pfCheck.GetWorkOperationTimeDiffSeconds<int>((objCount) =>
            {
                while (objCount > 0)
                {
                    var target = MSObjectPool.Get();
                    MSObjectPool.Return(target);
                    --objCount;
                }

            }, loopCount, pfCheckCount);
            Console.WriteLine($"4.[Microsoft.Net.ObjectPool]  - {loopCount} logicloop // {pfCheckCount} operated // {pfCheckResultTime} seconds");
            */

            /*SomethingObject mySomethingObject = new SomethingObject();
            List<SocketAsyncEventArgs> eventObjList = new List<SocketAsyncEventArgs>(11);
            IPooledObjectPolicy<SocketAsyncEventArgs> policy2 = new SocketAsyncEventArgsPoolPolicy(mySomethingObject.OnSomethingHandler);
            DefaultObjectPool<SocketAsyncEventArgs> MSObjectPool2 = new DefaultObjectPool<SocketAsyncEventArgs>(policy2, 2);
            for (int i = 0; i < 3; ++i)
                eventObjList.Add(MSObjectPool2.Get());

            eventObjList.ForEach((item) => MSObjectPool2.Return(item));
            eventObjList.Clear();

            for (int i = 0; i < 3; ++i)
                eventObjList.Add(MSObjectPool2.Get());
            // 9개까지는 SocketAsyncEventArgsPoolPolicy.Create 호출이 없다가
            // 10개에서 SocketAsyncEventArgsPoolPolicy.Create 호출을 통해서 객체를 생성함

            MSObjectPool2.Return(eventObjList[0]);
            MSObjectPool2.Return(eventObjList[1]);
            MSObjectPool2.Return(eventObjList[2]); //풀이 아닌 일반 new 할당연산자를 이용하여 객체 생성(gc 수집대상)

            var eObj1 = MSObjectPool2.Get();
            var eObj2 = MSObjectPool2.Get();
            var eObj3 = MSObjectPool2.Get(); //풀에 반환되지 않음
            */

            SomethingObject mySomethingObject = new SomethingObject();
            IPooledObjectPolicy<SocketAsyncEventArgs> policy3 = new SocketAsyncEventArgsPoolPolicy(mySomethingObject.OnSomethingHandler);

            var provider = new DefaultObjectPoolProvider();
            provider.MaximumRetained = 2;

            ObjectPool<SocketAsyncEventArgs> pool = provider.Create(policy3);
            {
                var eventObj1 = pool.Get();
                var eventObj2 = pool.Get();
                var eventObj3 = pool.Get();

                var eventObj1_target = eventObj1.UserToken as CHollowObject;
                eventObj1_target?.ShowMyInfo();

                var eventObj2_target = eventObj2.UserToken as CHollowObject;
                eventObj2_target?.ShowMyInfo();

                var eventObj3_target = eventObj3.UserToken as CHollowObject;
                eventObj3_target?.ShowMyInfo();

                pool.Return(eventObj1);
                pool.Return(eventObj2);
                pool.Return(eventObj3);

                eventObj1_target = eventObj1.UserToken as CHollowObject;
                eventObj1_target?.ShowMyInfo();

                eventObj2_target = eventObj2.UserToken as CHollowObject;
                eventObj2_target?.ShowMyInfo();

                eventObj3_target = eventObj3.UserToken as CHollowObject;
                eventObj3_target?.ShowMyInfo();

                (pool as IDisposable).Dispose();

                eventObj1_target = eventObj1.UserToken as CHollowObject;
                eventObj1_target?.ShowMyInfo();

                eventObj2_target = eventObj2.UserToken as CHollowObject;
                eventObj2_target?.ShowMyInfo();

                eventObj3_target = eventObj3.UserToken as CHollowObject;
                eventObj3_target?.ShowMyInfo();
            }
           

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