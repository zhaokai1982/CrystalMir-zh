using Server.MirDatabase;
using Server.MirEnvir;
using S = ServerPackets;
// 重写攻击范围属性，扩大到10格
namespace Server.MirObjects.Monsters
{
    public class KingGuard : MonsterObject
    {
        // 重写攻击范围属性，返回固定值10
        protected virtual byte AttackRange
        {
            get
            {
                return 10;
            }
        }

        // 构造函数，接收MonsterInfo对象初始化
        protected internal KingGuard(MonsterInfo info)
            : base(info)
        {
        }
        // 判断目标是否在攻击范围内
        protected override bool InAttackRange()
        {
            return CurrentMap == Target.CurrentMap && Functions.InRange(CurrentLocation, Target.CurrentLocation, AttackRange);// 判断是近战还是远程攻击
        }

        // 攻击方法，根据距离和随机概率选择不同的攻击方式
        protected override void Attack()
        {
            if (!Target.IsAttackTarget(this))
            {
                Target = null;
                return;
            }

            ShockTime = 0;

            Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);
            bool ranged = CurrentLocation == Target.CurrentLocation || !Functions.InRange(CurrentLocation, Target.CurrentLocation, 1);

            ActionTime = Envir.Time + 300;
            AttackTime = Envir.Time + AttackSpeed;

            if (!ranged && Envir.Random.Next(3) > 0)
            {
                if (Envir.Random.Next(5) > 0)
                {
                    Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation });
                    int damage = GetAttackPower(Stats[Stat.最小攻击], Stats[Stat.最大攻击]);
                    if (damage == 0) return;

                    DelayedAction action = new DelayedAction(DelayedType.Damage, Envir.Time + 300, Target, damage, DefenceType.ACAgility, false);
                    ActionList.Add(action);
                }
                else
                {
                    Broadcast(new S.ObjectAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Type = 1 });

                    int damage = GetAttackPower(Stats[Stat.最小攻击], Stats[Stat.最大攻击] * 2);
                    if (damage == 0) return;

                    DelayedAction action = new DelayedAction(DelayedType.Damage, Envir.Time + 300, Target, damage, DefenceType.AC, true);
                    ActionList.Add(action);
                }

            }
            else
            {
                if (Envir.Random.Next(3) > 0)
                {
                    Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID });
                    AttackTime = Envir.Time + AttackSpeed + 500;
                    int damage = GetAttackPower(Stats[Stat.最小魔法], Stats[Stat.最大魔法]);
                    if (damage == 0) return;

                    DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time + 500, Target, damage, DefenceType.MAC, false);
                    ActionList.Add(action);

                }
                else
                {
                    Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, TargetID = Target.ObjectID, Type = 1 });

                    int damage = GetAttackPower(Stats[Stat.最小魔法], Stats[Stat.最大魔法] * 2);
                    if (damage == 0) return;

                    DelayedAction action = new DelayedAction(DelayedType.RangeDamage, Envir.Time + 500, Target, damage, DefenceType.MAC, true);
                    ActionList.Add(action);
                }
            }
        }

        // 完成攻击处理，对目标造成伤害
        protected override void CompleteAttack(IList<object> data)
        {
            MapObject target = (MapObject)data[0];
            int damage = (int)data[1];
            DefenceType defence = (DefenceType)data[2];
            bool aoe = (bool)data[3];

            if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;

            if (aoe)
            {
                List<MapObject> targets = FindAllTargets(3, CurrentLocation);
                if (targets.Count == 0) return;

                for (int i = 0; i < targets.Count; i++)
                {
                    if (targets[i].Attacked(this, damage, defence) <= 0) continue;
                }
            }
            else
            {
                if (target.Attacked(this, damage, defence) <= 0) return;
            }
        }

        // 完成远程攻击处理，对目标造成伤害并可能附加中毒效果
        protected override void CompleteRangeAttack(IList<object> data)
        {
            MapObject target = (MapObject)data[0];
            int damage = (int)data[1];
            DefenceType defence = (DefenceType)data[2];
            bool aoe = (bool)data[3];

            if (target == null || !target.IsAttackTarget(this) || target.CurrentMap != CurrentMap || target.Node == null) return;

            if (aoe)
            {
                List<MapObject> targets = FindAllTargets(AttackRange, CurrentLocation);
                if (targets.Count == 0) return;

                for (int i = 0; i < targets.Count; i++)
                {
                    if (targets[i].Attacked(this, damage, defence) <= 0) continue;

                    if (Envir.Random.Next(3) >= 0)
                    {
                        Broadcast(new S.ObjectEffect { ObjectID = targets[i].ObjectID, Effect = SpellEffect.KingGuard, EffectType = 0 });
                        PoisonTarget(targets[i], 5, 10, PoisonType.Slow, 1000);
                    }
                    else
                    {
                        Broadcast(new S.ObjectEffect { ObjectID = targets[i].ObjectID, Effect = SpellEffect.KingGuard, EffectType = 1 });
                        PoisonTarget(targets[i], 5, 10, PoisonType.Paralysis, 1000);
                    }
                }
            }
            else
            {
                if (target.Attacked(this, damage, defence) <= 0) return;

                PoisonTarget(target, 10, 5, PoisonType.Green, 1000);
            }
        }

        // 处理目标，包括攻击和移动逻辑
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

            MoveTo(Target.CurrentLocation);

        }
    }
}