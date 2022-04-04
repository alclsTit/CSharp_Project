using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_Design.State
{
    public enum eMonsterState
    {
        NoInitialized = 0,
        Created = 1,
        Active = 2,
        Dead = 3
    }

    public enum eMonsterType
    {
        Land = 0,
        Fly = 1
    }

    public interface IMonsterStat
    {
        int hp { get; }

        int mp { get; }

        int power { get; }
    }

    public abstract class MonsterBase
    {
        public delegate void EventHandler(MonsterBase mob);
        public event EventHandler OnDead;

        public class MonsterStat : IMonsterStat
        {
            public int hp { get; private set; } = 0;
            public int mp { get; private set; } = 0;
            public int power { get; private set; } = 0;
            public double defenseRate { get; private set; } = 0.0;

            public MonsterStat(int hp, int mp, int power, double defenseRate)
            {
                this.hp = hp;
                this.mp = mp;
                this.power = power;
                this.defenseRate = defenseRate;
            }

            public void UpdateHP(int damage)
            {
                var finalDamage = Convert.ToInt32(damage * (1.0 - defenseRate));
                hp -= finalDamage;
            }
        }

        public eMonsterState mState { get; protected set; } = eMonsterState.NoInitialized;

        public string mName { get; protected set; } = "";

        public eMonsterType mMoveType { get; protected set; } = eMonsterType.Land;

        public MonsterStat mStat { get; protected set; }

        protected MonsterBase()
        {
            mName = "Dummy";
            mStat = new MonsterStat(0, 0, 0, 0.0);
            mState = eMonsterState.Created;
        }

        public virtual void Create()
        {
            mState = eMonsterState.Active;
            Update();
        }

        public abstract void Update();
  
        public virtual void Delete()
        {
            mState = eMonsterState.Dead;
            OnDead(this);
        }

        public virtual void UpdateHP(int damage)
        {
            if (mStat.hp > 0)
                mStat.UpdateHP(damage);
            else
                Delete();
        }
    }
}
