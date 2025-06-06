﻿using Server.MirDatabase;
using S = ServerPackets;

namespace Server.MirObjects.Monsters
{
    public class HarvestMonster : MonsterObject
    {
        protected short Quality;
        protected int RemainingSkinCount;

        private List<UserItem> _drops;


        protected internal HarvestMonster(MonsterInfo info)
            : base(info)
        {
            RemainingSkinCount = 2;
        }

        protected override void Drop() { }
        public override bool Harvest(PlayerObject player)
        {
            if (RemainingSkinCount == 0)
            {
                for (int i = _drops.Count - 1; i >= 0; i--)
                {
                    if (player.CheckGroupQuestItem(_drops[i]))
                    {
                        _drops.RemoveAt(i); 
                    }
                    else
                    {
                        if (player.CanGainItem(_drops[i]))
                        {
                            player.GainItem(_drops[i]);
                            _drops.RemoveAt(i);
                        }
                    }
                }

                if (_drops.Count == 0)
                {
                    Harvested = true;
                    _drops = null;
                    Broadcast(new S.ObjectHarvested { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation });
                }
                else player.ReceiveChat("You cannot carry anymore.", ChatType.System);

                return true;
            }

            if (--RemainingSkinCount > 0) return true;


            _drops = new List<UserItem>();

            for (int i = 0; i < Info.Drops.Count; i++)
            {
                DropInfo drop = Info.Drops[i];

                var reward = drop.AttemptDrop(EXPOwner?.Stats[Stat.物品爆率百分比] ?? 0, EXPOwner?.Stats[Stat.金币爆率百分比] ?? 0);

                if (reward != null)
                {
                    foreach (var dropItem in reward.Items)
                    {
                        UserItem item = Envir.CreateDropItem(dropItem);
                        if (item == null) continue;

                        if (drop.QuestRequired)
                        {
                            if (!player.CheckGroupQuestItem(item, false)) continue;
                        }

                        if (item.Info.Type == ItemType.肉)
                        {
                            item.CurrentDura = (ushort)Math.Max(0, item.CurrentDura + Quality);
                        }

                        _drops.Add(item);
                    }
                }
            }

            if (_drops.Count == 0)
            {
                player.ReceiveChat("没有发现任何物品", ChatType.System);
                Harvested = true;
                _drops = null;
                Broadcast(new S.ObjectHarvested { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation });
            }

            return true;
        }

        /*
        public override bool MagAttacked(MonsterObject A, int Damage)
        {
            bool B = base.MagAttacked(A, Damage);

            if (B)
                Quality = (short)Math.Max(short.MinValue, Quality - 2000);
            return true;
        }*/
    }
}
