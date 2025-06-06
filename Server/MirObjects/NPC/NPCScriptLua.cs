using System.Drawing;
using Server.MirDatabase;
using Server.MirEnvir;
using System.Text.RegularExpressions;
using S = ServerPackets;
using NLua;
using System.Text;
using System.IO;
using System.Numerics;



namespace Server.MirObjects
{

    public partial class NPCScript
    {
        public string LuaFileName = null;

        Lua lua;

        public void LoadLua()
        {
            if (lua! != null)
            {
                lua.Close();
                lua?.Dispose();
                lua = null;
            }
            lua = new Lua();
            lua.State.Encoding = Encoding.UTF8;
            //初始化Lua库
            lua.LoadCLRPackage();
            RegistGlobalLuaFunctions();
            try
            {
                lua.DoFile(LuaFileName);
            }
            catch (NLua.Exceptions.LuaException ex)
            {
                MessageQueue.Enqueue($"脚本错误：{ex.Message}");
            }
        }

        public void CallLua(string key)
        {
            if (key.Contains("[@_"))
            {
                key = key.Remove(0, 3);
                key = key.Remove(key.Length - 1);
            }
            else
            {
                key = key.Remove(0, 2);
                key = key.Remove(key.Length - 1);
            }

            if (key.EndsWith(')'))
                try
                {
                    lua.DoString(key);
                }
                catch (NLua.Exceptions.LuaException ex)
                {
                    MessageQueue.Enqueue($"脚本错误：{ex.Message}");
                }

            else
                try
                {
                    lua.DoString(key + "()");
                }
                catch (NLua.Exceptions.LuaException ex)
                {
                    MessageQueue.Enqueue($"脚本错误：{ex.Message}");
                }

        }
        public void CallLua(MonsterObject monster, string key)
        {
            lua["monster"] = monster;
            CallLua(key);
            lua["monster"] = null;
        }
        public void CallLua(PlayerObject player, uint objectID, string key)
        {
            lua["player"] = player;
            lua["objectID"] = objectID;
            CallLua(key);
            lua["player"] = null;
            lua["objectID"] = null;
        }
        #region 注册全局命令
        public void RegistGlobalLuaFunctions()
        {
            List<string> functions = new List<string> {
                "ISADMIN",
                "GIVEGOLD",
                "MOVE",
                "INSTANCEMOVE",
                "TAKEGOLD",
                "GIVEITEM",
                "TAKEITEM",
                "OpenNPC",
                "GETPLAYINFO",
                "LOCALMESSAGE",
                "GLOBALMESSAGE",
                "USERNAME",
                "CHECKHUM",
                "GROUPLEADER",
                "GROUPCHECKNEARBY",
                "MONCLEAR",
                "SETTIMER",
                "TIMERECALL",
                "TIMERECALLGROUP",
                "GROUPTELEPORT",
                "GROUPTELEPORTINSTANCE",
                "BREAKTIMERECALL",
                "EXPIRETIMER" ,
                "HASBAGSPACE",
                "MONGEN",
                "GIVEBUFF",
                "REMOVEBUFF",
                "CHANGEGENDER",
                "SET",
                "GIVECREDIT",
                "TAKECREDIT",
                "CHECKITEM",
                "LOADVALUE",
                "SAVEVALUE",
                "PLAYSOUND",
                "SETEFFECT",
                "GETEQUIPGRADE",
                "GETEQUIPSTAT",
                "CHECKCLASS",
                "EQUIPUP",
                "GETUID",
                "CHECKGOLD",
                "CHECKCREDIT",
                "DELITEM",
                "CHECKMAP",
                "CHECKPKPOINT",
                "LEVEL",
                "INGUILD",
                "ADDTOGUILD",
                "CHECKQUEST",
                "SETQUEST",
                "CHECKACCOUNTLIST",
                "DELACCOUNTLIST",
                "HASBUFF",
            };

            foreach (var functionName in functions)
            {
                try
                {
                    lua.RegisterFunction(functionName, this, typeof(NPCScript).GetMethod(functionName));
                }
                catch (NLua.Exceptions.LuaException ex)
                {

                    MessageQueue.Enqueue($"脚本错误：{ex.Message}");
                }
                catch (System.Reflection.AmbiguousMatchException ex)
                {

                    MessageQueue.Enqueue($"系统错误：{ex.Message}");
                }

            }
        }
        #endregion
        public bool ISADMIN()
        {
            var player = lua["player"] as PlayerObject;
            return player.IsGM;
        }
        public void GIVEGOLD(uint gold)
        {
            var player = lua["player"] as PlayerObject;
            if (gold + player.Account.Gold >= uint.MaxValue)
                gold = uint.MaxValue - player.Account.Gold;
            player.GainGold(gold);


        }
        public void MOVE(string MapNumber, int X_Coord, int Y_Coord)
        {
            var player = lua["player"] as PlayerObject;
            Map map = Envir.GetMapByNameAndInstance(MapNumber);
            if (map == null) return;

            var coords = new Point(X_Coord, Y_Coord);

            if (coords.X > 0 && coords.Y > 0) player.Teleport(map, coords);
            else player.TeleportRandom(200, 0, map);
        }
        public void INSTANCEMOVE(string MapNumber, int InstanceID, int X_Coord, int Y_Coord)
        {
            var player = lua["player"] as PlayerObject;

            var map = Envir.GetMapByNameAndInstance(MapNumber, InstanceID);
            if (map == null) return;
            player.Teleport(map, new Point(X_Coord, Y_Coord));
        }
        public void TAKEGOLD(uint gold)
        {
            var player = lua["player"] as PlayerObject;
            if (gold >= player.Account.Gold) gold = player.Account.Gold;
            player.Account.Gold -= gold;
            player.Enqueue(new S.LoseGold { Gold = gold });
        }
        public void GIVEITEM(string ItemName, ushort Amount)
        {
            var player = lua["player"] as PlayerObject;
            var info = Envir.GetItemInfo(ItemName);

            if (info == null)
            {
                MessageQueue.Enqueue("无法获取物品信息: " + ItemName);
                return;
            }

            while (Amount > 0)
            {
                UserItem item = Envir.CreateFreshItem(info);

                if (item == null)
                {
                    MessageQueue.Enqueue("无法创建用户物品" + ItemName);
                    return;
                }

                if (item.Info.StackSize > Amount)
                {
                    item.Count = Amount;
                    Amount = 0;
                }
                else
                {
                    Amount -= item.Info.StackSize;
                    item.Count = item.Info.StackSize;
                }

                if (player.CanGainItem(item))
                    player.GainItem(item);
            }
        }
        public void TAKEITEM(string ItemName, ushort count, int Dura = -1)
        {
            var player = lua["player"] as PlayerObject;
            var info = Envir.GetItemInfo(ItemName);

            if (info == null)
            {
                MessageQueue.Enqueue("TAKEITEM命令未能获取物品信息" + ItemName);
                return;
            }

            for (int j = 0; j < player.Info.Inventory.Length; j++)
            {
                UserItem item = player.Info.Inventory[j];
                if (item == null) continue;
                if (item.Info != info) continue;
                bool checkDura = Dura != -1;
                if (checkDura)
                {
                    if (item.CurrentDura < (Dura * 1000)) continue;
                }


                if (count > item.Count)
                {
                    player.Enqueue(new S.DeleteItem { UniqueID = item.UniqueID, Count = item.Count });
                    player.Info.Inventory[j] = null;

                    count -= item.Count;
                    continue;
                }

                player.Enqueue(new S.DeleteItem { UniqueID = item.UniqueID, Count = count });
                if (count == item.Count)
                    player.Info.Inventory[j] = null;
                else
                    item.Count -= count;
                break;
            }
            player.RefreshStats();
        }
        public void OpenNPC(string content, int image = 0, int maxLine = 0)
        {
            if (content != null)
            {
                var player = lua["player"] as PlayerObject;
                char[] separators = new char[] { '\n' };
                // 使用String.Split方法分割字符串
                string[] parts = content.Split(separators, StringSplitOptions.None);
                // 将分割后的字符串数组转换为List<string>
                player.NPCSpeech = new List<string>(parts);

                player.Enqueue(new S.NPCResponse { Page = player.NPCSpeech });
            }
        }
        public string GETPLAYINFO(string SAY)
        {
            var player = lua["player"] as PlayerObject;
            var objectID = lua["objectID"];
            var npcObjectId = player.NPCObjectID;
            if (objectID is double)
            {
                player.NPCObjectID = Convert.ToUInt32((double)objectID);
            }
            else
            {
                // Handle the case where objectID is not a double
                throw new InvalidCastException("objectID is not of type double");
            }
            List<string>
               say = new List<string>(),
               buttons = new List<string>(),
               elseSay = new List<string>(),
               elseActs = new List<string>(),
               elseButtons = new List<string>(),
               gotoButtons = new List<string>();
            NPCPage page = new NPCPage("");
            var v = new NPCSegment(page, say, buttons, elseSay, elseButtons, gotoButtons).ReplaceValue(player, $"<${SAY}>");
            player.NPCObjectID = npcObjectId;
            return v;
        }
        public void LOCALMESSAGE(int type, string Content)
        {
            var player = lua["player"] as PlayerObject;
            ChatType chatType;

            if (!Enum.TryParse(type.ToString(), true, out chatType)) return;
            player.ReceiveChat(Content, chatType);
        }
        public void GLOBALMESSAGE(int type, string Content)
        {
            var player = lua["player"] as PlayerObject;
            if (!Enum.TryParse(type.ToString(), true, out ChatType chatType)) return;
            var p = new S.Chat { Message = Content, Type = chatType };
            Envir.Broadcast(p);
        }
        public string USERNAME()
        {
            return GETPLAYINFO("USERNAME");
        }
        public int CHECKHUM(string mapfile, int instanceValue = 1)
        {
            Map map = Envir.GetMapByNameAndInstance(mapfile, instanceValue);
            if (map == null)
            {
                return 0;
            }
            return map.Players.Count();
        }
        public bool GROUPLEADER()
        {
            var player = lua["player"] as PlayerObject;
            return (player.GroupMembers != null && player.GroupMembers[0] == player);
        }
        public bool GROUPCHECKNEARBY()
        {
            bool failed = false;
            var player = lua["player"] as PlayerObject;
            Point target = new Point(-1, -1);
            for (int j = 0; j < player.CurrentMap.NPCs.Count; j++)
            {
                NPCObject ob = player.CurrentMap.NPCs[j];
                if (ob.ObjectID != player.NPCObjectID) continue;
                target = ob.CurrentLocation;
                break;
            }
            if (target.X == -1)
            {
                failed = true;
            }
            if (player.GroupMembers == null)
                failed = true;
            else
            {
                for (int j = 0; j < player.GroupMembers.Count; j++)
                {
                    if (player.GroupMembers[j] == null) continue;
                    failed |= !Functions.InRange(player.GroupMembers[j].CurrentLocation, target, 9);
                }
            }
            return !failed;
        }
        public void MONCLEAR(string mapfile, int instanceValue = 1)
        {

            Map map = Envir.GetMapByNameAndInstance(mapfile, instanceValue);
            if (map == null) return;
            foreach (var cell in map.Cells)
            {
                if (cell == null || cell.Objects == null) continue;

                for (int j = 0; j < cell.Objects.Count(); j++)
                {
                    MapObject ob = cell.Objects[j];

                    if (ob.Race != ObjectType.Monster) continue;
                    if (ob.Dead) continue;
                    ob.Die();
                }
            }
        }
        public void SETTIMER(string name, int seconds, byte type)
        {
            var player = lua["player"] as PlayerObject;
            if (seconds < 0) seconds = 0;
            player.SetTimer(name, seconds, type);

        }
        public void TIMERECALL(long Time, string PageName = "")
        {
            var player = lua["player"] as PlayerObject;
            var tempString = "";
            if (PageName.Length > 0) tempString = "[" + PageName + "]";

            Map tempMap = player.CurrentMap;
            Point tempPoint = player.CurrentLocation;

            var action = new DelayedAction(DelayedType.NPC, Envir.Time + (Time * 1000), player.NPCObjectID, player.NPCScriptID, tempString, tempMap, tempPoint);
            player.ActionList.Add(action);
        }
        public void TIMERECALLGROUP(long Time, string PageName = "")
        {
            var player = lua["player"] as PlayerObject;
            var tempString = "";
            if (player.GroupMembers == null) return;
            if (PageName.Length > 0) tempString = "[" + PageName + "]";

            for (int j = 0; j < player.GroupMembers.Count(); j++)
            {
                var groupMember = player.GroupMembers[j];

                var action = new DelayedAction(DelayedType.NPC, Envir.Time + (Time * 1000), player.NPCObjectID, player.NPCScriptID, tempString, player.CurrentMap, player.CurrentLocation);
                groupMember.ActionList.Add(action);
            }
        }
        public void GROUPTELEPORT(string MapNumber, int X_Coord, int Y_Coord)
        {
            var player = lua["player"] as PlayerObject;
            if (player.GroupMembers == null) return;

            var map = Envir.GetMapByNameAndInstance(MapNumber, 1);
            if (map == null) return;

            for (int j = 0; j < player.GroupMembers.Count(); j++)
            {
                if (X_Coord == 0 || Y_Coord == 0)
                {
                    player.GroupMembers[j].TeleportRandom(200, 0, map);
                }
                else
                {
                    player.GroupMembers[j].Teleport(map, new Point(X_Coord, Y_Coord));
                }
            }
        }
        public void GROUPTELEPORTINSTANCE(string MapNumber, int InstanceID, int X_Coord, int Y_Coord)
        {
            var player = lua["player"] as PlayerObject;
            if (player.GroupMembers == null) return;

            var map = Envir.GetMapByNameAndInstance(MapNumber, InstanceID);
            if (map == null) return;

            for (int j = 0; j < player.GroupMembers.Count(); j++)
            {
                if (X_Coord == 0 || Y_Coord == 0)
                {
                    player.GroupMembers[j].TeleportRandom(200, 0, map);
                }
                else
                {
                    player.GroupMembers[j].Teleport(map, new Point(X_Coord, Y_Coord));
                }
            }
        }
        public void BREAKTIMERECALL()
        {
            var player = lua["player"] as PlayerObject;
            foreach (DelayedAction ac in player.ActionList.Where(u => u.Type == DelayedType.NPC))
            {
                ac.FlaggedToRemove = true;
            }
        }
        public void EXPIRETIMER(string name)
        {
            var player = lua["player"] as PlayerObject;
            player.ExpireTimer(name);
        }
        public int HASBAGSPACE()
        {
            var player = lua["player"] as PlayerObject;

            int slotCount = 0;

            for (int k = 0; k < player.Info.Inventory.Length; k++)
                if (player.Info.Inventory[k] == null) slotCount++;
            return slotCount;
        }
        public void MONGEN(string MapNumber, int InstanceID, string MonsterName, int X_Coord, int Y_Coord, int count)
        {

            Map map = Envir.GetMapByNameAndInstance(MapNumber, InstanceID);
            if (map == null) return;

            MonsterInfo monInfo = Envir.GetMonsterInfo(MonsterName);
            if (monInfo == null) return;

            for (int j = 0; j < count; j++)
            {
                MonsterObject monster = MonsterObject.GetMonster(monInfo);
                if (monster == null) return;
                monster.Direction = 0;
                monster.ActionTime = Envir.Time + 1000;
                monster.Spawn(map, new Point(X_Coord, Y_Coord));
            }
        }
        public void GIVEBUFF(string BuffName, int Time, string buffStats, bool Infinite, bool Visible, int statValue)
        {
            var player = lua["player"] as PlayerObject;
            var buffStatsList = new Stats();
            if (Enum.TryParse(buffStats, out Stat enumValue))
            {
                buffStatsList[enumValue] = statValue;
            }
            try
            {
                player.AddBuff((BuffType)(byte)Enum.Parse(typeof(BuffType), BuffName, true), player, Settings.Second * Time, buffStatsList, Visible);
            }
            catch (Exception) { 
            
            }
        }
        public void REMOVEBUFF(string BuffName)
        {
            var player = lua["player"] as PlayerObject;
            if (!Enum.IsDefined(typeof(BuffType), BuffName)) return;
            BuffType bType = (BuffType)(byte)Enum.Parse(typeof(BuffType), BuffName);
            player.RemoveBuff(bType);
        }
        public void CHANGEGENDER()
        {
            var player = lua["player"] as PlayerObject;
            switch (player.Info.Gender)
            {
                case MirGender.男性:
                    player.Info.Gender = MirGender.女性;
                    break;
                case MirGender.女性:
                    player.Info.Gender = MirGender.男性;
                    break;
            }
        }
        public void SET(int flag, uint status)
        {

            if (flag < 0 || flag >= Globals.FlagIndexCount) return;
            var flagIsOn = Convert.ToBoolean(status);
            var player = lua["player"] as PlayerObject;
            player.Info.Flags[flag] = flagIsOn;
            for (int f = player.CurrentMap.NPCs.Count - 1; f >= 0; f--)
            {
                if (Functions.InRange(player.CurrentMap.NPCs[f].CurrentLocation, player.CurrentLocation, Globals.DataRange))
                    player.CurrentMap.NPCs[f].CheckVisible(player);
            }

            if (flagIsOn) player.CheckNeedQuestFlag(flag);
        }
        public void GIVECREDIT(uint Amount)
        {
            var player = lua["player"] as PlayerObject;
            if (Amount + player.Account.Credit >= uint.MaxValue)
                Amount = uint.MaxValue - player.Account.Credit;
            player.GainCredit(Amount);
        }
        public void TAKECREDIT(uint Amount)
        {
            var player = lua["player"] as PlayerObject;
            if (Amount >= player.Account.Credit) Amount = player.Account.Credit;
            player.Account.Credit -= Amount;
            player.Enqueue(new S.LoseCredit { Credit = Amount });
        }
        public ushort CHECKITEM(string ItemName, int dura = 0)
        {
            var player = lua["player"] as PlayerObject;
            var info = Envir.GetItemInfo(ItemName);
            ushort count = 0;
            foreach (var item in player.Info.Inventory.Where(item => item != null && item.Info == info))
            {
                if (dura > 0)
                    if (item.CurrentDura < (dura * 1000)) continue;

                count += item.Count;
            }
            return count;
        }
        public void LOADVALUE(string filePath, string header, string key, bool writeWhenNull = true)
        {
            InIReader reader = new InIReader(filePath);
            string loadedString = reader.ReadString(header, key, "", writeWhenNull);

            if (loadedString == "") return;

        }
        public void SAVEVALUE(string filePath, string header, string key, string val)
        {
            InIReader reader = new InIReader(filePath);
            reader.Write(header, key, val);
        }
        public void PLAYSOUND(int soundID)
        {
            var player = lua["player"] as PlayerObject;
            player.Enqueue(new S.PlaySound { Sound = soundID });
        }
        public void SETEFFECT()
        {
            //todo
        }
        public int GETEQUIPGRADE(int slot)
        {
            var player = lua["player"] as PlayerObject;
            ushort level = player.Level;
            MirClass job = player.Class;
            var grade = -1;
            if (player.Info.Equipment[slot] == null)
            {
                return grade;
            }
            grade = (int)Functions.GetRealItem(player.Info.Equipment[slot].Info, level, job, new List<ItemInfo>()).Grade;

            return grade;

        }
        public int GETEQUIPSTAT(int slot, string stat)
        {
            var player = lua["player"] as PlayerObject;
            ushort level = player.Level;
            MirClass job = player.Class;
            if (player.Info.Equipment[slot] == null)
            {
                return -1;
            }
            var realItem = Functions.GetRealItem(player.Info.Equipment[slot].Info, level, job, new List<ItemInfo>());
            if (realItem == null || realItem.Stats == null)
            {
                return -1;
            }
            Stat statEnum;
            if (Enum.TryParse<Stat>(stat, out statEnum))
            {
                return player.Info.Equipment[slot].AddedStats[statEnum];
            }
            else
            {
                return -1;
            }
        }
        public string CHECKCLASS()
        {
            var player = lua["player"] as PlayerObject;
            return Enum.GetName(typeof(MirClass), player.Class) ?? "未知职业";
        }
        public bool EQUIPUP(int slot, string statName, int value)
        {
            var player = lua["player"] as PlayerObject;
            var item = player.Info.Equipment[slot];
            if (item == null)
            {
                return false;
            }
            if (Enum.TryParse<Stat>(statName, out Stat stat))
            {
                item.AddedStats[stat] = value;
            }
            else
            {
                return false;
            }

            player.Enqueue(new S.RefreshItem { Item = item });
            return true;
        }
        public ulong GETUID(int slot)
        {
            var player = lua["player"] as PlayerObject;
            return player.Info.Equipment[slot].UniqueID;
        }
        public uint CHECKGOLD()
        {
            var player = lua["player"] as PlayerObject;
            return player.Account.Gold;
        }
        public uint CHECKCREDIT()
        {
            var player = lua["player"] as PlayerObject;
            return player.Account.Credit;
        }
        public void DELITEM(int slot, ushort count)
        {
            var player = lua["player"] as PlayerObject;
            var uid = GETUID(slot);
            if (uid != 0)
            {
                player.Enqueue(new S.DeleteItem { UniqueID = uid, Count = count });
            }
        }

