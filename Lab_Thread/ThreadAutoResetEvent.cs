using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_Thread
{
    sealed public class ContentsMission
    {
        public struct sMission
        {
            public Int32 missionID { get; private set; } = 0;
            public string message { get; private set; } = "";
            public sMission(Int32 id, string message)
            {
                missionID = id;
                this.message = message;
            }
        }
        public Dictionary<Int32, sMission> mMission { get; private set; } = new Dictionary<int, sMission>();

        public bool CheckMissionExist(Int32 id)
        {
            return mMission.ContainsKey(id);
        }

        public void SetMissions(Int32 id, string message)
        {
            // 통일성과 구조체 생성자 호출을 위해서 new 로 구조체 할당 (이 경우 dictioanry 객체가 할당된 힙 영역에 구조체가 할당됨)

            if (!CheckMissionExist(id))
                mMission.Add(id, new sMission(id, message));
            else
                mMission[id] = new sMission(id, message);
        }
    }

    sealed public class ContentsReward
    {
        public struct sReward
        {
            public Int32 grade { get; private set; } = 0;
            public Int32 itemIID { get; private set; } = 0;
            public Int16 itemCount { get; private set; } = 0;

            public sReward(Int32 grade, Int32 iid, Int16 count)
            {
                this.grade = grade;
                itemIID = iid;
                itemCount = count;
            }
        }

        public Dictionary<Int32, sReward> mRewards { get; private set; } = new Dictionary<int, sReward>();
    
        public bool CheckGradeExist(Int32 grade)
        {
            return mRewards.ContainsKey(grade);
        }

        public void SetRewards(Int32 grade, Int32 iid, Int16 count)
        {
            if (!CheckGradeExist(grade))
                mRewards.Add(grade, new sReward(grade, iid, count));
            else
                mRewards[grade] = new sReward(grade, iid, count);
        }
    }

    static class ContentsManager
    {
        public static ContentsMission mMissions { get; private set; } = new ContentsMission();
        public static ContentsReward mRewards { get; private set; } = new ContentsReward();

        static public void SetMissionAndReward(ContentsMission mission, ContentsReward reward)
        {
            mMissions = mission;
            mRewards = reward;
        }

        static public async Task SetMissionAsync(ContentsMission mission, string message)
        {
            await Task.Run(() =>
            {
                int count = 0;
                string tmpMessage = "";

                for(int i = 0; i < 60; ++i)
                {
                    ++count;
                    tmpMessage = $"{count} - {message}";
                    mission.SetMissions(count, tmpMessage);
                }

                Console.WriteLine($"[Mission Thread ID] => {Thread.CurrentThread.ManagedThreadId}");
            }).ConfigureAwait(false);

            Console.WriteLine($"After Await... [Mission Thread ID] => {Thread.CurrentThread.ManagedThreadId}");
        }

        static public async Task SetRewardAsync(ContentsReward reward, Int32 iid, Int16 count)
        {
            await Task.Run(() =>
            {
                int grade = 0;
                int tmpIID = 0;

                for(int i = 0; i < 30; ++i)
                {
                    ++grade;
                    tmpIID = iid + i;
                    reward.SetRewards(grade, tmpIID, Convert.ToInt16(i + 1));
                }

                Console.WriteLine($"[Reward Thread ID] => {Thread.CurrentThread.ManagedThreadId}");
            });

            Console.WriteLine($"After Await... [Reward Thread ID] => {Thread.CurrentThread.ManagedThreadId}");
        }
    }


    public static class ThreadAutoResetEvent
    {
        public static readonly AutoResetEvent msWorkerEvent = new AutoResetEvent(false);
        public static readonly AutoResetEvent msMainEvent = new AutoResetEvent(false);

        public static void Process(int seconds)
        {
            // 작업[1]
            Console.WriteLine("Starting a long running work...");
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            Console.WriteLine("Work is done!!!");
            
            msWorkerEvent.Set();
            
            // 작업[2]
            // main 스레드는 5초동안 WaitSleepJoin 상태이고 Process 스레드가 아래의 작업 진행
            Console.WriteLine("Waiting for a main thread to complete its work");
            
            // 작업[3]
            // msMainEvent는 차단기가 내려간 상태에서 시작하였으므로 WaitOne을 만나면 Process 스레드 대기
            msMainEvent.WaitOne();
            
            // 작업[4]
            // 지정한 시간만큼 Process 스레드 대기(Running -> WaitSleepJoin)
            Console.WriteLine("Starting second operation...");
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            Console.WriteLine("Work is done");
            
            // 작업[5]
            // 해당 작업으로 msMainEvent.WaitOne에 의해서 블럭되어있던 메인스레드 차단기를 올림
            // 이후 출력문("Second operation is completed!!!") 실행
            msMainEvent.Set();
        }
    }
}
