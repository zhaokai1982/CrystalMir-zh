﻿using CustomFormControl;
using Server.Account;
using Server.Database;
using Server.MirDatabase;
using Server.MirEnvir;
using Server.MirForms.Systems;
using Server.MirObjects;
using Server.Systems;

namespace Server
{
    public partial class SMain : Form
    {
        public static Envir Envir => Envir.Main;

        public static Envir EditEnvir => Envir.Edit;

        protected static MessageQueue MessageQueue => MessageQueue.Instance;

        public SMain()
        {
            InitializeComponent();

            AutoResize();

            // 绑定双击事件
            LogTextBox.DoubleClick += LogTextBox_DoubleClick;
            DebugLogTextBox.DoubleClick += DebugLogTextBox_DoubleClick;
            ChatLogTextBox.DoubleClick += ChatLogTextBox_DoubleClick;
        }

        private void AutoResize()
        {
            int columnCount = PlayersOnlineListView.Columns.Count;

            foreach (ColumnHeader column in PlayersOnlineListView.Columns)
            {
                column.Width = PlayersOnlineListView.Width / (columnCount - 1) - 1;
            }

            indexHeader.Width = 2;
        }

        public static void Enqueue(Exception ex)
        {
            MessageQueue.Enqueue(ex);
        }

        public static void EnqueueDebugging(string msg)
        {
            MessageQueue.EnqueueDebugging(msg);
        }

        public static void EnqueueChat(string msg)
        {
            MessageQueue.EnqueueChat(msg);
        }

        public static void Enqueue(string msg)
        {
            MessageQueue.Enqueue(msg);
        }

        private void configToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void InterfaceTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                Text = $"总计: {Envir.LastCount}, 实计: {Envir.LastRealCount}";
                PlayersLabel.Text = $"在线角色: {Envir.Players.Count}";
                MonsterLabel.Text = $"刷新怪物: {Envir.MonsterCount}";
                ConnectionsLabel.Text = $"连线登录: {Envir.Connections.Count}";
                BlockedIPsLabel.Text = $"屏蔽锁定 IPs: {Envir.IPBlocks.Count(x => x.Value > Envir.Now)}";
                UpTimeLabel.Text = $"运行时间: {Envir.Stopwatch.ElapsedMilliseconds / 1000 / 60 / 60 / 24}日:{Envir.Stopwatch.ElapsedMilliseconds / 1000 / 60 / 60 % 24}小时:{Envir.Stopwatch.ElapsedMilliseconds / 1000 / 60 % 60}分钟:{Envir.Stopwatch.ElapsedMilliseconds / 1000 % 60}秒";

                if (Settings.Multithreaded && (Envir.MobThreads != null))
                {
                    CycleDelayLabel.Text = $"延迟周期: {Envir.LastRunTime:0000}";
                    for (int i = 0; i < Envir.MobThreads.Length; i++)
                    {
                        if (Envir.MobThreads[i] == null) break;
                        CycleDelayLabel.Text = CycleDelayLabel.Text + $"|{Envir.MobThreads[i].LastRunTime:0000}";

                    }
                }
                else
                    CycleDelayLabel.Text = $"延迟周期: {Envir.LastRunTime}";

                while (!MessageQueue.MessageLog.IsEmpty)
                {
                    string message;

                    if (!MessageQueue.MessageLog.TryDequeue(out message)) continue;

                    LogTextBox.AppendText(message);
                }

                while (!MessageQueue.DebugLog.IsEmpty)
                {
                    string message;

                    if (!MessageQueue.DebugLog.TryDequeue(out message)) continue;

                    DebugLogTextBox.AppendText(message);
                }

                while (!MessageQueue.ChatLog.IsEmpty)
                {
                    string message;

                    if (!MessageQueue.ChatLog.TryDequeue(out message)) continue;

                    ChatLogTextBox.AppendText(message);
                }

