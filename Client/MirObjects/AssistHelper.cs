// 引入客户端网络相关命名空间
using Client.MirNetwork;
// 引入客户端场景相关命名空间
using Client.MirScenes;
// 引入客户端场景对话框相关命名空间
using Client.MirScenes.Dialogs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

// 引入客户端数据包命名空间，并使用别名 C
using C = ClientPackets;

namespace Client.MirObjects
{
    /// <summary>
    /// 自动技能类，用于管理自动释放技能的逻辑
    /// </summary>
    public class AutoSkill
    {
        /// <summary>
        /// 获取当前游戏场景中的用户对象
        /// </summary>
        public UserObject User
        {
            get
            {
                return GameScene.User;
            }
        }
        /// <summary>
        /// 下次释放技能的时间
        /// </summary>
        public long NextSpellTime;
        /// <summary>
        /// 技能释放间隔时间
        /// </summary>
        public int Invertal;
        /// <summary>
        /// 要释放的技能类型
        /// </summary>
        public Spell Spell;

        /// <summary>
        /// 构造函数，初始化自动技能对象
        /// </summary>
        /// <param name="spell">技能类型</param>
        /// <param name="invertal">技能释放间隔时间</param>
        public AutoSkill(Spell spell, int invertal)
        {
            Spell = spell;
            Invertal = invertal;
        }

        /// <summary>
        /// 检查技能是否处于切换开启状态
        /// </summary>
        /// <returns>如果技能开启则返回 true，否则返回 false</returns>
        public bool IsToggle()
        {
            switch (Spell)
            {
                case Spell.MagicShield:
                    // 检查智能魔法盾设置是否开启
                    return Settings.SmartSheild;

                case Spell.FlamingSword:
                    // 检查智能烈火剑法设置是否开启
                    return Settings.SmartFireHit;
                
                case Spell.TwinDrakeBlade:
                    // 检查自动双龙斩设置是否开启
                    return Settings.自动双龙斩;

                case Spell.ElementalBarrier:
                    // 检查智能元素屏障设置是否开启
                    return Settings.SmartElementalBarrier;
            }

            return false;
        }

        /// <summary>
        /// 检查技能的状态是否满足释放条件
        /// </summary>
        /// <returns>如果满足条件则返回 true，否则返回 false</returns>
        public virtual bool CheckState()
        {
            switch(Spell)
            {
                case Spell.MagicShield:
                    // 检查用户的魔法盾是否已经开启
                    return User.MagicShield;

                case Spell.ElementalBarrier:
                    // 检查用户的元素屏障是否已经开启
                    return User.ElementalBarrier;
            }

            return false;
        }

        /// <summary>
        /// 处理技能释放逻辑
        /// </summary>
        /// <returns>如果技能成功释放则返回 true，否则返回 false</returns>
        public bool Process()
        {
            // 检查技能是否未开启
            if (!IsToggle())
                return false;

            // 检查是否未到下次释放时间
            if (CMain.Time < NextSpellTime)
                return false;

            // 检查技能状态是否已经满足
            if (CheckState())
                return false;

            // 获取当前游戏场景中的用户对象
            UserObject User = GameScene.User;
            // 获取用户的指定技能
            ClientMagic magic = User.GetMagic(Spell);
            // 检查用户是否拥有该技能
            if (magic == null)
                return false;

            // 检查技能是否处于冷却时间或用户魔法值是否足够
            if (User.IsMagicInCD(Spell) || !User.CheckMagicMP(magic))
                return false;

            // 更新下次释放技能的时间
            NextSpellTime = CMain.Time + Invertal;
            // 调用游戏场景的技能释放方法
            GameScene.Scene.UseSpell(Spell);
            return true;
        }
    }

    /// <summary>
    /// 辅助助手类，用于管理自动攻击、拾取物品等辅助功能
    /// </summary>
    public class AssistHelper
    {
        // 用于记录使用物品的时间
        private long[] UseItemTime = new long[3];

        // 用于记录使用护身符的形状
        private byte useAmuletShape = 1;

        // 自动攻击开关
        public bool AutoAttack;
        // 上一个目标对象
        private MapObject LastTargetObject;
        // 当前目标对象
        private MapObject TargetObject;
        // 查找目标的方向
        private MirDirection FindDirection;
        // 下次查找目标的时间
        private long FindTargetTime;
        // 随机改变方向的时间
        private long RandomDirectionTime;
        // 最大攻击距离
        private long MaxAttackDist = 20;
        // 忽略的怪物名称列表
        private string[] IgnoreMonsterName = { "变异骷髅", "大刀", "弓箭手", "神兽", "月灵", "带刀" };
        // 当前寻路路径
        private List<Node> CurrentPath;
        // 下次执行动作的时间
        private long NextActionTime;
        // 拾取物品的时间
        private long PickItemTime;
        // 黑名单对象集合
        private HashSet<MapObject> BlackObject = new HashSet<MapObject>();
        // 击杀数量
        private int KillCount;
        // 开始时间
        private long BeginTime;

