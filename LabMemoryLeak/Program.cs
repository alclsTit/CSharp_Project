using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Data;

namespace Lab_ThreadManager.ThreadManager
{
    public static class GlobalExtensions
    {
        public static string ClassName(this object name) => name.GetType().Name;

        public static string? MethodName(this object method)
        {
            var stackFrame = new System.Diagnostics.StackFrame(1).GetMethod()?.Name;
            return stackFrame;
        }
    }

    // case1. 비관리 리소스를 해제하지 않았을 경우 발생하는 메모리 누수
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class MemoryLeakCase1
    {
        public IntPtr mHandlePtr;

        public MemoryLeakCase1()
        {
            mHandlePtr = new IntPtr();
        }

    }

    public class MemoryLeakCase2Layout
    {
        public event EventHandler? MyEventHandler;

        public void InvokeEvent()
        {
            MyEventHandler?.Invoke(null, EventArgs.Empty);
        }
    }

    public class MemoryLeakCase2Element
    {
        //public IntPtr mHandlerPtr { get; private set; } = new IntPtr(); 

        // case1. 이벤트 핸들러가 정적 메서드일 경우 
        public static void EventHandlerStatic(object? sender, EventArgs e)
        {
        }

        // case2. 이벤트 핸들러가 인스턴스 메서드일 경우
        public void EventHandlerInstance(object? sender, EventArgs e)
        {
            Console.WriteLine($"Message in {this.ClassName}.{this.MethodName()}");
        }
    }


    public class NormalObject : IDisposable
    {
        private bool mDisposed = false;

        public NormalObject()
        {
            Console.WriteLine("NormalObject alloced");
        }

