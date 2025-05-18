// 引入 Client.MirControls 命名空间，用于使用客户端的控件类
using Client.MirControls;
// 引入 Client.MirGraphics 命名空间，用于处理客户端的图形相关操作
using Client.MirGraphics;
// 引入 Client.MirNetwork 命名空间，用于处理客户端的网络通信
using Client.MirNetwork;
// 引入 Client.MirScenes.Dialogs 命名空间，用于使用客户端场景中的对话框类
using Client.MirScenes.Dialogs;
// 引入 Client.MirSounds 命名空间，用于处理客户端的声音播放
using Client.MirSounds;
// 为 ClientPackets 命名空间设置别名 C，方便后续使用
using C = ClientPackets;
// 为 ServerPackets 命名空间设置别名 S，方便后续使用
using S = ServerPackets;

namespace Client.MirScenes
{
    /// <summary>
    /// 角色选择场景类，继承自 MirScene 类，负责处理角色选择界面的相关逻辑和显示
    /// </summary>
    public class SelectScene : MirScene
    {
        // 定义背景图片控件
        public MirImageControl Background, Title;
        // 定义新建角色对话框
        private NewCharacterDialog _character;
        // 定义服务器标签控件
        public MirLabel ServerLabel;
        // 定义角色显示动画控件
        public MirAnimatedControl CharacterDisplay;
        // 定义开始游戏、新建角色、删除角色、显示信息、退出游戏按钮
        public MirButton StartGameButton, NewCharacterButton, DeleteCharacterButton, CreditsButton, ExitGame;
        // 定义角色选择按钮数组
        public CharacterButton[] CharacterButtons;
        // 定义上次登录时间标签和其标题标签
        public MirLabel LastAccessLabel, LastAccessLabelLabel;
        // 定义角色信息列表
        public List<SelectInfo> Characters = new List<SelectInfo>();
        // 定义当前选中的角色索引
        private int _selected;

