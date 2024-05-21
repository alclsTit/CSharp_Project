using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Net;
using Lab_SimpleEchoServer.Network;
using System.Collections.Concurrent;

namespace Lab_SimpleEchoServer
{
    #region "Task.Delay vs Thread.Sleep"
    /*
    class CTEST
    {
        Stopwatch mTimeChecker = new Stopwatch();

        public void DoSomething()
        {
            mTimeChecker.Restart();
            Task.Delay(3000);
            mTimeChecker.Stop();

            Console.WriteLine($"DoSomething finished - TimeElapsed[{mTimeChecker.ElapsedMilliseconds}]ms");
        }

        public async Task DoSomethingAsync()
        {
            mTimeChecker.Restart();    
            await Task.Delay(3000);
            mTimeChecker.Stop();

            Console.WriteLine($"DoSomethingAsync finished - TimeElapsed[{mTimeChecker.ElapsedMilliseconds}]ms");
        }

        public void DoSomethingBlock()
        {
            mTimeChecker.Restart();
            Thread.Sleep(3000);
            mTimeChecker.Stop();

            Console.WriteLine($"DoSomethingBlock finished - TimeElapsed[{mTimeChecker.ElapsedMilliseconds}]ms");
        }
    }

    CTEST test = new CTEST();
    // 1. 일반 void 동기함수에서 Task.Delay(3000) 호출해도 의미없다(작업을 3초동안 기다리지 않는다)
    test.DoSomething();

    // 2. 일반 void 동기함수에 Thread.Sleep(3000) 사용 시, Thread.Sleep을 호출한 스레드를 블럭한다
    // 따라서 "Thread.Sleep job Done..." 메시지가 test.DoSomethingBlock 함수 호출 후 3초 뒤에 출력된다
    test.DoSomethingBlock();

    Console.WriteLine("Thread.Sleep job Done...");

    // 3. 비동기 함수에 await Task.Delay(3000)을 할경우, await 연산자를 마주치면 스레드풀이 관리하는 별도의 작업자 스레드에서
    // 3초동안 대기요청을 하고 바로 await가 선언된 함수를 호출한 부분으로 넘어가서 이후의 작업을 진행한다. 
    // 따라서 DoSomethingAsync 함수의 await 연산자를 마주친 후 "Face to Await..."가 호출된다.
    // 3초가 지난 후 작업자 스레드에서 "DoSomethingAsync finished ..." 메시지를 출력하고 Task.WaitAll 이후의 작업을 진행한다
    var result = test.DoSomethingAsync();

    Console.WriteLine("Face to Await...");

    // Task.WaitAll(Task)를 하여 DoSomethingAsync 작업의 await 이후 작업이 완료되어 task 값을 반환할 때까지 기다린다
    // Task.WaitAll을 해주지 않을 경우 DoSomethingAsync 작업을 기다리지 않고 다음 문장을 실행한다
    Task.WhenAll(result);
    Console.WriteLine("Task.Delay job Done...");
    Console.WriteLine("All Jobs finished...");      
    */
    #endregion

    #region "SEASONPASS"
    public enum eMissionDay
    {
        DAY = 1,
        WEEK = 2
    }

    public enum eMissionType
    {
        ROUNDING_CLEAR = 1,
        ROUNDING_SCORE = 2,
        ITEM_TRADE = 3
    }

    // 저장 프로시저를 통해 전체 미션, 보상 데이터를 한꺼번에 로드 후 응답 핸들러에서 세팅 
    // 시즌패스 전체 데이터 로드는 로직 서버 프로세스가 띄워지거나 리로드 진행 시
    class SeasonPassMission
    {
        public short mCategory { get; private set; }
        public short mClearCount { get; private set; }
        public eMissionDay mMissionDay { get; private set; }
        public eMissionType mMissionType { get; private set; }

        public SeasonPassMission(short category, short count, eMissionDay day, eMissionType type)
        {
            mCategory = category;
            mClearCount = count;
            mMissionDay = day;
            mMissionType = type;
        }
    }