        // 物品过滤列表
        private Dictionary<string, ItemFilter> itemFilterList = new Dictionary<string, ItemFilter>();

        // 自动技能列表
        public List<AutoSkill> AutoList = new List<AutoSkill>();

        /// <summary>
        /// 获取或设置物品过滤列表
        /// </summary>
        internal Dictionary<string, ItemFilter> ItemFilterList
        {
            get
            {
                return itemFilterList;
            }

            set
            {
                itemFilterList = value;
            }
        }

        /// <summary>
        /// 构造函数，初始化自动技能列表
        /// </summary>
        public AssistHelper()
        {
            // 添加烈火剑法自动技能，间隔 500 毫秒
            AutoList.Add(new AutoSkill(Spell.FlamingSword, 500));
            // 添加双龙斩自动技能，间隔 3000 毫秒
            AutoList.Add(new AutoSkill(Spell.TwinDrakeBlade, 3000));
            // 添加魔法盾自动技能，间隔 500 毫秒
            AutoList.Add(new AutoSkill(Spell.MagicShield, 500));
            // 添加元素屏障自动技能，间隔 1000 毫秒
            AutoList.Add(new AutoSkill(Spell.ElementalBarrier, 1000));
        }

        /// <summary>
        /// 处理自动技能和自动使用物品的逻辑
        /// </summary>
        public void Process()
        {
            for (int i=0; i<AutoList.Count; ++i)
            {
                // 尝试执行自动技能
                if (AutoList[i].Process())
                    break;
            }

            // 执行自动使用物品的逻辑
            AutoUseItem();
        }

        /// <summary>
        /// 清除黑名单中的无效对象
        /// </summary>
        private void ClearBlackObjects()
        {
            // 移除在地图中找不到的黑名单对象
            BlackObject.RemoveWhere(o => { return GameScene.Scene.MapControl.FindObject(o.ObjectID, o.CurrentLocation.X, o.CurrentLocation.Y) == null; });
        }

        /// <summary>
        /// 查找目标对象
        /// </summary>
        private void FindTarget()
        {
            // 检查是否未到下次查找目标的时间
            if (FindTargetTime >= CMain.Time)
                return;

            // 检查是否需要查找目标
            if (!NeedFindTarget())
                return;

            // 更新上一个目标对象
            LastTargetObject = TargetObject;
            // 清空当前目标对象
            TargetObject = null;
            // 清空拾取物品时间
            PickItemTime = 0;
            // 获取当前游戏场景中的用户对象
            UserObject User = UserObject.User;
            // 初始化最小距离为最大值
            int MinDist = int.MaxValue;
            // 遍历地图中的所有对象
            for (int i = 0; i < MapControl.Objects.Count; ++i)
            {
                MapObject obj = MapControl.Objects[i];

                // 检查对象是否可以作为目标
                if (!CanBeTarget(obj))
                    continue;

                // 计算用户与对象之间的距离
                int dist = Functions.Distance(User.CurrentLocation, obj.CurrentLocation);
                if (dist < MinDist)
                {
                    // 更新目标对象
                    TargetObject = obj;
                    // 更新下次查找目标的时间
                    FindTargetTime = CMain.Time + 1000;
                    // 更新最小距离
                    MinDist = dist;
                }
            }

            if (TargetObject != null)
            {
                // 计算用户与目标对象之间的距离
                int dist = Functions.Distance(User.CurrentLocation, TargetObject.CurrentLocation);
                // 更新下次查找目标的时间
                FindTargetTime = CMain.Time + 1000;
                // 清空当前寻路路径
                if (CurrentPath != null)
                    CurrentPath.Clear();
                // 在聊天对话框中显示发现目标的信息
                GameScene.Scene.ChatDialog.ReceiveChat(string.Format("发现目标:{0} L:{1},{2} 坐标:{3},{4} 距离:{5}", TargetObject.Name, 
                    User.CurrentLocation.X, User.CurrentLocation.Y, TargetObject.CurrentLocation.X, TargetObject.CurrentLocation.Y, dist), ChatType.System);
                return;
            }

            // 检查是否到了随机改变方向的时间
            if (CMain.Time >= RandomDirectionTime)
            {
                // 获取当前游戏场景的地图控制器
                MapControl MapControl = GameScene.Scene.MapControl;
                // 计算用户按当前方向移动一步后的位置
                Point pt = Functions.PointMove(User.CurrentLocation, FindDirection, 1);
                // 检查该位置是否为空
                if (!MapControl.EmptyCell(pt))
                {
                    // 随机选择一个方向
                    FindDirection = (MirDirection)CMain.Random.Next(8);
                    for (int i = 0; i < 8; i++)
                    {
                        // 检查该位置是否为空
                        if (MapControl.EmptyCell(pt))
                            break;

                        // 获取下一个方向
                        FindDirection = Functions.NextDir(FindDirection);
                        // 重新计算位置
                        pt = Functions.PointMove(User.CurrentLocation, FindDirection, 1);
                    }
                }

                // 更新随机改变方向的时间
                RandomDirectionTime = CMain.Time + 1000;
                // 在聊天对话框中显示周围没有目标，开始游荡的信息
                GameScene.Scene.ChatDialog.ReceiveChat("周围没有目标，开始游荡", ChatType.System);
            }

            // 按指定方向移动
            Move(FindDirection);
        }