        /// <summary>
        /// 构造函数，初始化角色选择场景
        /// </summary>
        /// <param name="characters">角色信息列表</param>
        public SelectScene(List<SelectInfo> characters)
        {
            // 播放角色选择场景的音乐，循环播放
            SoundManager.PlayMusic(SoundList.SelectMusic, true);
            // 场景销毁时停止播放音乐
            Disposing += (o, e) => SoundManager.StopMusic();

            // 赋值角色信息列表
            Characters = characters;
            // 对角色信息列表进行排序
            SortList();

            // 绑定按键按下事件
            KeyPress += SelectScene_KeyPress;

            // 初始化背景图片控件
            Background = new MirImageControl
            {
                // 人物选择创建界面背景图片，更改整体分辨率需要修改
                Index = 67,
                // 图片资源库
                Library = Libraries.Prguse,
                // 设置父控件为当前场景
                Parent = this,
            };

            // 初始化标题图片控件
            Title = new MirImageControl
            {
                // 人物选择创建界面标题图片
                Index = 40,
                // 图片资源库
                Library = Libraries.Title,
                // 设置父控件为当前场景
                Parent = this,
                // 放置位置
                Location = new Point(600, 200)
            };

            // 初始化服务器标签控件
            ServerLabel = new MirLabel
            {
                // 选择人物界面上方标题栏位置 1280X768 位置
                Location = new Point(560, 60),
                // 设置父控件为背景图片控件
                Parent = Background,
                // 标签大小
                Size = new Size(155, 17),
                // 标签文本
                Text = "Legend of Mir 2",
                // 文本绘制格式：水平居中、垂直居中
                DrawFormat = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
            };

            // 计算按钮的 x 坐标基准值
            var xPoint = ((Settings.ScreenWidth - 200) / 5);

            // 初始化开始游戏按钮
            StartGameButton = new MirButton
            {
                // 初始禁用按钮
                Enabled = false,
                // 鼠标悬停时的图片索引
                HoverIndex = 341,
                // 正常状态的图片索引
                Index = 340,
                // 图片资源库
                Library = Libraries.Title,
                // 按钮位置
                Location = new Point(100 + (xPoint * 1) - (xPoint / 2) - 50, Settings.ScreenHeight - 32),
                // 设置父控件为背景图片控件
                Parent = Background,
                // 按钮按下时的图片索引
                PressedIndex = 342
            };
            // 绑定按钮点击事件，点击时开始游戏
            StartGameButton.Click += (o, e) => StartGame();

            // 初始化新建角色按钮
            NewCharacterButton = new MirButton
            {
                // 鼠标悬停时的图片索引
                HoverIndex = 344,
                // 正常状态的图片索引
                Index = 343,
                // 图片资源库
                Library = Libraries.Title,
                // 按钮位置
                Location = new Point(100 + (xPoint * 2) - (xPoint / 2) - 50, Settings.ScreenHeight - 32),
                // 设置父控件为背景图片控件
                Parent = Background,
                // 按钮按下时的图片索引
                PressedIndex = 345,
            };
            // 绑定按钮点击事件，点击时打开新建角色对话框
            NewCharacterButton.Click += (o, e) => OpenNewCharacterDialog();

            // 初始化删除角色按钮
            DeleteCharacterButton = new MirButton
            {
                // 鼠标悬停时的图片索引
                HoverIndex = 347,
                // 正常状态的图片索引
                Index = 346,
                // 图片资源库
                Library = Libraries.Title,
                // 按钮位置
                Location = new Point(100 + (xPoint * 3) - (xPoint / 2) - 50, Settings.ScreenHeight - 32),
                // 设置父控件为背景图片控件
                Parent = Background,
                // 按钮按下时的图片索引
                PressedIndex = 348
            };
            // 绑定按钮点击事件，点击时删除角色
            DeleteCharacterButton.Click += (o, e) => DeleteCharacter();

            // 初始化显示信息按钮
            CreditsButton = new MirButton
            {
                // 鼠标悬停时的图片索引
                HoverIndex = 350,
                // 正常状态的图片索引
                Index = 349,
                // 图片资源库
                Library = Libraries.Title,
                // 按钮位置
                Location = new Point(100 + (xPoint * 4) - (xPoint / 2) - 50, Settings.ScreenHeight - 32),
                // 设置父控件为背景图片控件
                Parent = Background,
                // 按钮按下时的图片索引
                PressedIndex = 351
            };
            // 绑定按钮点击事件，当前为空实现
            CreditsButton.Click += (o, e) =>
            {

            };

            // 初始化退出游戏按钮
            ExitGame = new MirButton
            {
                // 鼠标悬停时的图片索引
                HoverIndex = 353,
                // 正常状态的图片索引
                Index = 352,
                // 图片资源库
                Library = Libraries.Title,
                // 按钮位置
                Location = new Point(100 + (xPoint * 5) - (xPoint / 2) - 50, Settings.ScreenHeight - 32),
                // 设置父控件为背景图片控件
                Parent = Background,
                // 按钮按下时的图片索引
                PressedIndex = 354
            };
            // 绑定按钮点击事件，点击时关闭程序
            ExitGame.Click += (o, e) => Program.Form.Close();

            // 初始化角色显示动画控件
            CharacterDisplay = new MirAnimatedControl
            {
                // 启用动画
                Animated = true,
                // 动画帧数
                AnimationCount = 16,
                // 动画延迟时间
                AnimationDelay = 250,
                // 启用淡入效果
                FadeIn = true,
                // 淡入延迟时间
                FadeInDelay = 75,
                // 淡入速率
                FadeInRate = 0.1F,
                // 图片索引
                Index = 220,
                // 图片资源库
                Library = Libraries.ChrSel,
                // 控件位置
                Location = new Point(200, 300),
                // 设置父控件为背景图片控件
                Parent = Background,
                // 使用偏移量
                UseOffSet = true,
                // 初始隐藏控件
                Visible = false
            };
            // 绑定控件绘制完成后的事件
            CharacterDisplay.AfterDraw += (o, e) =>
            {
                // if (_selected >= 0 && _selected < Characters.Count && characters[_selected].Class == MirClass.Wizard)
                // 绘制混合图片
                Libraries.ChrSel.DrawBlend(CharacterDisplay.Index + 560, CharacterDisplay.DisplayLocationWithoutOffSet, Color.White, true);
            };

            // 初始化角色选择按钮数组
            CharacterButtons = new CharacterButton[4];

            // 初始化第一个角色选择按钮
            CharacterButtons[0] = new CharacterButton
            {
                // 按钮位置
                Location = new Point(447, 122),
                // 设置父控件为背景图片控件
                Parent = Background,
                // 按钮点击音效
                Sound = SoundList.ButtonA,
            };
            // 绑定按钮点击事件，点击时选择第一个角色并更新界面
            CharacterButtons[0].Click += (o, e) =>
            {
                if (characters.Count <= 0) return;

                _selected = 0;
                UpdateInterface();
            };

            // 初始化第二个角色选择按钮
            CharacterButtons[1] = new CharacterButton
            {
                // 按钮位置
                Location = new Point(447, 226),
                // 设置父控件为背景图片控件
                Parent = Background,
                // 按钮点击音效
                Sound = SoundList.ButtonA,
            };
            // 绑定按钮点击事件，点击时选择第二个角色并更新界面
            CharacterButtons[1].Click += (o, e) =>
            {
                if (characters.Count <= 1) return;
                _selected = 1;
                UpdateInterface();
            };

            // 初始化第三个角色选择按钮
            CharacterButtons[2] = new CharacterButton
            {
                // 按钮位置
                Location = new Point(447, 330),
                // 设置父控件为背景图片控件
                Parent = Background,
                // 按钮点击音效
                Sound = SoundList.ButtonA,
            };
            // 绑定按钮点击事件，点击时选择第三个角色并更新界面
            CharacterButtons[2].Click += (o, e) =>
            {
                if (characters.Count <= 2) return;

                _selected = 2;
                UpdateInterface();
            };

            // 初始化第四个角色选择按钮
            CharacterButtons[3] = new CharacterButton
            {
                // 按钮位置
                Location = new Point(447, 434),
                // 设置父控件为背景图片控件
                Parent = Background,
                // 按钮点击音效
                Sound = SoundList.ButtonA,
            };
            // 绑定按钮点击事件，点击时选择第四个角色并更新界面
            CharacterButtons[3].Click += (o, e) =>
            {
                if (characters.Count <= 3) return;

                _selected = 3;
                UpdateInterface();
            };

            // 初始化上次登录时间标签
            LastAccessLabel = new MirLabel
            {
                // 标签位置
                Location = new Point(140, 509),
                // 设置父控件为背景图片控件
                Parent = Background,
                // 标签大小
                Size = new Size(180, 21),
                // 文本绘制格式：左对齐、垂直居中
                DrawFormat = TextFormatFlags.Left | TextFormatFlags.VerticalCenter,
                // 显示边框
                Border = true,
            };
            // 初始化上次登录时间标签的标题标签
            LastAccessLabelLabel = new MirLabel
            {
                // 标签位置
                Location = new Point(-80, -1),
                // 设置父控件为上次登录时间标签
                Parent = LastAccessLabel,
                // 标签文本
                Text = "上次联机:",
                // 标签大小
                Size = new Size(100, 21),
                // 文本绘制格式：左对齐、垂直居中
                DrawFormat = TextFormatFlags.Left | TextFormatFlags.VerticalCenter,
                // 显示边框
                Border = true,
            };
            // 更新界面显示
            UpdateInterface();
        }

