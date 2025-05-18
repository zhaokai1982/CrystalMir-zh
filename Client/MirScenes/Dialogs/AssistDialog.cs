// 引入客户端控件相关命名空间
using Client.MirControls;
// 引入客户端图形相关命名空间
using Client.MirGraphics;
// 引入客户端网络相关命名空间
using Client.MirNetwork;
// 引入客户端对象相关命名空间
using Client.MirObjects;
// 引入客户端声音相关命名空间
using Client.MirSounds;
// 引入通用系统命名空间
using System;
// 引入集合相关命名空间
using System.Collections.Generic;
// 引入绘图相关命名空间
using System.Drawing;
// 引入 LINQ 查询相关命名空间
using System.Linq;
// 引入字符串处理相关命名空间
using System.Text;
// 引入异步任务相关命名空间
using System.Threading.Tasks;
// 引入 Windows 窗体相关命名空间
using System.Windows.Forms;

// 为客户端数据包命名空间设置别名
using C = ClientPackets;

namespace Client.MirScenes.Dialogs
{
    /// <summary>
    /// 辅助设置对话框类，继承自 MirImageControl，用于显示和处理各种辅助设置选项。
    /// </summary>
    public sealed class AssistDialog : MirImageControl
    {
        // 标签页按钮数组，用于切换不同的设置页面
        public MirCheckBox[] TabPageButton;
        // 标签页名称数组，定义了各个标签页的显示名称
        public static string[] Pages = { "基本设置", "职业设置", "保护设置" };

        // 基本设置标签页索引
        public static int BASE = 0;
        // 职业设置标签页索引
        public static int CLASS = 1;
        // 保护设置标签页索引
        public static int PROTECT = 2;
        // 物品设置标签页索引
        public static int ITEM = 3;

        // 物品过滤列表每页显示的数量
        public static int PageSize = 10;

        // 关闭对话框按钮
        public MirButton CloseButton;
        // 以下是各种复选框，用于不同的设置选项
        public MirCheckBox CheckBoxFreeShift, CheckBoxSmartFire, CheckBoxShowGroupInfo, CheckBoxHideDead, CheckBoxShowMonsterName, CheckBoxShowNPCName;
        public MirCheckBox CheckBoxSmartDaMo, CheckBoxSmartYiJinJin;
        public MirCheckBox CheckBoxJinGang;
        public MirCheckBox CheckBoxSmartSheild, CheckBoxAutoPick, CheckBoxChangePoison, CheckBoxSpaceThrusting;
        public MirCheckBox CheckBoxShowLevel, CheckBoxShowTransform, CheckBoxShowGuildName;
        public MirCheckBox CheckBoxShowPing, CheckBoxShowHealth, CheckBoxShowDamage, CheckBoxShowHeal, CheckBoxHideSystem2;

        // 开启保护复选框
        public MirCheckBox CheckBoxProtect;
        // 保护设置相关的标签数组
        public MirLabel[] LabelProtect;
        public MirLabel[] LabelUse;
        public MirLabel[] LabelTip;
        // 保护设置相关的文本框数组
        public MirTextBox[] TextBoxProtectPercent;
        public MirTextBox[] TextBoxProtectItem;
        // 法术输入文本框
        public MirTextBox TextBoxSpell;

        // 物品过滤复选框数组
        public MirCheckBox[] CheckBoxItemFilter;
        // 物品过滤列表上一页按钮
        public MirButton PreviousButton;
        // 物品过滤列表下一页按钮
        public MirButton NextButton;
        // 物品过滤列表当前页码标签
        public MirLabel PageNumberLabel;

        // 当前显示的物品过滤列表页码
        private int Page = 0;
        // 物品过滤列表的起始索引
        private int StartIndex = 0;
        /// <summary>
        /// 获取物品过滤列表的最大页码
        /// </summary>
        private int maxPage
        {
            get
            {
                // 根据物品过滤列表的数量和每页显示数量计算最大页码
                return (int)Math.Ceiling((double)GameScene.Scene.AssistHelper.ItemFilterList.Count / PageSize);
            }
        }


