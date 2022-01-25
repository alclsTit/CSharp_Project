using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using Lab_Pooling.ObjectPooling;

namespace Lab_Pooling
{
    public class Program
    {
        static void Main(string[] args)
        {
            var element_size = 1000000;
            var startTime = DateTime.Now;

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

            // 1. Object Pool (queue / lock)
            CObjectPool<CHollowObject> myObjectPool = new CObjectPool<CHollowObject>(element_size, () => { return new CHollowObject(); });
            int myObjectPoolCount = element_size;
            while(myObjectPoolCount > 0 )
            {
                var target = myObjectPool.Pop();
                myObjectPool.Push(target);
                --myObjectPoolCount;
            }
            var diffTime = (DateTime.Now - startTime).TotalSeconds;
            Console.WriteLine($"1.[ObjectPool] - {element_size} performance {diffTime} seconds");


            // 2. Object Pool (concurrentqueue / lock)
            CObjectPoolConcurrent<CHollowObject> myObjectPoolConcurrent = new CObjectPoolConcurrent<CHollowObject>(element_size, () => { return new CHollowObject(); });
            myObjectPoolCount = element_size;
            while (myObjectPoolCount > 0)
            {
                var target = myObjectPool.Pop();
                myObjectPool.Push(target);
                --myObjectPoolCount;
            }
            diffTime = (DateTime.Now - startTime).TotalSeconds;
            Console.WriteLine($"2.[Concurrent ObjectPool] - {element_size} performance {diffTime} seconds");

            // 3. Just use normal allocator, deallocator
            startTime = DateTime.Now;
            Queue<CHollowObject> localQ = new Queue<CHollowObject>(element_size);
            for (int i = 0; i < element_size; ++i)
                localQ.Enqueue(new CHollowObject());

            for (int i = 0; i < element_size; ++i)
                localQ.Dequeue();
      
            diffTime = (DateTime.Now - startTime).TotalSeconds;
            Console.WriteLine($"3.[Normal Allocator] - {element_size} performance {diffTime} seconds");
            
       }
    }
}
