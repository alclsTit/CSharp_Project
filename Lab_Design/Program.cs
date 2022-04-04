using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using Lab_Design.State;

namespace Lab_Design
{
    internal class Program
    {
        static void Main(string[] args)
        {
            MonsterManager.Initialize(5);
            MonsterManager.RegisterMob(eMonsterType.Land, new Goblin());
            MonsterManager.RegisterMob(eMonsterType.Land, new Goblin("WarriorGoblin", new MonsterBase.MonsterStat(200, 100, 20, 0.2)));
            MonsterManager.RegisterMob(eMonsterType.Land, new Goblin("Oger", new MonsterBase.MonsterStat(500, 250, 50, 0.5)));
            MonsterManager.RegisterMob(eMonsterType.Fly, new Eagle());
            MonsterManager.RegisterMob(eMonsterType.Fly, new Eagle("BoldEagle", new MonsterBase.MonsterStat(1000, 500, 450, 0.5)));

            Thread MobThread = new Thread(() => MonsterManager.UpdateHitMob());
            MobThread.Start();

            while (true) { }
        }
    }
}