        /// <summary>
        /// 辅助设置对话框的构造函数，初始化各种控件和事件
        /// </summary>
        public AssistDialog()
        {
            // 设置对话框的图标索引
            Index = 33;
            // 设置对话框的图标库
            Library = Libraries.Prguse3;

            // 允许对话框移动
            Movable = true;
            // 允许对话框排序
            Sort = true;
            // 将对话框居中显示
            Location = Center;


            // 初始化标签页按钮数组
            TabPageButton = new MirCheckBox[Pages.Length];
            for (int i = 0; i < TabPageButton.Length; ++i)
            {
                int j = i;
                TabPageButton[i] = new MirCheckBox
                {
                    // 设置按钮的图标索引
                    Index = 13 + i * 2,
                    // 设置按钮未选中时的图标索引
                    UnTickedIndex = 13 + i * 2,
                    // 设置按钮选中时的图标索引
                    TickedIndex = 14 + i * 2,
                    // 设置按钮的父控件为当前对话框
                    Parent = this,
                    // 设置按钮的位置
                    Location = new Point(22 + i * 94, 14),
                    // 设置按钮的图标库
                    Library = Libraries.Prguse3
                };
                // 为按钮的点击事件绑定切换标签页的方法
                TabPageButton[i].Click += (o, e) => SwitchTab(j);
            }


            #region 基本
            // 初始化免 shift 复选框
            CheckBoxFreeShift = new MirCheckBox { Index = 2086, UnTickedIndex = 2086, TickedIndex = 2087, Parent = this, Location = new Point(26, 70), Library = Libraries.Prguse };
            // 设置复选框的选中状态为配置中的值
            CheckBoxFreeShift.Checked = Settings.FreeShift;
            // 设置复选框的标签文本
            CheckBoxFreeShift.LabelText = "免shift";
            // 为复选框的点击事件绑定处理方法
            CheckBoxFreeShift.Click += CheckBoxFreeShiftClick;

            // 初始化显示等级复选框
            CheckBoxShowLevel = new MirCheckBox { Index = 2086, UnTickedIndex = 2086, TickedIndex = 2087, Parent = this, Location = new Point(26, 95), Library = Libraries.Prguse };
            CheckBoxShowLevel.LabelText = "显示等级";
            CheckBoxShowLevel.Checked = Settings.ShowLevel;
            CheckBoxShowLevel.Click += CheckBoxShowLevelClick;

            // 初始化显示时装复选框
            CheckBoxShowTransform = new MirCheckBox { Index = 2086, UnTickedIndex = 2086, TickedIndex = 2087, Parent = this, Location = new Point(26, 120), Library = Libraries.Prguse };
            CheckBoxShowTransform.LabelText = "显示时装";
            CheckBoxShowTransform.Checked = Settings.ShowTransform;
            CheckBoxShowTransform.Click += CheckBoxShowTransformClick;

            // 初始化显示公会名复选框
            CheckBoxShowGuildName = new MirCheckBox { Index = 2086, UnTickedIndex = 2086, TickedIndex = 2087, Parent = this, Location = new Point(150, 70), Library = Libraries.Prguse };
            CheckBoxShowGuildName.LabelText = "显示公会名";
            CheckBoxShowGuildName.Checked = Settings.ShowGuildName;
            // 为复选框的点击事件绑定匿名处理方法，切换配置中的显示公会名状态
            CheckBoxShowGuildName.Click += (o, e) =>
            {
                Settings.ShowGuildName = !Settings.ShowGuildName;
            };

            // 初始化显示组队信息复选框
            CheckBoxShowGroupInfo = new MirCheckBox { Index = 2086, UnTickedIndex = 2086, TickedIndex = 2087, Parent = this, Location = new Point(150, 95), Library = Libraries.Prguse };
            CheckBoxShowGroupInfo.LabelText = "显示组队信息";
            CheckBoxShowGroupInfo.Checked = Settings.ShowGroupInfo;
            CheckBoxShowGroupInfo.Click += (o, e) =>
            {
                Settings.ShowGroupInfo = !Settings.ShowGroupInfo;
            };

            // 初始化显示伤害复选框
            CheckBoxShowDamage = new MirCheckBox { Index = 2086, UnTickedIndex = 2086, TickedIndex = 2087, Parent = this, Location = new Point(150, 120), Library = Libraries.Prguse };
            CheckBoxShowDamage.LabelText = "显示伤害";
            CheckBoxShowDamage.Checked = Settings.DisplayDamage;
            CheckBoxShowDamage.Click += (o, e) =>
            {
                Settings.DisplayDamage = !Settings.DisplayDamage;
            };

            // 初始化显示恢复复选框
            CheckBoxShowHeal = new MirCheckBox { Index = 2086, UnTickedIndex = 2086, TickedIndex = 2087, Parent = this, Location = new Point(150, 145), Library = Libraries.Prguse };
            CheckBoxShowHeal.LabelText = "显示恢复";
            CheckBoxShowHeal.Checked = Settings.ShowHeal;
            CheckBoxShowHeal.Click += (o, e) =>
            {
                Settings.ShowHeal = !Settings.ShowHeal;
            };


            // 初始化隐藏尸体复选框
            CheckBoxHideDead = new MirCheckBox { Index = 2086, UnTickedIndex = 2086, TickedIndex = 2087, Parent = this, Location = new Point(150, 170), Library = Libraries.Prguse };
            CheckBoxHideDead.LabelText = "隐藏尸体";
            CheckBoxHideDead.Checked = Settings.HideDead;
            CheckBoxHideDead.Click += (o, e) =>
            {
                Settings.HideDead = !Settings.HideDead;
            };

            // 初始化怪物显名复选框
            CheckBoxShowMonsterName = new MirCheckBox { Index = 2086, UnTickedIndex = 2086, TickedIndex = 2087, Parent = this, Location = new Point(282, 70), Library = Libraries.Prguse };
            CheckBoxShowMonsterName.LabelText = "怪物显名";
            CheckBoxShowMonsterName.Checked = Settings.ShowMonsterName;
            CheckBoxShowMonsterName.Click += (o, e) =>
            {
                Settings.ShowMonsterName = !Settings.ShowMonsterName;
            };

            // 初始化隐藏掉落通知复选框
            CheckBoxHideSystem2 = new MirCheckBox { Index = 2086, UnTickedIndex = 2086, TickedIndex = 2087, Parent = this, Location = new Point(282, 95), Library = Libraries.Prguse };
            CheckBoxHideSystem2.LabelText = "隐藏掉落通知";
            CheckBoxHideSystem2.Checked = Settings.HideSystem2;
            CheckBoxHideSystem2.Click += (o, e) =>
            {
                Settings.HideSystem2 = !Settings.HideSystem2;
            };

            // 初始化 NPC 显名复选框
            CheckBoxShowNPCName = new MirCheckBox { Index = 2086, UnTickedIndex = 2086, TickedIndex = 2087, Parent = this, Location = new Point(282, 120), Library = Libraries.Prguse };
            CheckBoxShowNPCName.LabelText = "NPC显名";
            CheckBoxShowNPCName.Checked = Settings.ShowNPCName;
            CheckBoxShowNPCName.Click += (o, e) =>
            {
                Settings.ShowNPCName = !Settings.ShowNPCName;
            };

            // 初始化显示 Ping 复选框
            CheckBoxShowPing = new MirCheckBox { Index = 2086, UnTickedIndex = 2086, TickedIndex = 2087, Parent = this, Location = new Point(26, 145), Library = Libraries.Prguse };
            CheckBoxShowPing.LabelText = "显示Ping";
            CheckBoxShowPing.Checked = Settings.ShowPing;
            CheckBoxShowPing.Click += (o, e) =>
            {
                Settings.ShowPing = CheckBoxShowPing.Checked;
            };

            // 初始化显示血量复选框
            CheckBoxShowHealth = new MirCheckBox { Index = 2086, UnTickedIndex = 2086, TickedIndex = 2087, Parent = this, Location = new Point(26, 170), Library = Libraries.Prguse };
            CheckBoxShowHealth.LabelText = "显示血量";
            CheckBoxShowHealth.Checked = Settings.ShowHealth;
            CheckBoxShowHealth.Click += (o, e) => { Settings.ShowHealth = CheckBoxShowHealth.Checked; };

            #endregion

            #region 职业

            // 初始化自动烈火复选框
            CheckBoxSmartFire = new MirCheckBox { Index = 2086, UnTickedIndex = 2086, TickedIndex = 2087, Parent = this, Location = new Point(26, 70), Library = Libraries.Prguse };
            CheckBoxSmartFire.LabelText = "自动烈火";
            CheckBoxSmartFire.Checked = Settings.SmartFireHit;
            CheckBoxSmartFire.Click += CheckBoxFireClick;

            // 初始化自动双龙复选框
            CheckBoxSpaceThrusting = new MirCheckBox { Index = 2086, UnTickedIndex = 2086, TickedIndex = 2087, Parent = this, Location = new Point(26, 95), Library = Libraries.Prguse };
            CheckBoxSpaceThrusting.LabelText = "自动双龙";
            CheckBoxSpaceThrusting.Checked = Settings.自动双龙斩;
            CheckBoxSpaceThrusting.Click += CheckBoxSpaceThrustingClick;

            // 初始化自动开盾复选框
            CheckBoxSmartSheild = new MirCheckBox { Index = 2086, UnTickedIndex = 2086, TickedIndex = 2087, Parent = this, Location = new Point(150, 70), Library = Libraries.Prguse };
            CheckBoxSmartSheild.LabelText = "自动开盾";
            CheckBoxSmartSheild.Checked = Settings.SmartSheild;
            CheckBoxSmartSheild.Click += CheckBoxSheildClick;

            // 初始化自动毒符复选框
            CheckBoxChangePoison = new MirCheckBox { Index = 2086, UnTickedIndex = 2086, TickedIndex = 2087, Parent = this, Location = new Point(301, 70), Library = Libraries.Prguse };
            CheckBoxChangePoison.LabelText = "自动毒符";
            CheckBoxChangePoison.Checked = Settings.SmartChangePoison;
            CheckBoxChangePoison.Click += CheckBoxChangePoisonClick;

            // 初始化自动金刚术复选框
            CheckBoxJinGang = new MirCheckBox { Index = 2086, UnTickedIndex = 2086, TickedIndex = 2087, Parent = this, Location = new Point(301, 145), Library = Libraries.Prguse };
            CheckBoxJinGang.LabelText = "自动金刚术";
            CheckBoxJinGang.Checked = Settings.SmartElementalBarrier;
            CheckBoxJinGang.Click += (o, e) => Settings.SmartElementalBarrier = CheckBoxJinGang.Checked;
            #endregion

            #region 保护
            // 初始化开启保护复选框
            CheckBoxProtect = new MirCheckBox { Index = 2086, UnTickedIndex = 2086, TickedIndex = 2087, Parent = this, Location = new Point(26, 70), Library = Libraries.Prguse };
            CheckBoxProtect.LabelText = "开启保护";
            CheckBoxProtect.Checked = Settings.开启保护;
            CheckBoxProtect.Click += CheckBoxProtectClick;

            // 初始化保护设置相关的标签和文本框数组
            LabelProtect = new MirLabel[3];
            LabelUse = new MirLabel[3];
            TextBoxProtectPercent = new MirTextBox[3];
            TextBoxProtectItem = new MirTextBox[3];
            TextBoxSpell = new MirTextBox();
            LabelTip = new MirLabel[3];

            // 创建保护设置控件
            CreateProtectControls(0, 0);
            CreateProtectControls(1, 1);
            CreateProtectControls(2, 0);
            #endregion

            #region 物品
            // 初始化自动拾取复选框
            CheckBoxAutoPick = new MirCheckBox
            {
                Index = 2086,
                UnTickedIndex = 2086,
                TickedIndex = 2087,
                Parent = this,
                Location = new Point(26, 69),
                Library = Libraries.Prguse
            };
            CheckBoxAutoPick.LabelText = "自动拾取";
            CheckBoxAutoPick.Checked = Settings.AutoPick;
            CheckBoxAutoPick.Click += CheckBoxAutoPickClick;

            // 初始化物品过滤复选框数组
            CheckBoxItemFilter = new MirCheckBox[PageSize];
            for (int i = 0; i < PageSize; i++)
            {
                int x = i < PageSize / 2 ? 150 : 300;
                int y = i < PageSize / 2 ? 70 + 20 * i : 70 + 20 * (i - 5);
                int j = i;
                CheckBoxItemFilter[i] = new MirCheckBox
                {
                    Index = 2086,
                    UnTickedIndex = 2086,
                    TickedIndex = 2087,
                    Parent = this,
                    Location = new Point(x, y),
                    Library = Libraries.Prguse
                };
                // 为物品过滤复选框的点击事件绑定处理方法
                CheckBoxItemFilter[i].Click += (o, e) => CheckBoxItemFilterClick(j);
            }

            // 初始化物品过滤列表当前页码标签
            PageNumberLabel = new MirLabel
            {
                Text = "",
                Parent = this,
                Size = new Size(83, 17),
                Location = new Point(217, 170),
                DrawFormat = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter,
                Font = new Font(Settings.FontName, 7F),
            };

            // 初始化物品过滤列表上一页按钮
            PreviousButton = new MirButton
            {
                Index = 240,
                HoverIndex = 241,
                PressedIndex = 242,
                Library = Libraries.Prguse2,
                Parent = this,
                Location = new Point(220, 172),
                Sound = SoundList.ButtonA,
            };
            // 为上一页按钮的点击事件绑定处理方法
            PreviousButton.Click += (o, e) =>
            {
                Page--;
                if (Page < 0) Page = 0;
                StartIndex = PageSize * Page;

                UpdateItemFilters();
            };

            // 初始化物品过滤列表下一页按钮
            NextButton = new MirButton
            {
                Index = 243,
                HoverIndex = 244,
                PressedIndex = 245,
                Library = Libraries.Prguse2,
                Parent = this,
                Location = new Point(280, 172),
                Sound = SoundList.ButtonA,
            };
            // 为下一页按钮的点击事件绑定处理方法
            NextButton.Click += (o, e) =>
            {
                Page++;
                if ((Page + 1) > maxPage) Page--;
                StartIndex = PageSize * Page;
                UpdateItemFilters();
            };

            #endregion

            // 初始化关闭对话框按钮
            CloseButton = new MirButton
            {
                HoverIndex = 361,
                Index = 360,
                Location = new Point(428, 1),
                Library = Libraries.Prguse2,
                Parent = this,
                PressedIndex = 362,
                Sound = SoundList.ButtonA,
            };
            // 为关闭按钮的点击事件绑定隐藏对话框的方法
            CloseButton.Click += (o, e) => Hide();
        }

