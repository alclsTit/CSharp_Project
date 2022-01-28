using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using Lab_ThreadManager.GameLib;
using Lab_ThreadManager.ThreadManager;

namespace Lab_ThreadManager
{
    public class CConfigInfo
    {
        private readonly string mFilePath = "";
        private readonly string mFileName = "";
        private string mFileRealPath = "";

        public CConfigInfo(string filepath, string filename)
        {
            mFilePath = filepath;
            mFileName = filename;
            mFileRealPath = System.IO.Path.Combine(mFilePath, mFileName);

            if (!System.IO.File.Exists(mFileRealPath))
                throw new Exception($"There is No file[{mFileRealPath}]");
        }

        public bool LoadConfig()
        {
            if (!string.IsNullOrEmpty(mFileRealPath))
            {
                var IOThreadMin = IniConfig.IniFileRead("Thread", "IOThreadMin", "0", mFileRealPath);

                var NewIOThreadMin = Convert.ToInt32(IOThreadMin);
                Interlocked.Increment(ref NewIOThreadMin);

                IniConfig.IniFileWrite("Thread", "IOThreadMin", NewIOThreadMin.ToString(), mFileRealPath);

                return true;
            }
            else
            {
                Console.WriteLine($"Config file path was wrong, check again");
                return false;
            }
        }

        public bool LoadConfigTest()
        {
            if(!string.IsNullOrEmpty(mFileRealPath))
            {
                var IOThreadMin = IniConfig.IniFileRead("Thread", "IOThreadMin", "0", mFileRealPath);

                var NewIOThreadMin = Convert.ToInt32(IOThreadMin);
                Interlocked.Decrement(ref NewIOThreadMin);

                IniConfig.IniFileWrite("Thread", "IOThreadMin", NewIOThreadMin.ToString(), mFileRealPath);

                return true;
            }
            else
            {
                Console.WriteLine($"Config file path was wrong, check again");
                return false;
            }
        }
    }

    public class Program
    {

        public static void DoSomething(object state)
        {
            DateTime curtime = DateTime.Now; 
            while(true)
            {
                var diffTime = (DateTime.Now - curtime).TotalSeconds;
                if (state.ToString() == "1")
                {
                    if (diffTime > 6)
                    {
                        Console.WriteLine($"[{state}] DoSomething function is working... Time passed {diffTime.ToString()}");
                        curtime = DateTime.Now;
                        Thread.Sleep(1000);
                    }
                }
                else
                {
                    if (diffTime > 2)
                    {
                        Console.WriteLine($"[{state}] DoSomething function is working... Time passed {diffTime.ToString()}");
                        curtime = DateTime.Now;
                        Thread.Sleep(2000);
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            try
            {
                var ableToUseWorkThead = CThreadPoolEx.GetAvailableWorkThreadNum();
                var ableToUseIOThread = CThreadPoolEx.GetAvailableIOThreadNum();

                CThreadPoolEx.ResetThreadPoolInfo(1, 2, 1, 1);

                ThreadPool.GetMinThreads(out int minwork, out int minio);
                ThreadPool.GetMaxThreads(out int maxwork, out int maxio);


                ThreadPool.QueueUserWorkItem(DoSomething, "1");
                ThreadPool.QueueUserWorkItem(DoSomething, "2");

                Task.Run(() =>
                {
                    while (true)
                    {
                        Console.WriteLine("This is called by Task");
                    }
                });
        



                /*CConfigInfo configInfo = new CConfigInfo(System.IO.Path.GetFullPath(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), @"..\..\Config")), "ServerInfo.ini");
                Thread t1 = new Thread(() => configInfo.LoadConfig());
                Thread t2 = new Thread(() => configInfo.LoadConfigTest());

                t1.Name = "LoadConfigThread";
                t2.Name = "LoadConfigTest";
                t1.Start();
                t2.Start();
                */
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in Program.Main - {ex.Message} - {ex.StackTrace}");
            }

            while(true)
            {

            }
        }
    }
}
