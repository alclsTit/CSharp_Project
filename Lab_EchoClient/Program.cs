using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Net;
using Lab_EchoClient.Client;

namespace Lab_EchoClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            SimpleClient simpleClient = new SimpleClient();
            Thread workerThread = new Thread(() => { simpleClient.StartClient("127.0.0.1", 8800); });
            workerThread.Start();

            while (true)
            {
                Thread.Sleep(10);
            }
        }
    }
}