        /// <summary>
        /// 处理物品过滤复选框的点击事件，更新物品过滤列表中的拾取状态
        /// </summary>
        /// <param name="j">复选框的索引</param>
        private void CheckBoxItemFilterClick(int j)
        {
            string name = CheckBoxItemFilter[j].LabelText;
            if (GameScene.Scene.AssistHelper.ItemFilterList.ContainsKey(name))
                GameScene.Scene.AssistHelper.ItemFilterList[name].Pick = CheckBoxItemFilter[j].Checked;
        }

        /// <summary>
        /// 更新物品过滤列表的显示内容和页码标签
        /// </summary>
        private void UpdateItemFilters()
        {
            ItemFilter[] items = GameScene.Scene.AssistHelper.ItemFilterList.Values.ToArray();
            for (int i = 0; i < PageSize; ++i)
            {
                if (StartIndex + i < items.Length)
                {
                    // 显示复选框并设置标签文本和选中状态
                    CheckBoxItemFilter[i].Visible = true;
                    CheckBoxItemFilter[i].LabelText = items[StartIndex + i].Name;
                    CheckBoxItemFilter[i].Checked = items[StartIndex + i].Pick;
                }
                else
                {
                    // 隐藏超出列表范围的复选框
                    CheckBoxItemFilter[i].Visible = false;
                }
            }

            // 更新页码标签的显示内容
            PageNumberLabel.Text = (Page + 1) + " / " + maxPage;
        }

