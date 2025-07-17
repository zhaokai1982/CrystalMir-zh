using Server.MirObjects;
using Server.MirObjects.Monsters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server.Systems
{
    public partial class GuildItemForm : Form
    {
        public GuildObject Guild { get; set; }
        public string GuildName;
        public SMain main;

        public GuildItemForm()
        {
            InitializeComponent();
            this.Load += GuildItemForm_Load;
        }

        #region 加载公会公告
        public void SetGuildNotice(List<string> notice)
        {
            GuildNoticeBox.Text = string.Join(Environment.NewLine, notice);
        }
        #endregion

        #region Load Member Count
        public void SetMemberCount(int memberCount, int memberCap)
        {
            MemberCountLabel.Text = $"成员数量: {memberCount}/{memberCap}";
        }
        #endregion

        #region Load Guild Points
        public void SetGuildPoints(byte sparePoints)
        {
            GuildPointsLabel.Text = $"公会点数: {sparePoints}";
        }
        #endregion

        #region 加载公会经验值
        public void SetGuildExperience(long experience)
        {
            GuildEXPLabel.Text = $"公会经验: {experience}";
        }
        #endregion

        #region 加载公会等级
        public void SetGuildRanks(List<GuildRank> ranks)
        {
            GuildRanksListView.Items.Clear();

            foreach (var rank in ranks)
            {
                ListViewItem item = new ListViewItem(rank.Name);
                GuildRanksListView.Items.Add(item);
            }
        }
        #endregion

        #region 加载公会聊天
        public void LoadGuildChat()
        {
            if (main == null || Guild == null) return;

            GuildChatBox.Clear();

            string[] chatLogLines = main.ChatLogTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            foreach (var line in chatLogLines)
            {
                if (line.Contains($"由系统发送给公会: '{GuildName}':"))
                {
                    GuildChatBox.AppendText(line + Environment.NewLine);
                    continue;
                }

                int guildMessageIndex = line.IndexOf(": !~");
                if (guildMessageIndex > -1)
                {
                    int playerNameStart = line.IndexOf("]: ") + 3;
                    int playerNameEnd = line.IndexOf(":", playerNameStart);

                    if (playerNameStart > 0 && playerNameEnd > playerNameStart)
                    {
                        string playerName = line.Substring(playerNameStart, playerNameEnd - playerNameStart).Trim();

                        if (Guild.Ranks.Any(rank => rank.Members.Any(member => member.Name == playerName)))
                        {
                            GuildChatBox.AppendText(line + Environment.NewLine);
                        }
                    }
                }
            }
        }
        #endregion

        #region 增益列表
        public void SetBuffList(List<GuildBuff> activeBuffs, List<GuildBuffInfo> allBuffInfos)
        {
            BuffListView.Items.Clear();

            // 用于快速检查增益效果是否生效的字典
            var activeBuffsById = activeBuffs.ToDictionary(buff => buff.Id);

            foreach (var buffInfo in allBuffInfos)
            {
                ListViewItem item = new ListViewItem(buffInfo.Id.ToString());

                // 显示增益效果的名称
                item.SubItems.Add(buffInfo.Name);

                // 检查这个增益效果是否处于激活状态
                if (activeBuffsById.TryGetValue(buffInfo.Id, out GuildBuff activeBuff))
                {
                    // 增益效果已激活
                    item.SubItems.Add("激活特效");
                    item.SubItems.Add(activeBuff.ActiveTimeRemaining.ToString());
                }
                else
                {
                    // 增益效果未激活
                    item.SubItems.Add("尚未激活");
                    item.SubItems.Add("0"); // 非活动增益效果剩余时间为零
                }

                BuffListView.Items.Add(item);
            }
        }
        #endregion

        #region 删除按钮
        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (MemberListView == null) return;
            if (MemberListView.SelectedItems == null) return;

            Server.MirObjects.GuildObject Guild = SMain.Envir.GetGuild(GuildName);
            if (Guild == null) return;

            foreach (var m in MemberListView.SelectedItems)
            {
                var lm = (ListViewItem)m;

                Guild.DeleteMember(lm.SubItems[0].Text);
                MemberListView.Items.Remove(lm);
                main.ProcessGuildViewTab();
                break;
            }
        }
        #endregion

        #region 更新公会公告
        private void RefreshNoticeButton_Click(object sender, EventArgs e)
        {
            var guild = SMain.Envir.GetGuild(GuildName);
            if (guild == null) return;

            List<string> newNotice = GuildNoticeBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();

            // 记录公会通知更新情况
            string noticeUpdateLog = $"公会: '{GuildName}' 的公告由系统修改";
            SMain.EnqueueChat(noticeUpdateLog);

            Logger.GetLogger(LogType.Server).Info(noticeUpdateLog);

            guild.NewNotice(newNotice);

            SetGuildNotice(guild.Info.Notice);
        }
        #endregion

        #region 发送公会消息
        private void SendGuildMessageButton_Click(object sender, EventArgs e)
        {
            var guild = SMain.Envir.GetGuild(GuildName);
            if (guild == null) return;

            string message = SendGuildMesageBox.Text.Trim();

            if (string.IsNullOrEmpty(message)) return;

            guild.SendMessage($"系统信息: {message}", ChatType.Guild);

            string timestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            GuildChatBox.AppendText($"[{timestamp}]: 系统信息: {message}" + Environment.NewLine);

            string logMessage = $"由系统发送给公会: '{GuildName}': {message}";
            SMain.EnqueueChat(logMessage);

            SendGuildMesageBox.Clear();
        }
        #endregion

        #region 加载表单
        private void GuildItemForm_Load(object sender, EventArgs e)
        {
            LoadGuildChat();
        }
        #endregion
    }
}