        /// <summary>
        /// 处理按键按下事件，当按下回车键且开始游戏按钮可用时，开始游戏
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">按键事件参数</param>
        private void SelectScene_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 如果按下的不是回车键，直接返回
            if (e.KeyChar != (char)Keys.Enter) return;
            // 如果开始游戏按钮可用，开始游戏
            if (StartGameButton.Enabled)
                StartGame();
            // 标记事件已处理
            e.Handled = true;
        }

        /// <summary>
        /// 对角色信息列表进行排序，按上次登录时间降序排列
        /// </summary>
        public void SortList()
        {
            if (Characters != null)
                Characters.Sort((c1, c2) => c2.LastAccess.CompareTo(c1.LastAccess));
        }

        /// <summary>
        /// 打开新建角色对话框
        /// </summary>
        private void OpenNewCharacterDialog()
        {
            // 如果对话框不存在或已销毁，则创建新的对话框
            if (_character == null || _character.IsDisposed)
            {
                _character = new NewCharacterDialog { Parent = this };

                // 绑定对话框创建角色事件，点击创建按钮时发送新建角色请求
                _character.OnCreateCharacter += (o, e) =>
                {
                    Network.Enqueue(new C.NewCharacter
                    {
                        Name = _character.NameTextBox.Text,
                        Class = _character.Class,
                        Gender = _character.Gender
                    });
                };
            }

            // 显示对话框
            _character.Show();
        }

