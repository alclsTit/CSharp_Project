using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Threading;

namespace Lab_ThreadManager.GameLib
{
    // Non-ThreadSafe, Thread-Safe
    public static class IniConfig
    {
        private const int MAX_LEN = 255;

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString(string section, string key, string value, string filepath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        public static extern long GetPrivateProfileString(string section, string key, string default_value, StringBuilder value, int size, string filepath);

        private static object mLock = new object();

        // Non Thread-Safe
        public static void IniFileWrite(string section, string key, string value, string filepath)
        {
            WritePrivateProfileString(section, key, value, filepath);
            Console.WriteLine($"IniFileWrite - Thread[{Thread.CurrentThread.Name}] - {value}");
        }

        // Non Thread-Safe
        public static string IniFileRead(string section, string key, string value, string filepath)
        {
            var readInfo = new StringBuilder(MAX_LEN);

            GetPrivateProfileString(section, key, value, readInfo, MAX_LEN, filepath);
            Console.WriteLine($"IniFileRead - Thread[{Thread.CurrentThread.Name}] - {readInfo.ToString().Trim()}");

            return readInfo.ToString().Trim();
        }

        // Thread-Safe
        public static void IniFileWriteThreadSafe(string section, string key, string value, string filepath)
        {
            lock(mLock)
            {
                WritePrivateProfileString(section, key, value, filepath);
            }
        }

        // Thread-Safe
        public static string IniFileReadThreadSafe(string section, string key, string value, string filepath)
        {
            var readInfo = new StringBuilder(MAX_LEN);

            lock(mLock)
            {
                GetPrivateProfileString(section, key, value, readInfo, MAX_LEN, filepath);
                Console.WriteLine($"IniFileRead - Thread[{Thread.CurrentThread.Name}] - {readInfo.ToString().Trim()}");
            }

            return readInfo.ToString().Trim();
        }
    }
}
