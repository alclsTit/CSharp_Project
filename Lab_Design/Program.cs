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

            Array tmpArray = Array.CreateInstance(typeof(int), 2);
            foreach(var tmp in tmpArray)
                Console.WriteLine($"Data is {tmp}");

            //Array.ForEach<int>(tmpArray., (n) => { Console.WriteLine($"Data is {n}"); });

            Array tmpArray2D = Array.CreateInstance(typeof(int), 2, 3);
            foreach(var tmp in tmpArray2D)
            {
                Console.WriteLine($"Data is {tmp}");
            }


            int[,,] tmpArray3D = { { { 1, 2, 3 }, { 2, 3, 4 }, { 3, 4, 5 } },
                                   { {4,5,6}, { 5,6,7}, { 6,7,8} },
                                   { {7,8,9}, { 10,11,12 }, {13,14,15 }
                                 } };
            for( var i = 0; i < tmpArray3D.GetLength(0); ++i)
            {
                for(var j = 0; j < tmpArray3D.GetLength(1); ++j)
                {
                    for(var k = 0; k < tmpArray3D.GetLength(2); ++k)
                    {
                        Console.WriteLine($"[{i}, {j}, {k}] : {tmpArray3D[i,j,k]} ");
                    }
                }
            }

            TmpClass<TmpDataStruct> tmpC = new TmpClass<TmpDataStruct>();
            TmpClass<TmpDataClass> tmpC2 = new TmpClass<TmpDataClass>();
            var tmpInstance = new { mName = "string" };
            Console.WriteLine($"Type = {tmpInstance.GetType().Name}"); /* Type Result : Anonymous" */

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