        /// <summary>
        /// 创建保护设置相关的控件，包括标签和文本框
        /// </summary>
        /// <param name="index">控件的索引</param>
        /// <param name="type">保护类型，0 表示生命百分比，1 表示魔法百分比</param>
        private void CreateProtectControls(int index, int type)
        {
            int height = 95 + 25 * index;
            // 初始化保护百分比标签
            LabelProtect[index] = new MirLabel { AutoSize = true, Location = new Point(26, height), Parent = this, Text = type == 0 ? "生命百分比低于" : "魔法百分比低于" };

            // 初始化保护百分比文本框
            TextBoxProtectPercent[index] = new MirTextBox
            {
                Location = new Point(120, height),
                Parent = this,
                Size = new Size(30, 15),
                MaxLength = Globals.MaxPasswordLength,
                OnlyNumber = true,
                CanLoseFocus = true,
                FocusWhenVisible = false,
                Font = new Font(Settings.FontName, 8F),
                BackColour = Color.White,
                ForeColour = Color.Black
            };
            // 为文本框的文本改变事件绑定处理方法
            TextBoxProtectPercent[index].TextBox.TextChanged += (o, e) => PercentPercentTextBox_changed(index);
            // 设置文本框的初始文本
            TextBoxProtectPercent[index].Text = String.Format("{0}", GameScene.Scene.AssistHelper.GetProtectPercent(index));

            // 初始化使用标签
            LabelUse[index] = new MirLabel { AutoSize = true, Location = new Point(155, height), Parent = this, Text = "使用" };

            // 初始化保护物品文本框
            TextBoxProtectItem[index] = new MirTextBox
            {
                Location = new Point(200, height),
                Parent = this,
                Size = new Size(80, 15),
                MaxLength = Globals.MaxPasswordLength,
                CanLoseFocus = true,
                FocusWhenVisible = false,
                Font = new Font(Settings.FontName, 8F),
                BackColour = Color.White,
                ForeColour = Color.Black
            };
            // 为文本框的文本改变事件绑定处理方法
            TextBoxProtectItem[index].TextBox.TextChanged += (o, e) => PercentItemTextBox_changed(index);
            // 设置文本框的初始文本
            TextBoxProtectItem[index].Text = GameScene.Scene.AssistHelper.GetProtectItemName(index);
            // 初始化提示标签
            LabelTip[index] = new MirLabel { AutoSize = true, Location = new Point(285, height), Parent = this, Text = "填关键字" };
        }

