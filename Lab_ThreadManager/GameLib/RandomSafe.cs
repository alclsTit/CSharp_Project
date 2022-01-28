using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Lab_ThreadManager.GameLib
{
    /// <summary>
    /// Threadsafe 한 Random 값 추출 
    /// ThreadLocal은 스레드 독립적인 자원 생성을 보장해주는 템플릿 클래스
    /// </summary>
    public static class RandomSafe
    {
        private static int mSeed = Environment.TickCount;

        private static ThreadLocal<Random> mRandomThreadSafe;

        public static Random GetRandomClass => mRandomThreadSafe.Value;

        public static int GetRandomValue() => mRandomThreadSafe.Value.Next();

        static RandomSafe()
        {
            mRandomThreadSafe = new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref mSeed)));
        }         
    }
}
