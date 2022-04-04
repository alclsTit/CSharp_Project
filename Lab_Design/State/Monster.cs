using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_Design.State
{
    public abstract class LandMonster : MonsterBase
    {
        public override void Create()
        {
            mMoveType = eMonsterType.Land;
            Console.WriteLine($"{mName} is created. Stat is {mStat.hp} - {mStat.mp} - {mStat.power}. This mob can't fly");

            base.Create();
        }

        public override void Update()
        {
            if (mStat.hp <= 0)
            {
                Console.WriteLine($"{mName} hp is Zero. Mob is Dead");               
                Delete();
            }
        }

        public override void Delete()
        {
            base.Delete();
            Console.WriteLine($"Landing {mName} is Dead...");
        }
    }

    public abstract class FlyingMonster : MonsterBase
    {
        public override void Create()
        {
            mMoveType = eMonsterType.Fly;
            Console.WriteLine($"{mName} is created. Stat is {mStat.hp} - {mStat.mp} - {mStat.power}. This mob can fly");

            base.Create();
        }

        public override void Update()
        {
            if (mStat.hp <= 0)
            {
                Console.WriteLine($"{mName} hp is Zero. Mob is Dead");
                Delete();
            }
        }

        public override void Delete()
        {
            base.Delete();
            Console.WriteLine($"Flying {mName} is Dead...");
        }
    }

    public class Goblin : LandMonster
    {
        public Goblin()
        {
            mName = "goblin";
            mStat = new MonsterStat(100, 50, 10, 0.1);
        }

        public Goblin(string name, MonsterStat stat)
        {
            mName = name;
            mStat = stat;
        }
    }

    public class Eagle : FlyingMonster
    {
        public Eagle()
        {
            mName = "eagle";
            mStat = new MonsterStat(300, 100, 35, 0.3);
        }

        public Eagle(string name, MonsterStat stat)
        {
            mName = name;
            mStat = stat;
        }
    }
}