    class SeasonPassReward
    {
        public int mItemIID { get; private set; }
        public short mGiveCount { get; private set; }

        public SeasonPassReward(int iid, short count)
        {
            mItemIID = iid;
            mGiveCount = count;
        }
    }

    static class SeasonPassMananger
    {
        public static Dictionary<int, SeasonPassMission> msMissionDic { get; private set; } = new Dictionary<int, SeasonPassMission>();

        public static Dictionary<int, Dictionary<short, SeasonPassReward>> msRewardDic { get; private set; } = new Dictionary<int, Dictionary<short, SeasonPassReward>>();

        private static object msLockObject = new object();

        public static bool SetMissionAndRewardInfo()
        {
            try
            {
                var missionDone = SetMissionInfo();
                var rewardDone = SetRewardInfo();
                Task.WaitAll(missionDone, rewardDone);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in SeasonPassMananger.SetMissionAndRewardInfo - {ex.Message} - {ex.StackTrace}");
                return false;
            }
        }

        // 비동기 작업 내부에서 익셉션 발생 시, Task를 통해 외부로 익셉션이 전달된다
        // async void로 할경우 익셉션 정보가 외부로 전달되지 않는다
        private static async Task SetMissionInfo()
        {
            await Task.Run(() =>
            {
                for(int i = 1; i <= 10; ++i)
                {
                    msMissionDic.Add(i, new SeasonPassMission((short)i, 3, eMissionDay.DAY, eMissionType.ROUNDING_SCORE));
                }
            });
        }

        // 비동기 작업 내부에서 익셉션 발생 시, Task를 통해 외부로 익셉션이 전달된다
        // async void로 할경우 익셉션 정보가 외부로 전달되지 않는다
        private static async Task SetRewardInfo()
        {
            await Task.Run(() =>
            {
                for (int i = 1; i <= 10; ++i)
                {
                    Dictionary<short, SeasonPassReward> reward = new Dictionary<short, SeasonPassReward>(2);
                    for (short membership = 1; membership < 2; ++membership)
                    {
                        reward.Add(membership, new SeasonPassReward(i * 1000, 2));
                    }
                    msRewardDic.Add(i, reward);
                }
            });
        }

        public static void Clear()
        {
            lock(msLockObject)
            {
                msMissionDic.Clear();
                msRewardDic.Clear();
            }
        }
    }
    #endregion

    #region "AVTPRESET"
    enum eAVTSTATPRESET_STATINFO_CHANGE
    {
        CURRENT = 0,
        OTHER = 1,
        STAT_CHANGE = 2
    }


    enum eAVTSTATPRESET_FAILREASON
    {
        SUCCESS = 0,
        REQUEST_ABNORMAL_PRESET_NUMBER = 1,
        CHANGE_STAT_FUNC_LOGIC_ERROR = 2
    }


    public class AvtClass
    {
        public int mPower { get; private set; } = 0;
        public int mImpact { get; private set; } = 0;
        public int mHealth { get; private set; } = 0;
        public int mSkill { get; private set; } = 0;

        public AvtClass() { }

        public AvtClass? Copy()
        {
            return this.MemberwiseClone() as AvtClass;
        }

        public void ChangeAllStatInfo(int power, int impact, int health, int skill)
        {
            mPower = power;
            mImpact = impact;
            mHealth = health;
            mSkill = skill; 
        }
    }

    public class AvtPresetInfo
    {
        // 현재 선택한 프리셋 번호
        public int mCurPresetNumber { get; private set; }

        // 캐릭터가 보유한 프리셋 갯수
        public int mHavePresetCount { get; private set; }

        public AvtPresetInfo(int presetNum = 0, int presetCount = 0) 
        {
            mCurPresetNumber = presetNum;
            mHavePresetCount = presetCount;
        }

        public void ChangePresetNum(int presetNum) { mCurPresetNumber = presetNum; }

        public void ChangePresetCount(int count) { mHavePresetCount = count; }

    }