                ProcessPlayersOnlineTab(false);
                ProcessGuildViewTab(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private ListViewItem CreateListView(CharacterInfo character)
        {
            ListViewItem ListItem = new ListViewItem(character.Index.ToString()) { Tag = character };

            ListItem.SubItems.Add(character.Name);
            ListItem.SubItems.Add(character.Level.ToString());
            ListItem.SubItems.Add(character.Class.ToString());
            ListItem.SubItems.Add(character.Gender.ToString());

            return ListItem;
        }

        private void ProcessPlayersOnlineTab(bool forced = false)
        {
            if (PlayersOnlineListView.Items.Count != Envir.Players.Count || forced == true)
            {
                PlayersOnlineListView.Items.Clear();

                for (int i = PlayersOnlineListView.Items.Count; i < Envir.Players.Count; i++)
                {
                    CharacterInfo character = Envir.Players[i].Info;

                    ListViewItem tempItem = CreateListView(character);

                    PlayersOnlineListView.Items.Add(tempItem);
                }
            }
        }

        private void startServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Envir.Start();
        }

        private void stopServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Envir.Stop();
            Envir.MonsterCount = 0;
        }

        private void SMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 弹出确认对话框
            DialogResult result = MessageBox.Show("确定要关闭服务器吗？", "确认关闭", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            // 如果用户选择“否”，则取消关闭操作
            if (result == DialogResult.No)
            {
                e.Cancel = true;
            }
            else
            {
                // 如果用户选择“是”，则停止服务器
                Envir.Stop();
            }
        }

        private void closeServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void itemInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ItemInfoForm form = new ItemInfoForm();

