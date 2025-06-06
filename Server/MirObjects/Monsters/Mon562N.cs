﻿using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class Mon562N : MonsterObject
    {
        public long _BuffTime;

        protected internal Mon562N(MonsterInfo info)
            : base(info)
        {
        }
        protected override bool InAttackRange()
        {
            if (Target.CurrentMap != CurrentMap) return false;
            return CurrentMap == Target.CurrentMap && Functions.InRange(CurrentLocation, Target.CurrentLocation, Info.ViewRange);
        }
        protected override void Attack()
        {
            if (!Target.IsAttackTarget(this))
            {
                Target = null;
                return;
            }

            if (!CanAttack)
                return;

            ShockTime = 0;

            Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);
            bool ranged = CurrentLocation == Target.CurrentLocation || !Functions.InRange(CurrentLocation, Target.CurrentLocation, 2);

            ActionTime = Envir.Time + 500;
            AttackTime = Envir.Time + AttackSpeed;

            if (!ranged && Envir.Random.Next(2) > 0)
            {
                if (Envir.Random.Next(4) > 0)
                {
                    Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation });

                    int damage = GetAttackPower(Stats[Stat.最小攻击], Stats[Stat.最大攻击]);
                    if (damage == 0) return;
                    HalfmoonAttack(damage);

                    DelayedAction action = new(DelayedType.Damage, Envir.Time + 300, Target, damage, DefenceType.ACAgility, false);
                    ActionList.Add(action);
                }
                else
                {
                    Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Type = 1 });

                    int damage = GetAttackPower(Stats[Stat.最小攻击], Stats[Stat.最大攻击] * 2);
                    if (damage == 0) return;
                    FullmoonAttack(damage, 500, DefenceType.ACAgility, 1, 2);

                    DelayedAction action = new(DelayedType.Damage, Envir.Time + 300, Target, damage, DefenceType.ACAgility, false);
                    ActionList.Add(action);
                }

            }
            else

                switch (Envir.Random.Next(3))
                {
                    case 0:
                        {
                            List<MapObject> targets = FindAllTargets(Info.ViewRange, CurrentLocation);
                            if (targets.Count == 0) return;

                            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID });
                            for (int i = 0; i < targets.Count; i++)
                            {
                                Target = targets[i];
                                int damage = GetAttackPower(Stats[Stat.最小魔法], Stats[Stat.最大魔法]);
                                if (damage == 0) return;

                                DelayedAction action = new(DelayedType.RangeDamage, Envir.Time + 500, Target, damage, DefenceType.MACAgility, false);
                                ActionList.Add(action);

                                Broadcast(new S.ObjectEffect { ObjectID = targets[i].ObjectID, Effect = SpellEffect.Mon562NLightning });
                                PoisonTarget(targets[i], 15, 5, PoisonType.Paralysis, 1000);
                            }
                        }
                        break;
                    case 1:
                        {
                            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID });

                            int damage = GetAttackPower(Stats[Stat.最小魔法], Stats[Stat.最大魔法] * 2);
                            if (damage == 0) return;

                            DelayedAction action = new(DelayedType.RangeDamage, Envir.Time + 500, Target, damage, DefenceType.MAC, true);
                            ActionList.Add(action);
                        }
                        break;
                    case 2:
                        {
                            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID });

                            if (Dead || Target.HasBuff(BuffType.绝对封锁)) return;

                            if (Envir.Time >= _BuffTime)
                            {
                                var hpRate = (Stats[Stat.HPRatePercent] - 25);
                                var mpRate = (Stats[Stat.MPRatePercent] - 25);

                                var stats = new Stats
                                {
                                    [Stat.HPRatePercent] = hpRate,
                                    [Stat.MPRatePercent] = mpRate,
                                };
                                Target.AddBuff(BuffType.绝对封锁, this, Settings.Second * 300, stats);
                            }
                            _BuffTime = Envir.Time + 30000;
                        }
                        break;
                }
        }
    }
}