        ~NormalObject()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposed)
        {
            if (mDisposed)
                return;

            if (isDisposed)
            {
                // 관리 리소스 해제
            }

            // 비관리 리소스 해제
            Console.WriteLine("NormalObject dealloced");

            mDisposed = true;
        }

    }

    public class MemoryLeakCase3
    {
        public static List<Object> staticList { get; set; } = new List<Object>();

        public MemoryLeakCase3()
        {

        }

        public MemoryLeakCase3(int count)
        {
            for (int i = 0; i < count; ++i)
                staticList.Add(new object());
        }
    }

    public class MemoryLeakCase4
    {
        public List<object> myList { get; private set; }

        public void LoopAllocCase(int count)
        {
            myList = new List<object>(count);

            while (count > 0)
            {
                for (int i = 0; i < 100; ++i)
                {
                    myList.Add(new object());
                }

                --count;
            }
        }
    }

    public class MyCustomArray<T> : System.Collections.IEnumerable, ICloneable
    {
        private const int MAX_NUMBER = 100;
        private T[] mArray;

        public MyCustomArray(int capacity = MAX_NUMBER)
        {
            mArray = new T[capacity];
        }

        public MyCustomArray(T[] array)
        {
            mArray = array;
        }

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= MAX_NUMBER)
                    throw new IndexOutOfRangeException();

                return mArray[index];
            }
            set
            {
                if (index < 0 || index >= MAX_NUMBER)
                    throw new IndexOutOfRangeException();

                mArray[index] = value;
            }
        }

        protected virtual MyCustomArray<T> DeepCopy()
        {
            T[] tmpArray = new T[mArray.Length];
            Array.Copy(mArray, 0, tmpArray, 0, mArray.Length);
            return new MyCustomArray<T>(tmpArray);
        }

        public MyCustomArray<T> Clone()
        {
            return DeepCopy();
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (System.Collections.IEnumerator)GetEnumerator();
        }
       
        public Enumerator GetEnumerator()
        {
            return new Enumerator(mArray);
        }

        public void GetSumFromStartNumber(int start)
        {
            int point = 10;
            Action action = () => { ++point; Console.WriteLine($"The number => {start + point}"); };
            point *= 2;

            action(); 
        }

        public class Enumerator : System.Collections.IEnumerator
        {
            private T[] mArray;
            private int position = -1;

            public Enumerator(T[] items)
            {
                mArray = items;
            }

            public bool MoveNext()
            {
                ++position;
                return position < mArray.Length;
            }
            
            public object Current
            {
                get
                {
                    try
                    {
                        return mArray[position];
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw;
                    }
                }
            }

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            public void Reset()
            {
                position = -1;
            }
                     
        }
    }

    class MyConvertObject
    {
        private int mNumber;
        public MyConvertObject(int number)
        {
            this.mNumber = number;
        }
    }

    class MyConvertObjectTarget : MyConvertObject
    {
        private string mName;
        public MyConvertObjectTarget(string name, int number) : base(number)
        {
            this.mName = name;
        }
    }

    class UnManagedObject
    {
        public void NoOperateredFunction()
        {
            Console.WriteLine($"This is Function!!!");
        }
    }

    class UnManagedObjectChild : UnManagedObject
    {

    }

    public class Program
    {
        public static void Main(string[] args)
        {
            UnManagedObject pObject = new UnManagedObject();
            try
            {
                UnManagedObjectChild cObject = (UnManagedObjectChild)pObject;
            }
            catch (Exception)
            {
                UnManagedObjectChild cObject2 = pObject as UnManagedObjectChild;
                if (cObject2 != null)
                {
                    Console.WriteLine("Cast success!!!");
                }
                else
                {
                    Console.WriteLine("Cast Fail...");
                }

                object myObject12 = new object();
                UnManagedObjectChild cObject3 = myObject12 as UnManagedObjectChild;


            }


            try
            {
                MyCustomArray<double> myArray = new MyCustomArray<double>(5);
                myArray[1] = 1;
                myArray[2] = 2;
                myArray[3] = 3;
                myArray[4] = 4;

                for(int i = 0; i < 5; ++i)
                {
                    myArray.GetSumFromStartNumber(10 + i);
                }

                return;

                List<int> myList = new List<int>();
                myList.AddRange(new List<int>(6) { 7, 3, 2, 5, 6, 10 });
                var target = myList.FindAll((x) => { return x >= 3; });
                
                myList.Sort((left, right) => right.CompareTo(left));

                double exp1 = 0.1;
                double exp2 = 1;
                double exp3 = exp1 + exp2;
                //decimal exp_result_fail = exp1 * exp2;
                //decimal exp_result_success = (decimal)(exp1 * exp2);

                if (exp1 + exp2 == 1.1)
                {
                    Console.WriteLine("같음");
                }
                else
                {
                    Console.WriteLine("같지 않음");
                }


                object myObj = 5;

                object myObj2 = new object();
                short qp = (short)myObj2;


                Dictionary<int, string> myMap = new Dictionary<int, string>(5);
                myMap.Add(5, "a");
                myMap.Add(3, "b");
                myMap.Add(4, "c");
                myMap.Add(1, "d");
                myMap.Add(1, "d_1");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in Program.Main - {ex.Message} - {ex.StackTrace}");
            }


            int count = short.MaxValue;
            MemoryLeakCase4 leakCase4 = new MemoryLeakCase4();

            Thread t1 = new Thread(() => { leakCase4.LoopAllocCase(count); });
            t1.Start();

            t1.Join();

            while(true)
            {
                MemoryLeakCase3.staticList.Add(new object());
            }

            while (true)
            {
                MemoryLeakCase3 leakCase3 = new MemoryLeakCase3();
                count--;

                if (count % 5000 == 0)
                    Console.WriteLine($"Current Count => {count}");

                if (count <= 0)
                    break;
            }

            while(true)
            {

            }

            return;

            MemoryLeakCase2Layout layout = new MemoryLeakCase2Layout();

            while (true)
            {
                for (int i = 0; i < 10; ++i)
                {
                    //MemoryLeakCase2Element element = new MemoryLeakCase2Element();
                    //layout.MyEventHandler += MemoryLeakCase2Element.EventHandlerStatic;
                    MemoryLeakCase1 myObj = new MemoryLeakCase1();
                }
            }
        }
    }
}