        /// <summary>
        /// 检查对象是否可以作为目标
        /// </summary>
        /// <param name="obj">要检查的对象</param>
        /// <returns>如果可以作为目标则返回 true，否则返回 false</returns>
        private bool CanBeTarget(MapObject obj)
        {
            // 检查对象是否为上一个目标对象
            if (obj == LastTargetObject)
                return false;

            // 检查对象是否在黑名单中
            if (BlackObject.Contains(obj))
                return false;

            // 检查对象是否为怪物且未死亡，并且可以攻击
            if (obj is MonsterObject && !obj.Dead && CheckCanAttack(obj))
                return true;

            // 检查是否开启自动拾取，并且对象是否为物品，并且需要拾取
            if (Settings.AutoPick && obj is ItemObject && NeedPick(obj.Name)) 
                return true;

            return false;
        }

        /// <summary>
        /// 检查对象是否可以攻击
        /// </summary>
        /// <param name="obj">要检查的对象</param>
        /// <returns>如果可以攻击则返回 true，否则返回 false</returns>
        private bool CheckCanAttack(MapObject obj)
        {
            // 遍历忽略的怪物名称列表
            for (int i=0; i< IgnoreMonsterName.Length; ++i)
            {
                // 检查对象名称是否包含忽略的怪物名称
                if (obj.Name.Contains(IgnoreMonsterName[i]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 检查是否需要查找目标
        /// </summary>
        /// <returns>如果需要查找目标则返回 true，否则返回 false</returns>
        private bool NeedFindTarget()
        {
            // 检查当前目标对象是否为空
            if (TargetObject == null)
                return true;

            // 获取当前游戏场景中的用户对象
            UserObject User = UserObject.User;
            // 检查目标对象是否已死亡
            if (TargetObject.Dead)
            {
                // 击杀数量加 1
                KillCount++;
                // 计算耗时（分钟）
                int second =(int) (CMain.Time - BeginTime)/60000;
                // 在聊天对话框中显示目标死亡的信息
                GameScene.Scene.ChatDialog.ReceiveChat(string.Format("目标死亡,杀敌数:{0} 耗时:{1}分钟 平均:{2}杀/分", KillCount, second, second != 0 ? KillCount / second : 0), ChatType.System);
                return true; ;
            }

            // 计算用户与目标对象之间的最大距离
            int dist = Functions.MaxDistance(User.CurrentLocation, TargetObject.CurrentLocation);
            // 检查目标对象是否超出最大攻击距离
            if (dist > MaxAttackDist)
            {
                // 在聊天对话框中显示目标超出范围的信息
                GameScene.Scene.ChatDialog.ReceiveChat("目标超出范围", ChatType.System);
                return true;
            }

            // 检查在地图中是否找不到目标对象
            if (GameScene.Scene.MapControl.FindObject(TargetObject.ObjectID, TargetObject.CurrentLocation.X, TargetObject.CurrentLocation.Y) == null)
            {
                // 在聊天对话框中显示找不到目标的信息
                GameScene.Scene.ChatDialog.ReceiveChat("找不到目标", ChatType.System);
                return true;
            }

            // 检查是否拾取超时
            if (PickItemTime > 0 && CMain.Time >= PickItemTime)
            {
                // 在聊天对话框中显示拾取超时的信息
                GameScene.Scene.ChatDialog.ReceiveChat("拾取超时", ChatType.System);
                // 将目标对象加入黑名单
                BlackObject.Add(TargetObject);
                return true;
            }

            // 检查是否无法到达物品位置
            if (TargetObject is ItemObject && TargetObject.CurrentLocation != User.CurrentLocation
                && !GameScene.Scene.MapControl.EmptyCell(TargetObject.CurrentLocation))
            {
                // 在聊天对话框中显示无法到达物品位置的信息
                GameScene.Scene.ChatDialog.ReceiveChat("无法到达物品位置", ChatType.System);
                return true;
            }
          
            return false;
        }

        /// <summary>
        /// 处理目标对象的逻辑
        /// </summary>
        /// <returns>如果处理成功则返回 true，否则返回 false</returns>
        private bool ProcessTarget()
        {
            // 检查当前目标对象是否为空
            if (TargetObject == null)
                return false;

            // 获取当前游戏场景中的用户对象
            UserObject User = UserObject.User;
            // 检查用户是否处于麻痹、冻结或钓鱼状态
            if (User.Poison.HasFlag(PoisonType.Paralysis) || User.Poison.HasFlag(PoisonType.LRParalysis) || User.Poison.HasFlag(PoisonType.Frozen) || User.Fishing)
                return false;

            // 获取当前游戏场景的地图控制器
            MapControl MapControl = GameScene.Scene.MapControl;
            // 检查目标对象是否在用户的攻击范围内
            if (Functions.InRange(TargetObject.CurrentLocation, User.CurrentLocation, 1))
            {
                if (TargetObject is MonsterObject)
                {
                    // 检查是否可以进行攻击
                    if (CMain.Time > GameScene.AttackTime && User.CanRideAttack() && CMain.Time > NextActionTime)
                    {
                        // 设置目标对象
                        MapObject.TargetObject = TargetObject;

                        // 尝试处理技能释放
                        if (ProcessSpell())
                            return true;

                        // 处理攻击逻辑
                        ProcessAttack();

                        // 设置用户的排队动作
                        User.QueuedAction = new QueuedAction { Action = MirAction.近距攻击1, Direction = Functions.DirectionFromPoint(User.CurrentLocation, TargetObject.CurrentLocation), Location = User.CurrentLocation };
                        return true;
                    }
                }
                else if (TargetObject is ItemObject)
                {
                    // 检查用户是否与物品在同一位置
                    if (User.CurrentLocation == TargetObject.CurrentLocation)
                    {
                        // 设置拾取物品的时间
                        if (PickItemTime == 0)
                            PickItemTime = CMain.Time + 2000;
                    }
                    else
                    {
                        // 计算用户到物品的方向
                        MirDirection direction = Functions.DirectionFromPoint(User.CurrentLocation, TargetObject.CurrentLocation);
                        // 检查用户是否可以向该方向行走
                        if (User.CanWalk(direction))
                            // 设置用户的排队动作
                            User.QueuedAction = new QueuedAction {
                                Action = MirAction.行走动作, Direction = direction,
                                Location = Functions.PointMove(User.CurrentLocation, direction, 1) };
                    }
                }
            }
            else
            {
                // 检查当前寻路路径是否为空
                if (CurrentPath == null || CurrentPath.Count == 0)
                {
                    if (TargetObject is MonsterObject)
                    {
                        // 计算用户到怪物的方向
                        MirDirection direction = Functions.DirectionFromPoint(TargetObject.CurrentLocation, User.CurrentLocation);
                        // 计算怪物按该方向移动一步后的位置
                        Point pt = Functions.PointMove(TargetObject.CurrentLocation, direction, 1);
                        // 检查该位置是否为空
                        if (!MapControl.EmptyCell(pt))
                        {
                            // 随机选择一个方向
                            direction = (MirDirection)CMain.Random.Next(8);
                            for (int i = 0; i < 7; i++)
                            {
                                // 检查该位置是否为空
                                if (MapControl.EmptyCell(pt))
                                    break;

                                // 获取下一个方向
                                direction = Functions.NextDir(direction);
                                // 重新计算位置
                                pt = Functions.PointMove(TargetObject.CurrentLocation, direction, 1);
                            }
                        }
                        // 查找用户到该位置的路径
                        CurrentPath = GameScene.Scene.MapControl.PathFinder.FindPath(User.CurrentLocation, pt);
                    }
                    else
                    {
                        // 查找用户到物品位置的路径
                        CurrentPath = GameScene.Scene.MapControl.PathFinder.FindPath(User.CurrentLocation, TargetObject.CurrentLocation);
                    }

                    // 在聊天对话框中显示搜索路径的信息
                    GameScene.Scene.ChatDialog.ReceiveChat("搜索路径...", ChatType.System);
                    // 检查路径是否查找失败
                    if (CurrentPath == null || CurrentPath.Count == 0)
                        // 在聊天对话框中显示搜索路径失败的信息
                        GameScene.Scene.ChatDialog.ReceiveChat("搜索路径失败...", ChatType.System);
                }

                // 检查当前寻路路径是否不为空
                if (CurrentPath != null && CurrentPath.Count > 0)
                    // 按路径移动
                    Move2();
            }

            return true;
        }

        /// <summary>
        /// 处理攻击逻辑
        /// </summary>
        private void ProcessAttack()
        {
            // 获取当前游戏场景中的用户对象
            UserObject User = MapObject.User;
            // 检查用户是否拥有双龙斩技能
            if (User.GetMagic(Spell.TwinDrakeBlade) != null)
            {
                // 释放双龙斩技能
                GameScene.Scene.UseSpell(Spell.TwinDrakeBlade);
                return;
            }

            // 检查用户是否拥有闪影步技能，并且技能未处于冷却时间
            if (User.GetMagic(Spell.FlashDash) != null && !User.IsMagicInCD(Spell.FlashDash))
            {
                // 释放闪影步技能
                GameScene.Scene.UseSpell(Spell.FlashDash);
                return;
            }
        }

        /// <summary>
        /// 处理技能释放逻辑
        /// </summary>
        /// <returns>如果技能释放成功则返回 true，否则返回 false</returns>
        private bool ProcessSpell()
        {
            // 获取当前游戏场景中的用户对象
            UserObject User = MapObject.User;
            // 获取用户的毒剑技能
            ClientMagic magic = User.GetMagic(Spell.PoisonSword);
            // 检查用户是否拥有毒剑技能，并且技能未处于冷却时间，魔法值足够，目标对象未中毒，且未到下次动作时间
            if (magic != null && !User.IsMagicInCD(Spell.PoisonSword) && User.CheckMagicMP(magic)
                && TargetObject != null && !TargetObject.Poison.HasFlag(PoisonType.Green) && CMain.Time > MapControl.NextAction)
            {
                // 设置用户下次释放技能的位置
                User.NextMagicLocation = TargetObject.CurrentLocation;
                // 设置用户下次释放技能的目标对象
                User.NextMagicObject = TargetObject;
                // 设置用户下次释放技能的方向
                User.NextMagicDirection = Functions.DirectionFromPoint(User.CurrentLocation, TargetObject.CurrentLocation);
                // 更新下次执行动作的时间
                NextActionTime += 2500;
                // 调用地图控制器的技能释放方法
                GameScene.Scene.MapControl.UseMagic(magic, User);
                return true;
            }

            // 检查用户是否拥有加速技能，并且技能未处于冷却时间，且用户未拥有加速 buff
            if (User.GetMagic(Spell.Haste) != null && !User.IsMagicInCD(Spell.Haste) && !GameScene.Scene.Buffs.Any(e => e.Type == BuffType.体迅风))
            {
                // 释放加速技能
                GameScene.Scene.UseSpell(Spell.Haste);
                // 更新下次执行动作的时间
                NextActionTime += 2500;
                return true;
            }

            // 检查用户是否拥有轻身术技能，并且技能未处于冷却时间，且用户未拥有轻身术 buff
            if (User.GetMagic(Spell.LightBody) != null && !User.IsMagicInCD(Spell.LightBody) && !GameScene.Scene.Buffs.Any(e => e.Type == BuffType.风身术))
            {
                // 释放轻身术技能
                GameScene.Scene.UseSpell(Spell.LightBody);
                // 更新下次执行动作的时间
                NextActionTime += 2500;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 设置保护物品的名称
        /// </summary>
        /// <param name="i">物品索引</param>
        /// <param name="text">物品名称</param>
        public void SetProtectItemName(int i, string text)
        {
            switch (i)
            {
                case 0:
                    // 设置第一个保护物品的名称
                    Settings.PercentItem0 = text;
                    break;

                case 1:
                    // 设置第二个保护物品的名称
                    Settings.PercentItem1 = text;
                    break;

                case 2:
                    // 设置第三个保护物品的名称
                    Settings.PercentItem2 = text;
                    break;
            }
        }

        /// <summary>
        /// 设置保护物品的百分比
        /// </summary>
        /// <param name="i">物品索引</param>
        /// <param name="temp">保护百分比</param>
        public void SetProtectPercent(int i, int temp)
        {
            switch (i)
            {
                case 0:
                    // 设置第一个保护物品的百分比
                    Settings.ProtectPercent0 = temp;
                    break;

                case 1:
                    // 设置第二个保护物品的百分比
                    Settings.ProtectPercent1 = temp;
                    break;

                case 2:
                    // 设置第三个保护物品的百分比
                    Settings.ProtectPercent2 = temp;
                    break;
            }
        }

        /// <summary>
        /// 获取保护物品的名称
        /// </summary>
        /// <param name="i">物品索引</param>
        /// <returns>保护物品的名称</returns>
        public string GetProtectItemName(int i)
        {
            switch (i)
            {
                case 0:
                    return Settings.PercentItem0;

                case 1:
                    return Settings.PercentItem1;

                case 2:
                    return Settings.PercentItem2;
            }
            return "";
        }

        /// <summary>
        /// 获取保护物品的百分比
        /// </summary>
        /// <param name="i">物品索引</param>
        /// <returns>保护物品的百分比</returns>
        public int GetProtectPercent(int i)
        {
            switch (i)
            {
                case 0:
                    return Settings.ProtectPercent0;

                case 1:
                    return Settings.ProtectPercent1;

                case 2:
                    return Settings.ProtectPercent2;
            }
            return 0;
        }

        /// <summary>
        /// 按路径移动
        /// </summary>
        private void Move2()
        {
            // 获取当前游戏场景中的用户对象
            UserObject User = UserObject.User;
            // 查找当前路径中与用户位置相同的节点
            Node currentNode = CurrentPath.SingleOrDefault(x => User.CurrentLocation == x.Location);
            if (currentNode != null)
            {
                while (true)
                {
                    // 获取路径中的第一个节点
                    Node first = CurrentPath.First();
                    // 从路径中移除第一个节点
                    CurrentPath.Remove(first);

                    // 当移除的节点为当前节点时，退出循环
                    if (first == currentNode)
                        break;
                }
            }

            // 检查当前路径是否不为空
            if (CurrentPath.Count > 0)
            {
                // 计算用户到路径中第一个节点的方向
                MirDirection dir = Functions.DirectionFromPoint(User.CurrentLocation, CurrentPath.First().Location);
                // 查找路径中用户按该方向移动两步后的节点
                Node upcomingStep = CurrentPath.SingleOrDefault(x => Functions.PointMove(User.CurrentLocation, dir, 2) == x.Location);

                // 检查用户是否可以向该方向行走
                if (!User.CanWalk(dir))
                {
                    // 重新查找用户到路径中最后一个节点的路径
                    CurrentPath = GameScene.Scene.MapControl.PathFinder.FindPath(MapObject.User.CurrentLocation, CurrentPath.Last().Location);
                    return;
                }

                // 检查是否可以跑步，并且用户可以向该方向跑步，未到下次跑步时间，用户血量足够，路径长度大于 1，且存在两步后的节点
                if (GameScene.CanRun && User.CanRun(dir) && CMain.Time > GameScene.NextRunTime && User.HP >= 10 && CurrentPath.Count > 1 && upcomingStep != null)
                {
                    // 设置用户的排队动作（跑步）
                    User.QueuedAction = new QueuedAction { Action = MirAction.跑步动作, Direction = dir, Location = Functions.PointMove(User.CurrentLocation, dir, 2) };
                    return;
                }
                // 检查用户是否可以向该方向行走
                if (User.CanWalk(dir))
                {
                    // 设置用户的排队动作（行走）
                    User.QueuedAction = new QueuedAction { Action = MirAction.行走动作, Direction = dir, Location = Functions.PointMove(User.CurrentLocation, dir, 1) };

                    return;
                }
            }
        }

        /// <summary>
        /// 清除攻击目标
        /// </summary>
        public void ClearAttack()
        {
            // 清空当前目标对象
            TargetObject = null;
        }

        /// <summary>
        /// 初始化物品过滤列表
        /// </summary>
        public void Init()
        {
            // 清空物品过滤列表
            ItemFilterList.Clear();
            // 构建物品过滤配置文件的路径
            string path = Path.Combine("./Configs/", UserObject.User.Name + "_filter.txt");
            // 检查文件是否存在
            if (!File.Exists(path))
                return;

            // 读取文件的所有行
            string[] lines = File.ReadAllLines(path);
            for (int i = 0; i < lines.Length; ++i)
            {
                // 从文件行中解析物品过滤信息
                ItemFilter itemFilter = ItemFilter.FromLine(lines[i]);
                // 将物品过滤信息添加到列表中
                ItemFilterList[itemFilter.Name] = itemFilter;
            }
        }

        /// <summary>
        /// 保存物品过滤列表到文件
        /// </summary>
        public void Save()
        {
            // 构建物品过滤配置文件的路径
            string path = Path.Combine("./Configs/", UserObject.User.Name + "_filter.txt");

            // 将物品过滤列表中的信息转换为文本列表
            var list = ItemFilterList.Values.Select(item => item.ToText()).ToList();
            // 将文本列表写入文件
            File.WriteAllLines(path, list);
        }

        /// <summary>
        /// 按指定方向移动
        /// </summary>
        /// <param name="direction">移动方向</param>
        private void Move(MirDirection direction)
        {
            // 获取当前游戏场景中的用户对象
            UserObject User = UserObject.User;
            // 获取当前游戏场景的地图控制器
            MapControl MapControl = GameScene.Scene.MapControl;
            // 检查是否可以跑步，并且用户可以向该方向跑步，未到下次跑步时间，用户血量足够，且用户未潜行或潜行且冲刺
            if (GameScene.CanRun && User.CanRun(direction) && CMain.Time > GameScene.NextRunTime && User.HP >= 10 && (!User.Sneaking || (User.Sneaking && User.Sprint)))
            {
                // 计算跑步的距离
                int distance = User.RidingMount || User.Sprint && !User.Sneaking ? 3 : 2;
                bool fail = false;
                for (int i = 1; i <= distance; i++)
                {
                    // 检查该位置的门是否打开
                    if (!MapControl.CheckDoorOpen(Functions.PointMove(User.CurrentLocation, direction, i)))
                        fail = true;
                }
                if (!fail)
                {
                    // 计算跑步后的位置
                    Point location = Functions.PointMove(User.CurrentLocation, direction, distance);
                    // 设置用户的排队动作（跑步）
                    User.QueuedAction = new QueuedAction { Action = MirAction.跑步动作, Direction = direction, Location = location};
                }
            }
            else
            {
                // 计算行走后的位置
                Point location = Functions.PointMove(User.CurrentLocation, direction, 1);
                // 检查用户是否可以向该方向行走
                if (User.CanWalk(direction))
                    // 设置用户的排队动作（行走）
                    User.QueuedAction = new QueuedAction { Action = MirAction.行走动作, Direction = direction, Location = location };
            }
        }

        /// <summary>
        /// 此方法用于根据目标对象的位置计算移动方向并调用 Move 方法进行移动。
        /// </summary>
        private void Move1()
        {
            // 获取当前用户对象
            UserObject User = UserObject.User;
            // 获取当前游戏场景的地图控制器
            MapControl MapControl = GameScene.Scene.MapControl;
            // 计算从用户当前位置到目标对象位置的方向
            MirDirection direction = Functions.DirectionFromPoint(User.CurrentLocation, TargetObject.CurrentLocation);

            // 调用 Move 方法进行移动
            Move(direction);
        }

        /// <summary>
        /// 此方法用于自动使用物品，当开启保护设置时，会调用 AutoProtect 方法三次。
        /// </summary>
        private void AutoUseItem()
        {
            // 检查是否开启保护设置
            if (Settings.开启保护)
            {
                // 循环调用 AutoProtect 方法三次
                for (int i = 0; i < 3; i++)
                {
                    AutoProtect(i);
                }
            }
        }

        /// <summary>
        /// 此方法用于自动保护逻辑，根据索引检查用户的生命值或魔法值，当满足条件时使用对应的物品。
        /// </summary>
        /// <param name="index">索引，用于区分生命值和魔法值的检查。</param>
        private void AutoProtect(int index)
        {
            // 获取当前用户对象
            UserObject User = UserObject.User;
            int value = 0;
            // 根据索引判断是检查生命值还是魔法值
            if (index == 0 || index == 2)
                // 如果用户生命值百分比大于 0 则取该值，否则取 100
                value = User.PercentHealth > 0 ? User.PercentHealth : 100;
            else
                // 如果用户魔法值百分比大于 0 则取该值，否则取 100
                value = User.PercentMana > 0 ? User.PercentMana : 100;

            // 检查生命值或魔法值是否低于保护百分比，并且是否到了可以使用物品的时间
            if (value <= GetProtectPercent(index) && CMain.Time > UseItemTime[index])
            {
                // 获取保护物品的名称
                string itemName = GetProtectItemName(index);
                // 如果物品名称为空则直接返回
                if (string.IsNullOrEmpty(itemName))
                    return;

                // 更新下次可以使用该物品的时间
                UseItemTime[index] = CMain.Time + Settings.UseItemInterval;
                // 遍历用户的背包
                for (int i = 0; i < User.Inventory.Length; i++)
                {
                    UserItem item = User.Inventory[i];
                    
                    // 检查物品是否存在，物品信息是否存在，并且物品名称是否包含保护物品名称
                    if (item != null && item.Info != null && item.Info.Name.Contains(itemName))
                    {
                        // 发送使用物品的数据包
                        Network.Enqueue(new C.UseItem { UniqueID = item.UniqueID, Grid = MirGridType.Inventory });
                        // 找到物品后跳出循环
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 此方法用于自动出售物品，遍历用户背包，将符合条件的物品出售。
        /// </summary>
        private void SellItem()
        {
            // 获取当前用户对象
            UserObject User = UserObject.User;
            // 遍历用户的背包
            for (int i = 0; i < User.Inventory.Length; i++)
            {
                UserItem item = User.Inventory[i];
                // 检查物品是否为空，物品信息是否为空，或者物品是否标记为不可出售
                if (item == null || item.Info == null || item.Info.Bind.HasFlag(BindMode.DontSell))
                    continue;

                // 检查出售物品后用户的金币是否会超出 uint 最大值
                if (GameScene.Gold + item.Price() / 2 <= uint.MaxValue)
                    // 发送出售物品的数据包
                    Network.Enqueue(new C.SellItem { UniqueID = item.UniqueID, Count = item.Count });
            }
        }

        /// <summary>
        /// 此方法用于检查指定装备槽的装备是否符合指定的类型、数量和形状要求。
        /// </summary>
        /// <param name="slot">装备槽索引。</param>
        /// <param name="itemType">物品类型。</param>
        /// <param name="count">物品数量。</param>
        /// <param name="shape">物品形状。</param>
        /// <returns>如果符合要求返回 true，否则返回 false。</returns>
        private bool CheckEquipment(int slot, ItemType itemType, int count, int shape)
        {
            // 获取当前用户对象
            UserObject User = UserObject.User;
            // 获取指定装备槽的物品
            UserItem item = User.Equipment[slot];
            // 检查物品是否存在，物品类型是否为护身符，物品数量是否足够，物品形状是否匹配
            return item != null && item.Info.Type == ItemType.护身符 && item.Count >= count && item.Info.Shape == shape;
        }

        /// <summary>
        /// 此方法在发送使用魔法的请求前，根据魔法类型自动装备相应的护身符。
        /// </summary>
        /// <param name="magic">要使用的魔法对象。</param>
        public void PrevSendUseMagic(ClientMagic magic)
        {
            // 获取当前用户对象
            UserObject User = UserObject.User;
            // 根据魔法类型进行不同的处理
            switch (magic.Spell)
            {
                case Spell.Poisoning:
                    {
                        // 检查是否开启智能切换毒药设置
                        if (!Settings.SmartChangePoison)
                            break;

                        // 尝试自动装备指定形状和数量的护身符
                        bool success = AutoEquipAmulet(useAmuletShape, 1);
                        if (success)
                        {
                            // 如果装备成功，更新护身符形状
                            if (++useAmuletShape > 2)
                                useAmuletShape = 1;
                        }
                    }
                    break;
                case Spell.PoisonCloud:
                    {
                        // 检查是否开启智能切换毒药设置
                        if (!Settings.SmartChangePoison)
                            break;

                        // 尝试自动装备指定形状和数量的护身符
                        AutoEquipAmulet(1, 1);
                        AutoEquipAmulet(0, 1);
                    }
                    break;

                case Spell.SoulFireBall:
                case Spell.SummonSkeleton:
                case Spell.Hiding:
                case Spell.MassHiding:
                case Spell.SoulShield:
                case Spell.TrapHexagon:
                case Spell.Curse:
                case Spell.Plague:
                case Spell.UltimateEnhancer:
                case Spell.BlessedArmour:
                    {
                        // 检查是否开启智能切换毒药设置
                        if (!Settings.SmartChangePoison)
                            break;

                        // 尝试自动装备指定形状和数量的护身符
                        AutoEquipAmulet(0, 1);
                    }
                    break;

                case Spell.SummonHolyDeva:
                    {
                        // 检查是否开启智能切换毒药设置
                        if (!Settings.SmartChangePoison)
                            break;

                        // 尝试自动装备指定形状和数量的护身符
                        AutoEquipAmulet(0, 2);
                    }
                    break;

                case Spell.SummonShinsu:
                    {
                        // 检查是否开启智能切换毒药设置
                        if (!Settings.SmartChangePoison)
                            break;

                        // 尝试自动装备指定形状和数量的护身符
                        AutoEquipAmulet(0, 5);
                    }
                    break;
            }
        }

        /// <summary>
        /// 此方法用于自动装备护身符，根据形状和数量要求检查装备槽或背包，若满足条件则装备。
        /// </summary>
        /// <param name="shape">护身符的形状。</param>
        /// <param name="count">护身符的数量。</param>
        /// <returns>如果装备成功返回 true，否则返回 false。</returns>
        private bool AutoEquipAmulet(byte shape, byte count)
        {
            // 获取当前用户对象
            UserObject User = UserObject.User;
            if (shape != 0)
            {
                // 检查盾牌装备槽是否已经装备了符合要求的护身符
                if (CheckEquipment((int)EquipmentSlot.盾牌, ItemType.护身符, count, shape))
                    return true;

                // 从背包中获取符合要求的毒药物品
                UserItem item =  User.GetPoison(count, shape);
                if (item == null)
                    return false;

                // 发送装备物品的数据包，将物品装备到盾牌装备槽
                Network.Enqueue(new C.EquipItem { Grid = MirGridType.Inventory, UniqueID = item.UniqueID, To = (int)EquipmentSlot.盾牌 });
            }
            else
            {
                // 检查护身符装备槽是否已经装备了符合要求的护身符
                if (CheckEquipment((int)EquipmentSlot.护身符, ItemType.护身符, count, shape))
                    return true;

                // 从背包中获取符合要求的护身符物品
                UserItem item = User.GetAmulet(count);
                if (item == null)
                    return false;

                // 发送装备物品的数据包，将物品装备到护身符装备槽
                Network.Enqueue(new C.EquipItem { Grid = MirGridType.Inventory, UniqueID = item.UniqueID, To = (int)EquipmentSlot.护身符 });
            }
          
            return true;
        }

        /// <summary>
        /// 此方法用于判断是否需要拾取指定名称的物品，会根据物品过滤列表进行判断。
        /// </summary>
        /// <param name="name">物品名称。</param>
        /// <returns>如果需要拾取返回 true，否则返回 false。</returns>
        public bool NeedPick(string name)
        {
            // 移除物品名称中的括号及括号内的内容，并去除前后空格
            name = Regex.Replace(name, @"\([\d,]+\)", string.Empty).Trim();
            // 检查物品过滤列表中是否包含该物品名称
            if (!ItemFilterList.ContainsKey(name))
            {
                // 如果不包含，则创建一个新的物品过滤规则，默认拾取该物品，不出售
                ItemFilter itemFilter = new ItemFilter() { Name = name, Pick = true, Sell = false };
                ItemFilterList[name] = itemFilter;
                return itemFilter.Pick;
            }
            else
            {
                // 如果包含，则返回该物品过滤规则中的拾取设置
                return ItemFilterList[name].Pick;
            }
        }
    }
}
