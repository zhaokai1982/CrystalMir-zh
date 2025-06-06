﻿using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class CrystalBeast : MonsterObject
    {
        protected virtual byte AttackRange => 6;

        protected internal CrystalBeast(MonsterInfo info)
            : base(info)
        {
        }

        protected override bool InAttackRange()
        {
            return CurrentMap == Target.CurrentMap && Functions.InRange(CurrentLocation, Target.CurrentLocation, AttackRange);
        }

        protected override void Attack()
        {
            if (!Target.IsAttackTarget(this))
            {
                Target = null;
                return;
            }

            Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);
            bool ranged = CurrentLocation == Target.CurrentLocation || !Functions.InRange(CurrentLocation, Target.CurrentLocation, 1);

            if (!ranged)
            {
                switch (Envir.Random.Next(4))
                {
                    case 0:
                        {
                            Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Type = 0 });

                            int damage = GetAttackPower(Stats[Stat.最小攻击], Stats[Stat.最大攻击]);
                            if (damage == 0) return;

                            HalfmoonAttack(damage);

                            DelayedAction action = new DelayedAction(DelayedType.Damage, Envir.Time + 300, Target, damage, DefenceType.ACAgility);
                            ActionList.Add(action);
                        }
                        break;
                    case 1:
                        {
                            Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Type = 1 });

                            int damage = GetAttackPower(Stats[Stat.最小攻击], Stats[Stat.最大攻击]);
                            if (damage == 0) return;

                            TriangleAttack(damage, 3, 2, 500);

                            DelayedAction action = new DelayedAction(DelayedType.Damage, Envir.Time + 300, Target, damage, DefenceType.ACAgility);
                            ActionList.Add(action);
                        }
                        break;
                    case 2:
                        {
                            Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Type = 2 });

                            int damage = GetAttackPower(Stats[Stat.最小攻击], Stats[Stat.最大攻击]);
                            if (damage == 0) return;

                            WideLineAttack(damage, 3, 500, DefenceType.ACAgility, false, 3);

                            DelayedAction action = new DelayedAction(DelayedType.Damage, Envir.Time + 300, Target, damage, DefenceType.ACAgility);
                            ActionList.Add(action);
                        }
                        break;
                    case 3:
                        {
                            Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Type = 3 });

                            MirDirection jumpDir = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);

                            Point location;

                            for (int i = 0; i < 1; i++)
                            {
                                location = Functions.PointMove(CurrentLocation, jumpDir, 1);
                                if (!CurrentMap.ValidPoint(location)) return;
                            }

                            for (int i = 0; i < 1; i++)
                            {
                                location = Functions.PointMove(CurrentLocation, jumpDir, 1);

                                CurrentMap.GetCell(CurrentLocation).Remove(this);
                                RemoveObjects(jumpDir, 1);
                                CurrentLocation = location;
                                CurrentMap.GetCell(CurrentLocation).Add(this);
                                AddObjects(jumpDir, 1);

                                int damage = Stats[Stat.最大攻击];
                                if (damage > 0)

                                    FullmoonAttack(damage);

                                {
                                    DelayedAction action = new DelayedAction(DelayedType.Damage, Envir.Time + 500, location, damage, DefenceType.AC);
                                    CurrentMap.ActionList.Add(action);
                                }
                            }
                        }
                        break;

                }
            }

            else
            {
                if (Envir.Random.Next(3) == 0)
                {
                    MoveTo(Target.CurrentLocation);
                }
                else
                {
                    switch (Envir.Random.Next(2))
                    {
                        case 0:
                            {
                                Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID, Type = 0 });

                                var location = Target.CurrentLocation;
                                {

                                    Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);

                                    location = Functions.PointMove(location, Direction, 1);

                                    if (!CurrentMap.ValidPoint(location)) break;

                                    CurrentMap.GetCell(CurrentLocation).Remove(this);
                                    RemoveObjects(Direction, 1);
                                    CurrentLocation = location;
                                    CurrentMap.GetCell(CurrentLocation).Add(this);
                                    AddObjects(Direction, 1);
                                }
                                int damage = GetAttackPower(Stats[Stat.最小魔法], Stats[Stat.最大魔法]);
                                if (damage == 0) return;

                                PoisonTarget(Target, 4, 5, PoisonType.Frozen, 800);
                                PoisonTarget(Target, 5, 6, PoisonType.Dazed, 1000);

                                DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time + 1000, Target, damage, DefenceType.MACAgility, false);
                                ActionList.Add(action);

                            }
                            break;
                        case 1:
                            {
                                Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID, Type = 1 });
                                int damage = GetAttackPower(Stats[Stat.最小魔法], Stats[Stat.最大魔法]);
                                if (damage == 0) return;

                                PoisonTarget(Target, 6, 5, PoisonType.Frozen, 1000);

                                DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time + 300, Target, damage, DefenceType.MACAgility, false);
                                ActionList.Add(action);
                            }
                            break;
                    }
                }

            }
            ShockTime = 0;
            ActionTime = Envir.Time + 500;
            AttackTime = Envir.Time + AttackSpeed;
        }

        protected override void ProcessTarget()
        {
            if (Target == null) return;

            if (InAttackRange() && CanAttack)
            {
                Attack();
                return;
            }

            if (Envir.Time < ShockTime)
            {
                Target = null;
                return;
            }

            MoveTo(Target.Front);
        }

        protected override void CompleteAttack(IList<object> data)
        {
            MapObject target = (MapObject)data[0];
            int damage = (int)data[1];
            DefenceType defence = (DefenceType)data[2];
            bool aoe = data.Count >= 4 && (bool)data[3];

            if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;

            if (aoe)
            {
                var targets = FindAllTargets(2, CurrentLocation, false);

                for (int i = 0; i < targets.Count; i++)
                {
                    targets[i].Attacked(this, damage, defence);
                }
            }
            else
            {
                target.Attacked(this, damage, defence);
            }
        }

        protected override void CompleteRangeAttack(IList<object> data)
        {
            MapObject target = (MapObject)data[0];
            int damage = (int)data[1];
            DefenceType defence = (DefenceType)data[2];

            if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;

            List<MapObject> targets = FindAllTargets(2, target.CurrentLocation);

            for (int i = 0; i < targets.Count; i++)
            {
                targets[i].Attacked(this, damage, defence);
            }
        }
    }
}
