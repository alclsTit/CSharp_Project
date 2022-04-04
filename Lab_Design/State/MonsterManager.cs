using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Lab_Design.State
{
    /// <summary>
    /// 1. State 디자인패턴
    /// - 객체의 상태 변경발생 시 다른 대상에게 이를 알려 맞춤 작업 진행
    /// - C#에서는 event를 지원해주기 때문에 이를 바탕으로 패턴 구현
    /// </summary>
    public static class MonsterManager
    { 
        public const int MAX_MONSTER_NUM = 100;
        private static bool msIsInitialized = false;
        private static readonly object msLockObject = new object();

        private static Dictionary<eMonsterType, List<MonsterBase>> mMobs;

        public static void Initialize(int capacity = MAX_MONSTER_NUM)
        {
            if (msIsInitialized != false)
                return;

            lock (msLockObject)
            {
                if (msIsInitialized != false)
                    return;

                mMobs = new Dictionary<eMonsterType, List<MonsterBase>>(capacity);
                mMobs.Add(eMonsterType.Land, new List<MonsterBase>());
                mMobs.Add(eMonsterType.Fly, new List<MonsterBase>());
                msIsInitialized = true;
            }
        }

        private static void RemoveMob(MonsterBase mob)
        {
            var type = mob.mMoveType;

            lock (msLockObject)
            {
                mMobs[type].Remove(mob);
            }
        }

        public static bool RegisterMob(eMonsterType type, MonsterBase mob)
        {
            if (mob == null)
                throw new ArgumentNullException(nameof(mob));

            mob.OnDead += new MonsterBase.EventHandler(RemoveMob);

            lock (msLockObject)
            {
                mob.Create();
                mMobs[type].Add(mob);

                return true;
            }
        }

        public static void UpdateHitMob()
        {
            while(true)
            {
                foreach(var key in mMobs.Keys)
                {
                    foreach(var value in mMobs[key].ToList())
                    {
                        value.UpdateHP(10);
                    }
                }

                Thread.Sleep(100);
            }
        }

    }
}