        /// <summary>
        /// 开始游戏的逻辑
        /// </summary>
        public void StartGame()
        {
            // 如果资源库未加载完成
            if (!Libraries.Loaded)
            {
                // 创建加载进度动画控件
                MirAnimatedControl loadProgress = new MirAnimatedControl
                {
                    // 图片资源库
                    Library = Libraries.Prguse,
                    // 图片索引
                    Index = 940,
                    // 显示控件
                    Visible = true,
                    // 设置父控件为当前场景
                    Parent = this,
                    // 控件位置
                    Location = new Point(470, 680),
                    // 启用动画
                    Animated = true,
                    // 动画帧数
                    AnimationCount = 9,
                    // 动画延迟时间
                    AnimationDelay = 100,
                    // 循环播放动画
                    Loop = true,
                };
                // 绑定控件绘制完成后的事件，资源库加载完成后销毁控件并重新调用开始游戏方法
                loadProgress.AfterDraw += (o, e) =>
                {
                    if (!Libraries.Loaded) return;
                    loadProgress.Dispose();
                    StartGame();
                };
                return;
            }
            // 禁用开始游戏按钮
            StartGameButton.Enabled = false;

            // 发送开始游戏请求
            Network.Enqueue(new C.StartGame
            {
                CharacterIndex = Characters[_selected].Index
            });
            // 如果配置文件夹不存在，则创建文件夹
            if (!Directory.Exists("./Configs/")) Directory.CreateDirectory("./Configs/");
            // 读取角色配置文件
            Settings.AssistReader = new InIReader("./Configs/" + Characters[_selected].Name + ".txt");
            // 从配置文件中读取设置
            InIAttribute.ReadInI<Settings>(Settings.AssistReader);
        }

        /// <summary>
        /// 处理场景的更新逻辑，当前为空实现
        /// </summary>
        public override void Process()
        {

        }

        /// <summary>
        /// 处理接收到的网络数据包
        /// </summary>
        /// <param name="p">网络数据包</param>
        public override void ProcessPacket(Packet p)
        {
            switch (p.Index)
            {
                case (short)ServerPacketIds.NewCharacter:
                    // 处理新建角色请求的响应
                    NewCharacter((S.NewCharacter)p);
                    break;
                case (short)ServerPacketIds.NewCharacterSuccess:
                    // 处理新建角色成功的响应
                    NewCharacter((S.NewCharacterSuccess)p);
                    break;
                case (short)ServerPacketIds.DeleteCharacter:
                    // 处理删除角色请求的响应
                    DeleteCharacter((S.DeleteCharacter)p);
                    break;
                case (short)ServerPacketIds.DeleteCharacterSuccess:
                    // 处理删除角色成功的响应
                    DeleteCharacter((S.DeleteCharacterSuccess)p);
                    break;
                case (short)ServerPacketIds.StartGame:
                    // 处理开始游戏请求的响应
                    StartGame((S.StartGame)p);
                    break;
                case (short)ServerPacketIds.StartGameBanned:
                    // 处理开始游戏被封禁的响应
                    StartGame((S.StartGameBanned)p);
                    break;
                case (short)ServerPacketIds.StartGameDelay:
                    // 处理开始游戏延迟的响应
                    StartGame((S.StartGameDelay)p);
                    break;
                default:
                    // 调用基类的数据包处理方法
                    base.ProcessPacket(p);
                    break;
            }
        }

        /// <summary>
        /// 处理新建角色请求的响应
        /// </summary>
        /// <param name="p">新建角色响应数据包</param>
        private void NewCharacter(S.NewCharacter p)
        {
            // 启用新建角色对话框的确定按钮
            _character.OKButton.Enabled = true;

            switch (p.Result)
            {
                case 0:
                    // 服务器当前禁止创建账户
                    MirMessageBox.Show("服务器当前禁止创建账户");
                    // 销毁新建角色对话框
                    _character.Dispose();
                    break;
                case 1:
                    // 角色名不可用
                    MirMessageBox.Show("角色名不可用");
                    // 让角色名输入框获取焦点
                    _character.NameTextBox.SetFocus();
                    break;
                case 2:
                    // 选择的性别不存在
                    MirMessageBox.Show("选择的性别不存在\n 联系管理员");
                    break;
                case 3:
                    // 选择的职业不存在
                    MirMessageBox.Show("选择的职业不存在\n 联系管理员");
                    break;
                case 4:
                    // 每个账户只能创建指定数量的角色
                    MirMessageBox.Show("每个账户只能创建" + Globals.MaxCharacterCount + "角色");
                    // 销毁新建角色对话框
                    _character.Dispose();
                    break;
                case 5:
                    // 角色名被使用
                    MirMessageBox.Show("角色名被使用");
                    // 让角色名输入框获取焦点
                    _character.NameTextBox.SetFocus();
                    break;
            }
        }

