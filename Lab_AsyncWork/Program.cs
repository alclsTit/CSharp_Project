using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace Lab_AsyncWork
{ 
    class MyTestClass
    {
        public void DoTest()
        {
            int a = 1; // a 로컬 변수는 outer 변수 
            Action action = () => Console.WriteLine($"{a}"); // 해당 람다식은 클로저 
            a = 11;
            action();
        }

        public void DoTest2()
        {
            List<Action> list = new List<Action>();
            for (int idx = 0; idx < 10; ++idx)
                list.Add(() => { Console.WriteLine(idx); });

            list.ForEach(work => work());
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            var myTest = new MyTestClass();
            myTest.DoTest();
            myTest.DoTest2();

            int length = 10;

            Action<object>[] action = new Action<object>[length];

            for (var idx = 0; idx < length; idx++)
            {
                var tmpIdx = idx;
                action[idx] = (item) => { Console.WriteLine($"This is my [{item}] - [{tmpIdx * 2}] Method"); };
            }

            for (var idx = 0; idx < length; idx++)
                action[idx]((length - idx).ToString());

        }
    }
}
