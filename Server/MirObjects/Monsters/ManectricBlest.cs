using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class ManectricBlest : MonsterObject
    {
        private int _attackCount = 0;

        protected internal ManectricBlest(MonsterInfo info)
            : base(info)
        {
        }

        protected override void Attack()
        {
            if (!Target.IsAttackTarget(this))
            {
                Target = null;
                return;
            }

            Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);

            if(_attackCount >= 5)
            {
                Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Type = 2 });

                int damage = GetAttackPower(Stats[Stat.最小魔法], Stats[Stat.最大魔法]);

                List<MapObject> targets = FindAllTargets(3, CurrentLocation);

                for (int i = 0; i < targets.Count; i++)
                {
                    targets[i].Attacked(this, damage, DefenceType.MAC);

                    //TODO - Delay this
                    if (Envir.Random.Next(Settings.PoisonResistWeight) >= targets[i].Stats[Stat.毒药抵抗])
                    {
                        if (Envir.Random.Next(5) == 0)
                        {
                            targets[i].ApplyPoison(new Poison { PType = PoisonType.Frozen, Duration = 5, TickSpeed = 1000 }, this);
                        }
                    }
                }

                _attackCount = 0;

                return;
            }

            switch (Envir.Random.Next(3))
            {
                case 0:
                case 1:
                    _attackCount++;
                    base.Attack();
                    break;
                case 2:
                    {
                        int damage = GetAttackPower(Stats[Stat.最小攻击], Stats[Stat.最大攻击]);

                        Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Type = 1 });

                        MirDirection dir = Functions.PreviousDir(Direction);
                        Point tar;
                        Cell cell;

                        for (int i = 0; i < 8; i++)
                        {
                            tar = Functions.PointMove(CurrentLocation, dir, 1);
                            dir = Functions.NextDir(dir);
                            if (tar == Front) continue;

                            if (!CurrentMap.ValidPoint(tar)) continue;

                            cell = CurrentMap.GetCell(tar);

                            if (cell.Objects == null) continue;

                            for (int o = 0; o < cell.Objects.Count; o++)
                            {
                                MapObject ob = cell.Objects[o];
                                if (ob.Race != ObjectType.Player && ob.Race != ObjectType.Monster) continue;
                                if (!ob.IsAttackTarget(this)) continue;

                                ob.Attacked(this, damage, DefenceType.Agility);
                                break;
                            }
                        }
                        break;
                    }
            }

            ActionTime = Envir.Time + 300;
            AttackTime = Envir.Time + AttackSpeed;
            ShockTime = 0;
        }
       
    }
}