        /// <summary>
        /// 处理新建角色成功的响应
        /// </summary>
        /// <param name="p">新建角色成功响应数据包</param>
        private void NewCharacter(S.NewCharacterSuccess p)
        {
            // 销毁新建角色对话框
            _character.Dispose();
            // 显示角色创建成功的提示框
            MirMessageBox.Show("角色创建成功");

            // 将新角色信息插入到列表开头
            Characters.Insert(0, p.CharInfo);
            // 设置当前选中的角色为新创建的角色
            _selected = 0;
            // 更新界面显示
            UpdateInterface();
        }

        /// <summary>
        /// 处理删除角色的逻辑
        /// </summary>
        private void DeleteCharacter()
        {
            // 如果当前没有选中角色，直接返回
            if (_selected < 0 || _selected >= Characters.Count) return;

            // 显示确认删除角色的提示框
            MirMessageBox message = new MirMessageBox(string.Format("确定要删除角色 {0} 删除后不可恢复", Characters[_selected].Name), MirMessageBoxButtons.YesNo);
            // 获取要删除角色的索引
            int index = Characters[_selected].Index;

            // 绑定提示框的确定按钮点击事件
            message.YesButton.Click += (o1, e1) =>
            {
                // 显示输入角色名的输入框
                MirInputBox inputBox = new MirInputBox("请输入删除角色名");
                // 绑定输入框的确定按钮点击事件
                inputBox.OKButton.Click += (o, e) =>
                {
                    // 获取要删除角色的名称
                    string name = Characters[_selected].Name.ToString();

                    // 如果输入的角色名正确，发送删除角色请求
                    if (inputBox.InputTextBox.Text == name)
                    {
                        // 禁用删除角色按钮
                        DeleteCharacterButton.Enabled = false;
                        // 发送删除角色请求
                        Network.Enqueue(new C.DeleteCharacter { CharacterIndex = index });
                    }
                    else
                    {
                        // 显示输入错误的提示框
                        MirMessageBox failedMessage = new MirMessageBox(string.Format("输入错误"), MirMessageBoxButtons.OK);
                        failedMessage.Show();
                    }
                    // 销毁输入框
                    inputBox.Dispose();
                };
                // 显示输入框
                inputBox.Show();
            };
            // 显示确认删除角色的提示框
            message.Show();
        }

        /// <summary>
        /// 处理删除角色请求的响应
        /// </summary>
        /// <param name="p">删除角色响应数据包</param>
        private void DeleteCharacter(S.DeleteCharacter p)
        {
            // 启用删除角色按钮
            DeleteCharacterButton.Enabled = true;
            switch (p.Result)
            {
                case 0:
                    // 服务器当前禁止删除账户
                    MirMessageBox.Show("服务器当前禁止删除账户");
                    break;
                case 1:
                    // 账户不存在
                    MirMessageBox.Show("账户不存在\n联系管理员");
                    break;
            }
        }

        /// <summary>
        /// 处理删除角色成功的响应
        /// </summary>
        /// <param name="p">删除角色成功响应数据包</param>
        private void DeleteCharacter(S.DeleteCharacterSuccess p)
        {
            // 启用删除角色按钮
            DeleteCharacterButton.Enabled = true;
            // 显示角色删除成功的提示框
            MirMessageBox.Show("角色删除成功");

            // 从角色信息列表中移除被删除的角色
            for (int i = 0; i < Characters.Count; i++)
                if (Characters[i].Index == p.CharacterIndex)
                {
                    Characters.RemoveAt(i);
                    break;
                }

            // 更新界面显示
            UpdateInterface();
        }