            form.Show();
        }

        private void monsterInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MonsterInfoForm form = new MonsterInfoForm();

            form.Show();
        }

        private void nPCInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NPCInfoForm form = new NPCInfoForm();

            form.Show();
        }

        private void balanceConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BalanceConfigForm form = new BalanceConfigForm();

            form.Show();
        }

        private void questInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            QuestInfoForm form = new QuestInfoForm();

            form.Show();
        }

        private void serverToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConfigForm form = new ConfigForm();

            form.Show();
        }

        private void balanceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BalanceConfigForm form = new BalanceConfigForm();

            form.Show();
        }

        private void accountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AccountInfoForm form = new AccountInfoForm();

            form.Show();
        }

        private void mapInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MapInfoForm form = new MapInfoForm();

            form.Show();
        }

        private void itemInfoToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            ItemInfoForm form = new ItemInfoForm();

            form.Show();
        }

        private void monsterInfoToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            MonsterInfoForm form = new MonsterInfoForm();

            form.Show();
        }

        private void nPCInfoToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            NPCInfoForm form = new NPCInfoForm();

            form.Show();
        }

        private void questInfoToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            QuestInfoForm form = new QuestInfoForm();

            form.Show();
        }

        private void dragonSystemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DragonInfoForm form = new DragonInfoForm();

            form.Show();
        }

        private void miningToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MiningInfoForm form = new MiningInfoForm();

            form.Show();
        }

        private void guildsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GuildInfoForm form = new GuildInfoForm();

            form.Show();
        }

        private void fishingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SystemInfoForm form = new SystemInfoForm(0);

            form.Show();
        }

        private void GlobalMessageButton_Click(object sender, EventArgs e)
        {
            if (GlobalMessageTextBox.Text.Length < 1) return;

            foreach (var player in Envir.Players)
            {
                player.ReceiveChat(GlobalMessageTextBox.Text, ChatType.Announcement);
            }

            EnqueueChat(GlobalMessageTextBox.Text);
            GlobalMessageTextBox.Text = string.Empty;
        }

        private void PlayersOnlineListView_DoubleClick(object sender, EventArgs e)
        {
            CustomFormControl.ListViewNF list = (CustomFormControl.ListViewNF)sender;

            if (list.SelectedItems.Count > 0)
            {
                ListViewItem item = list.SelectedItems[0];
                string index = item.SubItems[0].Text;

                PlayerInfoForm form = new PlayerInfoForm(Convert.ToUInt32(index));

                form.Show();
            }
        }

        private void PlayersOnlineListView_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            e.Cancel = true;
            e.NewWidth = PlayersOnlineListView.Columns[e.ColumnIndex].Width;
        }

        private void mailToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SystemInfoForm form = new SystemInfoForm(1);

            form.Show();
        }

        private void goodsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SystemInfoForm form = new SystemInfoForm(2);

            form.Show();
        }

        private void relationshipToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SystemInfoForm form = new SystemInfoForm(4);

            form.Show();
        }

        private void refiningToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SystemInfoForm form = new SystemInfoForm(3);

            form.ShowDialog();
        }

        private void mentorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SystemInfoForm form = new SystemInfoForm(5);

            form.Show();
        }

        private void magicInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MagicInfoForm form = new MagicInfoForm();
            form.Show();
        }

        private void SMain_Load(object sender, EventArgs e)
        {
            var loaded = EditEnvir.LoadDB();

            if (loaded)
            {
                Envir.Start();
            }

            AutoResize();
        }

        private void gemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SystemInfoForm form = new SystemInfoForm(6);

            form.Show();
        }

        private void conquestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConquestInfoForm form = new ConquestInfoForm();

            form.Show();
        }

        private void rebootServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Envir.Reboot();
        }

        private void respawnsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SystemInfoForm form = new SystemInfoForm(7);

            form.Show();
        }

        private void monsterTunerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!SMain.Envir.Running)
            {
                MessageBox.Show("服务器须在运行状态才能调整怪物", "警告",
                MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }

            MonsterTunerForm form = new MonsterTunerForm();

            form.Show();
        }

        private void gameshopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GameShop form = new GameShop();
            form.Show();
        }

        private void itemNEWToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ItemInfoFormNew form = new ItemInfoFormNew();

            form.Show();
        }

        private void monsterExperimentalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MonsterInfoFormNew form = new MonsterInfoFormNew();

            form.Show();
        }

        private void dropBuilderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MirForms.DropBuilder.DropGenForm GenForm = new MirForms.DropBuilder.DropGenForm();

            GenForm.Show();
        }

        private void clearBlockedIPsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Envir.IPBlocks.Clear();
        }

        private void nPCsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Envir.ReloadNPCs();
        }

        private void dropsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Envir.ReloadDrops();
        }

        private void lineMessageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Envir.ReloadLineMessages();
        }

        #region 公会视图选项卡
        public void ProcessGuildViewTab(bool forced = false)
        {
            if (GuildListView.Items.Count != Envir.GuildList.Count || forced == true)
            {
                GuildListView.Items.Clear();

                foreach (GuildInfo guild in Envir.GuildList)
                {
                    ListViewItem tempItem = new ListViewItem(guild.GuildIndex.ToString()) { Tag = this };

                    tempItem.SubItems.Add(guild.Name);

                    if (guild.Ranks.Count > 0 && guild.Ranks[0].Members.Count > 0)
                    {
                        tempItem.SubItems.Add(guild.Ranks[0].Members[0].Name);
                    }
                    else
                    {
                        tempItem.SubItems.Add("DELETED");
                        tempItem.ForeColor = Color.Red;
                    }

                    tempItem.SubItems.Add($"{guild.Membercount}/{guild.MemberCap}");
                    tempItem.SubItems.Add(guild.Level.ToString());
                    tempItem.SubItems.Add($"{guild.Gold}");

                    GuildListView.Items.Add(tempItem);
                }
            }
        }

        private void GuildListView_DoubleClick(object sender, EventArgs e)
        {
            ListViewNF list = (ListViewNF)sender;

            if (list.SelectedItems.Count <= 0) return;

            ListViewItem item = list.SelectedItems[0];
            int index = Int32.Parse(item.Text);

            GuildObject Guild = Envir.GetGuild(index);
            GuildItemForm form = new GuildItemForm
            {
                GuildName = Guild.Name,
                Guild = Guild,
                main = this,
            };

            form.SetMemberCount(Guild.Info.Membercount, Guild.Info.MemberCap);
            form.SetGuildNotice(Guild.Info.Notice);
            form.SetBuffList(Guild.Info.BuffList, Settings.Guild_BuffList);
            form.SetGuildPoints(Guild.Info.SparePoints);
            form.SetGuildExperience(Guild.Info.Experience);

            if (Guild == null) return;

            foreach (var i in Guild.StoredItems)
            {
                if (i == null) continue;
                ListViewItem tempItem = new ListViewItem(i.Item.UniqueID.ToString()) { Tag = this };

                CharacterInfo character = Envir.GetCharacterInfo((int)i.UserId);
                if (character != null)
                    tempItem.SubItems.Add(character.Name);
                else if (i.UserId == -1)
                    tempItem.SubItems.Add("Server");
                else
                    tempItem.SubItems.Add("Unknown");

                tempItem.SubItems.Add(i.Item.FriendlyName);
                tempItem.SubItems.Add(i.Item.Count.ToString());
                tempItem.SubItems.Add(i.Item.CurrentDura + "/" + i.Item.MaxDura);

                form.GuildItemListView.Items.Add(tempItem);
            }

            foreach (var r in Guild.Ranks)
                foreach (var m in r.Members)
                {
                    ListViewItem tempItem = new ListViewItem(m.Name) { Tag = this };
                    tempItem.SubItems.Add(r.Name);
                    form.MemberListView.Items.Add(tempItem);
                }
            form.SetGuildRanks(Guild.Ranks);

            form.ShowDialog();
        }
        #endregion

        private void MainTabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            ProcessPlayersOnlineTab(true);
            ProcessGuildViewTab(true);
        }

        private void GuildListView_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void heroesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SystemInfoForm form = new SystemInfoForm(8);

            form.Show();
        }

        private void CharacterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CharacterInfoForm form = new CharacterInfoForm();

            form.Show();
        }

        private void recipeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RecipeInfoForm form = new RecipeInfoForm();

            form.Show();
        }

        private void accountsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            AccountInfoForm form = new AccountInfoForm();

            form.Show();
        }

        private void marketToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Market form = new Market();

            form.Show();
        }

        private void namelistsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Namelists form = new Namelists();

            form.Show();
        }

        #region Monsters Tab
        private void LoadMonstersButton_Click(object sender, EventArgs e)
        {
            MonsterListView.Items.Clear();
            for (int i = 0; i < Envir.MapList.Count; i++)
            {
                var map = Envir.MapList[i];
                ListViewItem ListItem = new ListViewItem(i.ToString()) { Tag = this };

                ListItem.SubItems.Add(map.Info.Title);
                ListItem.SubItems.Add(map.Info.FileName);
                ListItem.SubItems.Add(map.GetAllMonstersObjectsCount().ToString());
                int totalSpawnsCount = 0;
                int errorCount = 0;

                foreach (var spawn in map.Respawns)
                {
                    totalSpawnsCount += spawn.Info.Count;
                    errorCount += spawn.ErrorCount;
                }

                ListItem.SubItems.Add(totalSpawnsCount.ToString());
                ListItem.SubItems.Add(errorCount.ToString());

                MonsterListView.Items.Add(ListItem);
            }
        }
        #endregion

        private void 物品信息ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Envir.ReloadItems();
        }

        private void 怪物信息ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Envir.ReloadMonsters();
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Envir.ReloadMagics();
        }

        private void 任务信息ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Envir.ReloadQuests();
        }

        private void 配方信息ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Envir.ReloadCrafts();
        }

        private void 商城物品ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Envir.ReloadGameShop();
        }

        private void 触发脚本ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Envir.ReloadLua();
        }

        private void MonsterListView_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void UpTimeLabel_Click(object sender, EventArgs e)
        {

        }
        // 日志窗口双击清空内容（带确认）
        private void LogTextBox_DoubleClick(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定要清空日志内容吗？", "确认清空", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                LogTextBox.Clear();
            }
        }

        // 调试窗口双击清空内容（带确认）
        private void DebugLogTextBox_DoubleClick(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定要清空调试内容吗？", "确认清空", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                DebugLogTextBox.Clear();
            }
        }

        // 聊天窗口双击清空内容（带确认）
        private void ChatLogTextBox_DoubleClick(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定要清空聊天内容吗？", "确认清空", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                ChatLogTextBox.Clear();
            }
        }
    }
}