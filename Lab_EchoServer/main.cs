using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Lab_EchoServer.Server;
using Lab_EchoServerEngine;

namespace Lab_EchoServer
{
    internal class main
    {
        static int startNum = 1;
        static object myLock = new object();

        public static void ConvertByMemoryStream(string data, int comp)
        {
            while(true)
            {
                Span<byte> header;
                lock(myLock)
                {
                    if (startNum >= comp)
                        break;

                    header = BitConverter.GetBytes(startNum);
                    startNum += 1;
                }

                Span<byte> strToBytes = Encoding.UTF8.GetBytes(data);

                byte[] message = new byte[header.Length + strToBytes.Length];
                Buffer.BlockCopy(header.ToArray(), 0, message, 0, header.Length);
                Buffer.BlockCopy(strToBytes.ToArray(), 0, message, header.Length, strToBytes.Length);
   
                lock(myLock)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        ms.Write(message);

                        ms.Position = 0;

                        byte[] tmpBuffer = new byte[message.Length];
                        ms.Read(tmpBuffer, 0, message.Length);

                        var resultHeader = BitConverter.ToInt32(tmpBuffer, 0);
                        var resultBody = Encoding.UTF8.GetString(tmpBuffer, 4, message.Length - 4);
                        Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}/{Thread.CurrentThread.Name} - Result => {resultHeader}, {resultBody}");
                    }
                }

                // 10 msec 이하는 속도가 동일함...
                Thread.Sleep(10);
            }
        }


        static void Main(string[] args)
        {
            //SimpleServer server = new SimpleServer(100);

            //Thread workerThread = new Thread(() => { server.StartServer(8800, "127.0.0.1"); });
            //workerThread.Start();

            Class1 protoTest = new Class1();

            string inputStr;
            List<Thread> workerThreads = new List<Thread>();

            inputStr = Console.ReadLine();

            Stopwatch timeCheck = new Stopwatch();
            timeCheck.Start();
            for(var i = 0; i < 2; ++i)
            {
                workerThreads.Add(new Thread(() => { ConvertByMemoryStream(inputStr, 100); }));
                workerThreads[i].Name = i.ToString() + "_Thread";
            }
         
            // Thread.Start 이후 바로 Join하면 멀티스레드를 제대로 쓰지 못함. 싱글스레드로 돌아감.
            // ex: thread1.start -> thread1.join 시 thread1의 작업이 모두 끝날때까지 thread2의 동작이 대기...
            // Start를 통해 모든 thread를 시작시킨 후 join을 해서 멀티스레드 환경에서 모든 작업이 완료될 때까지 대기 
            foreach (var thread in workerThreads)
            {
                thread.Start();
                thread.Join();
            }

            foreach (var thread in workerThreads)
                thread.Join();

            timeCheck.Stop();
            Console.WriteLine($"Calculated Total Number => [{startNum}] / {timeCheck.Elapsed.TotalSeconds} sec");


            while(true)
            {
                Thread.Sleep(10);
            }
        }
    }
}