        public bool CHECKMAP(string MapName)
        {
            var player = lua["player"] as PlayerObject;
            Map map = Envir.GetMapByNameAndInstance(MapName);
            return player.CurrentMap == map;
        }
        public int CHECKPKPOINT()
        {
            var player = lua["player"] as PlayerObject;
            return player.PKPoints;
        }
        public int LEVEL()
        {
            var player = lua["player"] as PlayerObject;
            return player.Level;
        }
        public bool INGUILD(string GuildName = "")
        {
            var player = lua["player"] as PlayerObject;
            var failed = true;
            if (GuildName != "")
            {
                failed = player.MyGuild == null || player.MyGuild.Name != GuildName;
            }

            failed = player.MyGuild == null;
            return !failed;
        }
        public void ADDTOGUILD(string GuildName)
        {
            var player = lua["player"] as PlayerObject;

            if (player.MyGuild != null) return;

            GuildObject guild = Envir.GetGuild(GuildName);

            if (guild == null) return;

            player.PendingGuildInvite = guild;
            player.GuildInvite(true);
        }

        public bool CHECKQUEST(int questID, string status)
        {
            var player = lua["player"] as PlayerObject;
            var failed = false;
            string tempString = status.ToUpper();
            if (tempString == "ACTIVE")
            {
                failed = !player.CurrentQuests.Any(e => e.Index == questID);
            }
            else //COMPLETE
            {
                failed = !player.CompletedQuests.Contains(questID);
            }
            return !failed;
        }
        public void SETQUEST(int questID, string status)
        {
            var player = lua["player"] as PlayerObject;
            int.TryParse(status, out int questState);

            if (questID < 1) return;

            var activeQuest = player.CurrentQuests.FirstOrDefault(e => e.Index == questID);

            //remove from active list
            if (activeQuest != null)
            {
                player.SendUpdateQuest(activeQuest, QuestState.Remove);
            }

            switch (questState)
            {
                case 0: //cancel
                    if (player.CompletedQuests.Contains(questID))
                    {
                        player.CompletedQuests.Remove(questID);
                    }
                    break;
                case 1: //complete
                    if (!player.CompletedQuests.Contains(questID))
                    {
                        player.CompletedQuests.Add(questID);
                    }
                    break;
            }

            player.GetCompletedQuests();
        }
        public int CHECKACCOUNTLIST(string relativePath, string key = "")
        {
            // Construct the full path to the .txt file
            string fullPath = Path.Combine("Envir", "Namelists", relativePath + ".txt");
            int count = 0;
            // Check if the file exists
            if (File.Exists(fullPath))
            {
                // Read all lines from the file
                string[] lines = File.ReadAllLines(fullPath);

                // Check if the file is empty and display "Empty" if so, otherwise display each line as a new item
                if (lines.Length == 0)
                {
                    return count;
                }
                else
                {
                    foreach (string line in lines)
                    {
                        key = key == "" ? USERNAME() : key;
                        if (line == key)
                        {
                            count++;
                        }
                    }
                    return count;
                }
            }
            else
            {
                // Display a message if the file is not found
                return count;
            }
        }
        public void DELACCOUNTLIST(string relativePath, string key = "")
        {
            string playerToDelete = key == "" ? USERNAME() : key;
            string fullPath = Path.Combine("Envir", "Namelists", relativePath + ".txt");

            var lines = File.ReadAllLines(fullPath).ToList();
            foreach (string line in lines)
            {
                lines.Remove(playerToDelete);
            }

        }
        public bool HASBUFF(int bufftype) {
            var player = lua["player"] as PlayerObject;
            return player.HasBuff((BuffType)bufftype);
        }
    }
}