        /// <summary>
        /// 处理保护物品文本框的文本改变事件，更新辅助助手的保护物品名称
        /// </summary>
        /// <param name="i">文本框的索引</param>
        private void PercentItemTextBox_changed(int i)
        {
            GameScene.Scene.AssistHelper.SetProtectItemName(i, TextBoxProtectItem[i].Text);
        }

        /// <summary>
        /// 处理保护百分比文本框的文本改变事件，更新辅助助手的保护百分比
        /// </summary>
        /// <param name="i">文本框的索引</param>
        private void PercentPercentTextBox_changed(int i)
        {
            int temp = 0;
            // 尝试将文本框中的文本转换为整数
            int.TryParse(TextBoxProtectPercent[i].Text, out temp);
            GameScene.Scene.AssistHelper.SetProtectPercent(i, temp);
        }

        /// <summary>
        /// 切换标签页的方法，根据选中的标签页显示或隐藏相应的控件
        /// </summary>
        /// <param name="j">选中的标签页索引</param>
        private void SwitchTab(int j)
        {
            // 设置标签页按钮的选中状态
            for (int i = 0; i < TabPageButton.Length; ++i)
                TabPageButton[i].Checked = i == j;

            // 显示或隐藏基本设置相关的复选框
            CheckBoxFreeShift.Visible = j == BASE;
            CheckBoxShowLevel.Visible = j == BASE;
            CheckBoxShowTransform.Visible = j == BASE;
            CheckBoxShowPing.Visible = j == BASE;
            CheckBoxShowGuildName.Visible = j == BASE;
            CheckBoxShowGroupInfo.Visible = j == BASE;
            CheckBoxShowHealth.Visible = j == BASE;
            CheckBoxShowDamage.Visible = j == BASE;
            CheckBoxShowHeal.Visible = j == BASE;
            CheckBoxHideDead.Visible = j == BASE;
            CheckBoxShowMonsterName.Visible = j == BASE;
            CheckBoxShowNPCName.Visible = j == BASE;
            CheckBoxHideSystem2.Visible = j == BASE;

            // 显示或隐藏职业设置相关的复选框
            CheckBoxSmartFire.Visible = j == CLASS;
            //CheckBoxSmartDaMo.Visible = j == CLASS;
            CheckBoxChangePoison.Visible = j == CLASS;
            CheckBoxJinGang.Visible = j == CLASS;

            //CheckBoxSmartYiJinJin.Visible = j==CLASS;

            // 显示或隐藏保护设置相关的复选框
            CheckBoxProtect.Visible = j == PROTECT;

            // 显示或隐藏保护设置相关的标签和文本框
            for (int i = 0; i < 3; i++)
            {
                LabelProtect[i].Visible = j == PROTECT;
                LabelUse[i].Visible = j == PROTECT;
                TextBoxProtectPercent[i].Visible = j == PROTECT;
                TextBoxProtectItem[i].Visible = j == PROTECT;
                LabelTip[i].Visible = j == PROTECT;
            }
            TextBoxSpell.Visible = j == BASE;


            // 显示或隐藏职业设置相关的复选框
            CheckBoxSmartSheild.Visible = j == CLASS;
            CheckBoxChangePoison.Visible = j == CLASS;
            CheckBoxSpaceThrusting.Visible = j == CLASS;

            // 显示或隐藏物品设置相关的控件
            CheckBoxAutoPick.Visible = j == ITEM;
            PreviousButton.Visible = j == ITEM;
            NextButton.Visible = j == ITEM;
            PageNumberLabel.Visible = j == ITEM;
            for (int i = 0; i < PageSize; i++)
                CheckBoxItemFilter[i].Visible = j == ITEM;

            if (j == ITEM)
                // 当切换到物品设置标签页时，更新物品过滤列表
                UpdateItemFilters();
        }

        /// <summary>
        /// 处理自动攻击按钮的点击事件，切换自动攻击状态并清除攻击目标
        /// </summary>
        private void AutoAttackClick()
        {
            // 切换自动攻击状态（开/关）
            GameScene.Scene.AssistHelper.AutoAttack = !GameScene.Scene.AssistHelper.AutoAttack;

            // 清除当前攻击目标或状态
            GameScene.Scene.AssistHelper.ClearAttack();

            // 根据当前状态，输出系统提示到聊天框
            if (GameScene.Scene.AssistHelper.AutoAttack)
                GameScene.Scene.ChatDialog.ReceiveChat("开始自动攻击", ChatType.System);
            else
                GameScene.Scene.ChatDialog.ReceiveChat("结束自动攻击", ChatType.System);
        }
    }
}