        /// <summary>
        /// 处理开始游戏延迟的响应
        /// </summary>
        /// <param name="p">开始游戏延迟响应数据包</param>
        private void StartGame(S.StartGameDelay p)
        {
            // 启用开始游戏按钮
            StartGameButton.Enabled = true;

            // 计算下次登录时间
            long time = CMain.Time + p.Milliseconds;

            // 显示登录失败的提示框
            MirMessageBox message = new MirMessageBox(string.Format("账户登录失败 再次登录时间 {0} 秒", Math.Ceiling(p.Milliseconds / 1000M)));

            // 绑定提示框绘制前的事件，更新提示框文本
            message.BeforeDraw += (o, e) => message.Label.Text = string.Format("账户登录失败 再次登录时间 {0} 秒", Math.Ceiling((time - CMain.Time) / 1000M));

            // 绑定提示框绘制后的事件，下次登录时间到达后销毁提示框并重新调用开始游戏方法
            message.AfterDraw += (o, e) =>
            {
                if (CMain.Time <= time) return;
                message.Dispose();
                StartGame();
            };

            // 显示提示框
            message.Show();
        }

        /// <summary>
        /// 处理开始游戏被封禁的响应
        /// </summary>
        /// <param name="p">开始游戏被封禁响应数据包</param>
        public void StartGame(S.StartGameBanned p)
        {
            // 启用开始游戏按钮
            StartGameButton.Enabled = true;

            // 计算封禁剩余时间
            TimeSpan d = p.ExpiryDate - CMain.Now;
            // 显示账户被封禁的提示框
            MirMessageBox.Show(string.Format("此账户被禁用\n\n原因{0}\n解禁日期{1}\n倒计时{2:#,##0} 小时, {3} 分钟, {4} 秒", p.Reason,
                                             p.ExpiryDate, Math.Floor(d.TotalHours), d.Minutes, d.Seconds));
        }

        /// <summary>
        /// 处理开始游戏请求的响应
        /// </summary>
        /// <param name="p">开始游戏响应数据包</param>
        public void StartGame(S.StartGame p)
        {
            // 启用开始游戏按钮
            StartGameButton.Enabled = true;

            switch (p.Result)
            {
                case 0:
                    // 服务器维护禁止登录
                    MirMessageBox.Show("服务器维护禁止登录");
                    break;
                case 1:
                    // 尚未登录
                    MirMessageBox.Show("尚未登录");
                    break;
                case 2:
                    // 没有激活角色
                    MirMessageBox.Show("没有激活角色");
                    break;
                case 3:
                    // 无效地图或没有新手出生点
                    MirMessageBox.Show("无效地图或没有新手出生点");
                    break;
                case 4:
                    // 根据服务器返回的分辨率设置客户端分辨率
                    if (p.Resolution < Settings.Resolution || Settings.Resolution == 0) Settings.Resolution = p.Resolution;

                    switch (Settings.Resolution)
                    {
                        default:
                        case 1024:
                            Settings.Resolution = 1024;
                            // 设置客户端分辨率为 1024x768
                            CMain.SetResolution(1024, 768);
                            break;
                        case 1280:
                            // 设置客户端分辨率为 1280x768
                            CMain.SetResolution(1280, 768);
                            break;
                        case 1366:
                            // 设置客户端分辨率为 1366x768
                            CMain.SetResolution(1366, 768);
                            break;
                        case 1920:
                            // 设置客户端分辨率为 1920x1080
                            CMain.SetResolution(1920, 1080);
                            break;
                    }

                    // 切换到游戏场景
                    ActiveScene = new GameScene();
                    // 销毁当前场景
                    Dispose();
                    break;
            }
        }