    public class AvtPresetStatInfo
    {
        public string mName { get; private set; } = "NO_NAME";
        public int mPower { get; private set; } = 0;
        public int mImpact { get; private set; } = 0;
        public int mHealth { get; private set; } = 0;
        public int mSkill { get; private set; } = 0;
        public AvtPresetStatInfo(string name = "NO_NAME", int power = 0, int impact = 0, int health = 0, int skill = 0)
        {
            mName = name;
            this.ChangeAllStatInfo(power, impact, health, skill);
        }

        public void ChangeAllStatInfo(int power, int impact, int health, int skill)
        {
            mPower = power;
            mImpact = impact;
            mHealth = health;
            mSkill = skill;
        }
    }

    class AvtPresetSystem
    {
        public static readonly int msMaxAvtPresetNum = 5;

        public AvtPresetInfo mAvtPresetInfo { get; private set; } = new AvtPresetInfo();
        public Dictionary<int, AvtPresetStatInfo> mAvtPresetStatInfo { get; private set; } = new Dictionary<int, AvtPresetStatInfo>();

        public AvtPresetSystem() { }

        // 1.현재 스탯 프리셋의 정보를 요청했을 때 진행되는 로직
        public eAVTSTATPRESET_FAILREASON RequestAvtPresetInfo(int presetNum)
        {
            //if (mAvtPresetInfo.mCurPresetNumber != presetNum)
            return eAVTSTATPRESET_FAILREASON.SUCCESS;
        }

        public eAVTSTATPRESET_FAILREASON ChangeStatPresetInfo(AvtClass avtInfo, eAVTSTATPRESET_STATINFO_CHANGE type, int presetNum)
        {
            if (presetNum > msMaxAvtPresetNum)
                return eAVTSTATPRESET_FAILREASON.REQUEST_ABNORMAL_PRESET_NUMBER;

            if (type == eAVTSTATPRESET_STATINFO_CHANGE.CURRENT)
            {

            }
            else if (type == eAVTSTATPRESET_STATINFO_CHANGE.OTHER || type == eAVTSTATPRESET_STATINFO_CHANGE.STAT_CHANGE)
            {
                AvtClass? copyAvtInfo = avtInfo.Copy();
                if (copyAvtInfo != null)
                {
                    // 다른 스탯 프리셋을 변경 시
                    // 1. 스탯 재조정 함수 실행을 통한 최신 스탯으로 갱신
                    // 현재 스탯을 변경 스탯 프리셋 스탯으로 바꾼 뒤 스탯 재조정
                    copyAvtInfo.ChangeAllStatInfo(2, 2, 2, 2);
                }
                else
                {
                    return eAVTSTATPRESET_FAILREASON.CHANGE_STAT_FUNC_LOGIC_ERROR;
                }
            }

            return eAVTSTATPRESET_FAILREASON.SUCCESS;
        }

        public bool IncreasePresetCount()
        {
            var Increased = mAvtPresetInfo.mHavePresetCount + 1;
            if (Increased > msMaxAvtPresetNum)
                return false;

            mAvtPresetInfo.ChangePresetCount(Increased);
            return true;
        }
    }
    #endregion

    class Session
    {

    }

    class SessionManager
    {
        private static Lazy<SessionManager> mInstance = new Lazy<SessionManager>(() => { 
            return new SessionManager(); 
        });
        public static SessionManager Instance => mInstance.Value;

        private ConcurrentDictionary<int, Session> mConcurrentDic = new ConcurrentDictionary<int, Session>();

        private static int mSessionID = 1;

        public int Count => mConcurrentDic.Count;

        private string mDateTime = "2022-05-26 03:36:00";
        private bool mDateTimeCheckFlag = false;
        private Stopwatch mStopWatch = new Stopwatch();

        private System.Timers.Timer mTimer = new System.Timers.Timer();
        public int mTestCount = 0;
        public SessionManager()
        {
            mTimer.Interval = 3000;
            mTimer.Elapsed += new System.Timers.ElapsedEventHandler(SendTimeElaspedMessage);
        }