        /// <summary>
        /// 更新界面显示，根据当前选中的角色更新角色选择按钮、角色显示动画和上次登录时间标签
        /// </summary>
        private void UpdateInterface()
        {
            // 遍历角色选择按钮数组，更新按钮的选中状态和显示信息
            for (int i = 0; i < CharacterButtons.Length; i++)
            {
                CharacterButtons[i].Selected = i == _selected;
                CharacterButtons[i].Update(i >= Characters.Count ? null : Characters[i]);
            }

            // 如果当前有选中角色
            if (_selected >= 0 && _selected < Characters.Count)
            {
                // 显示角色显示动画控件
                CharacterDisplay.Visible = true;
                // 根据角色职业和性别设置角色显示动画的图片索引
                switch ((MirClass)Characters[_selected].Class)
                {
                    case MirClass.战士:
                        CharacterDisplay.Index = (byte)Characters[_selected].Gender == 0 ? 20 : 300;
                        break;
                    case MirClass.法师:
                        CharacterDisplay.Index = (byte)Characters[_selected].Gender == 0 ? 40 : 320;
                        break;
                    case MirClass.道士:
                        CharacterDisplay.Index = (byte)Characters[_selected].Gender == 0 ? 60 : 340;
                        break;
                    case MirClass.刺客:
                        CharacterDisplay.Index = (byte)Characters[_selected].Gender == 0 ? 80 : 360;
                        break;
                    case MirClass.弓箭:
                        CharacterDisplay.Index = (byte)Characters[_selected].Gender == 0 ? 100 : 140;
                        break;
                }

                // 显示上次登录时间，如果角色未曾登录则显示 "未曾登录"
                LastAccessLabel.Text = Characters[_selected].LastAccess == DateTime.MinValue ? "未曾登录" : Characters[_selected].LastAccess.ToString();
                // 显示上次登录时间标签和其标题标签
                LastAccessLabel.Visible = true;
                LastAccessLabelLabel.Visible = true;
                // 启用开始游戏按钮
                StartGameButton.Enabled = true;
            }
            else
            {
                // 隐藏角色显示动画控件
                CharacterDisplay.Visible = false;
                // 隐藏上次登录时间标签和其标题标签
                LastAccessLabel.Visible = false;
                LastAccessLabelLabel.Visible = false;
                // 禁用开始游戏按钮
                StartGameButton.Enabled = false;
            }
        }

        #region Disposable
        /// <summary>
        /// 释放资源的方法
        /// </summary>
        /// <param name="disposing">是否释放托管资源</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // 释放背景图片控件
                Background = null;
                // 释放新建角色对话框
                _character = null;

                // 释放服务器标签控件
                ServerLabel = null;
                // 释放角色显示动画控件
                CharacterDisplay = null;
                // 释放开始游戏按钮
                StartGameButton = null;
                // 释放新建角色按钮
                NewCharacterButton = null;
                // 释放删除角色按钮
                DeleteCharacterButton = null;
                // 释放显示信息按钮
                CreditsButton = null;
                // 释放退出游戏按钮
                ExitGame = null;
                // 释放角色选择按钮数组
                CharacterButtons = null;
                // 释放上次登录时间标签和其标题标签
                LastAccessLabel = null; LastAccessLabelLabel = null;
                // 释放角色信息列表
                Characters = null;
                // 重置当前选中的角色索引
                _selected = 0;
            }

            base.Dispose(disposing);
        }
        #endregion        
        public sealed class CharacterButton : MirImageControl
        {
            public MirLabel NameLabel, LevelLabel, ClassLabel;
            public bool Selected;

            public CharacterButton()
            {
                Index = 44; //45 locked
                Library = Libraries.Prguse;
                Sound = SoundList.ButtonA;

                NameLabel = new MirLabel
                {
                    Location = new Point(107, 9),
                    Parent = this,
                    NotControl = true,
                    Size = new Size(170, 18)
                };

                LevelLabel = new MirLabel
                {
                    Location = new Point(107, 28),
                    Parent = this,
                    NotControl = true,
                    Size = new Size(30, 18)
                };

                ClassLabel = new MirLabel
                {
                    Location = new Point(178, 28),
                    Parent = this,
                    NotControl = true,
                    Size = new Size(100, 18)
                };
            }

            public void Update(SelectInfo info)
            {
                if (info == null)
                {
                    Index = 44;
                    Library = Libraries.Prguse;
                    NameLabel.Text = string.Empty;
                    LevelLabel.Text = string.Empty;
                    ClassLabel.Text = string.Empty;

                    NameLabel.Visible = false;
                    LevelLabel.Visible = false;
                    ClassLabel.Visible = false;

                    return;
                }

                Library = Libraries.Title;

                Index = 660 + (byte)info.Class;

                if (Selected) Index += 5;


                NameLabel.Text = info.Name;
                LevelLabel.Text = info.Level.ToString();
                ClassLabel.Text = info.Class.ToString();

                NameLabel.Visible = true;
                LevelLabel.Visible = true;
                ClassLabel.Visible = true;
            }
        }
    }
}