        public void SendTimeElaspedMessage(object? sender, System.Timers.ElapsedEventArgs e)
        {
            Console.WriteLine($"Time:{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
        }

        public bool TryAdd()
        {
            ++mSessionID;
            var result = mConcurrentDic.TryAdd(mSessionID, new Session());
            return result;
        }

        public bool TryAdd(int index)
        {
            var result = mConcurrentDic.TryAdd(index, new Session());
            return result;
        }

        public bool TryRemove(int id)
        {
            if (id <= 0)
                return false;

            return mConcurrentDic.TryRemove(id, out var session);
        }

        public async Task CheckParallelFunc()
        {
            await Task.Run(() =>
            {
                Parallel.For(1, 100, (index) =>
                {
                    Console.WriteLine($"Task count = {index}");
                    this.TryAdd(index);
                    this.TryRemove(index);
                });
            });

            mDateTimeCheckFlag = true;
            mTimer.Start();
        }


        public void TestTImeCheck()
        {
            while(true)
            {
                if (mDateTimeCheckFlag)
                {                   
                    TimeSpan ts = Convert.ToDateTime(mDateTime) - DateTime.Now;
                    if (ts.TotalMilliseconds < 0)
                    {
                        mTimer.Stop();
                        mDateTimeCheckFlag = false;
                    }
                }
            }
        }

        public async Task CheckCount()
        {
            await Task.Delay(1000);
            mTestCount++;
        }
    }

    public static class CTEST
    {
        public static int msCount = 10;
        private static object mObjectLock = new object();

        public static void IncreaseCount()
        {
            int count = 0;
            lock(mObjectLock)
            {
                count = msCount;
            }

            for (int i = 0; i < 1000; ++i)
                ++count;

            Console.WriteLine($"Count = {count} ThreadNum = {Thread.CurrentThread.ManagedThreadId}");
        }

        public static void ShowThreadNumber(object? number)
        {
            Console.WriteLine($"Thread ID[{number}] = {Thread.CurrentThread.ManagedThreadId}");
        }
    }

    internal class Program
    {
        /*       
        static void Main(string[] args)
        {
            Thread t1 = new Thread(new ThreadStart(CTEST.IncreaseCount));
            Thread t2 = new Thread((item) => CTEST.ShowThreadNumber(item));
            Thread t3 = new Thread(CTEST.IncreaseCount);
            Thread t4 = new Thread(new ParameterizedThreadStart(CTEST.ShowThreadNumber));

            Parallel.For(0, 8, (item) => { CTEST.IncreaseCount(); });

            return;

            #region "SEASONPASS"
            if (SeasonPassMananger.SetMissionAndRewardInfo())
            {
                Console.WriteLine("Task Job success...");
            }
            else
            {
                Console.WriteLine("Task Job fail...");
            }
            #endregion

            #region "AVTPRESET"
            var avtpreset = new AvtPresetSystem();
            avtpreset.ChangeStatPresetInfo(new AvtClass(), eAVTSTATPRESET_STATINFO_CHANGE.OTHER, 2);
            #endregion

            var result = SessionManager.Instance.CheckParallelFunc();
            Task.WaitAll(result);
            Console.WriteLine($"SessionManager.Instance.CheckParallelFunc Done... {SessionManager.Instance.Count}");
            SessionManager.Instance.TestTImeCheck();

            var result = SessionManager.Instance.CheckCount();
            //SessionManager.Instance.mTestCount += 10;
            for(int i = 0; i < 1000; ++i)
            {
                SessionManager.Instance.mTestCount++;
            }
            Task.WaitAll(result);

            while(true)
            {

            }
            return;

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("192.168.0.12"), 8800);
            List<Listener> Listeners = new List<Listener>(3);
            List<Thread> ModuleThreads = new List<Thread>(3);

            for (int i = 0; i < 1; ++i)
            {
                Listener listener = new Listener();
                Listeners.Add(listener);
            }

            foreach (var listener in Listeners)
            {
                Thread thread = new Thread(() => { listener.Start(endPoint, 4096, 4096, 100, 1000); });
                ModuleThreads.Add(thread);
            }

            foreach (var module in ModuleThreads)
                module.Start();

            while (true)
            {
                Thread.Sleep(1000);
            }
        }
        */
    }
}
