﻿using Client.MirSounds;

namespace Client
{
    class Settings
    {
        public const long CleanDelay = 600000;
        // 屏幕分辨率设置
        public static int ScreenWidth = 1280, ScreenHeight = 768; //屏幕宽度 = 1024，屏幕高度 = 768  起始创建游戏窗口分辨率
        private static InIReader Reader = new InIReader(@".\Mir2Config.ini");
        private static InIReader QuestTrackingReader = new InIReader(Path.Combine(UserDataPath, @".\QuestTracking.ini"));// 配置文件读取器，分别用于主配置和任务追踪

        private static bool _useTestConfig;
        public static bool UseTestConfig
        {
            get
            {
                return _useTestConfig;
            }
            set 
            {
                if (value == true)
                {
                    Reader = new InIReader(@".\Mir2Test.ini");
                }
                _useTestConfig = value;
            }
        }
        public static InIReader AssistReader;

        public const string DataPath = @".\Data\",
                            MapPath = @".\Map\",
                            SoundPath = @".\Sound\",
                            ExtraDataPath = @".\Data\Extra\",
                            ShadersPath = @".\Data\Shaders\",
                            MonsterPath = @".\Data\Monster\",
                            GatePath = @".\Data\Gate\",
                            FlagPath = @".\Data\Flag\",
                            SiegePath = @".\Data\Siege\",
                            NPCPath = @".\Data\NPC\",
                            CArmourPath = @".\Data\CArmour\",
                            CWeaponPath = @".\Data\CWeapon\",
							CWeaponEffectPath = @".\Data\CWeaponEffect\",
							CHairPath = @".\Data\CHair\",
                            AArmourPath = @".\Data\AArmour\",
                            AWeaponPath = @".\Data\AWeapon\",
                            AWeaponEffectPath = @".\Data\AWeaponEffect\",
                            AHairPath = @".\Data\AHair\",
                            ARArmourPath = @".\Data\ARArmour\",
                            ARWeaponPath = @".\Data\ARWeapon\",
                            ARWeaponEffectPath = @".\Data\ARWeaponEffect\",
                            ARHairPath = @".\Data\ARHair\",
                            CHumEffectPath = @".\Data\CHumEffect\",
                            AHumEffectPath = @".\Data\AHumEffect\",
                            ARHumEffectPath = @".\Data\ARHumEffect\",
                            MountPath = @".\Data\Mount\",
                            FishingPath = @".\Data\Fishing\",
                            PetsPath = @".\Data\Pet\",
                            TransformPath = @".\Data\Transform\",
                            TransformMountsPath = @".\Data\TransformRide2\",
                            TransformEffectPath = @".\Data\TransformEffect\",
                            TransformWeaponEffectPath = @".\Data\TransformWeaponEffect\",
                            MouseCursorPath = @".\Data\Cursors\",
                            ResourcePath = @".\DirectX\",
                            UserDataPath = @".\Data\UserData\";

        // 日志相关设置
        public static bool LogErrors = true;
        public static bool LogChat = true;
        public static int RemainingErrorLogs = 100;

        // 图形相关设置
        public static bool FullScreen = false, Borderless = false, TopMost = true, MouseClip = false;
        public static string FontName = "Arial"; //"MS Sans Serif"
        public static float FontSize = 8F;
        public static bool UseMouseCursors = true;

        public static bool FPSCap = true;
        public static int MaxFPS = 100;
        public static int Resolution = 1280;
        public static bool DebugMode = false;

        //Network
        public static bool UseConfig = true;
        public static string IPAddress = "127.0.0.1";
        public static int Port = 7000;
        public const int TimeOut = 5000;

        //Sound
        public static int SoundOverLap = 3;
        private static byte _volume = 100;
        public static int SoundCleanMinutes = 5;

        public static byte Volume
        {
            get { return _volume; }
            set
            {
                switch (value)
                {
                    case > 100:
                        _volume = (byte)100;
                        break;
                    case <= 0:
                        _volume = (byte)0;
                        break;
                    default:
                        _volume = value;
                        break;
                }

                SoundManager.Vol = Convert.ToInt32(_volume);
            }
        }

        private static byte _musicVolume = 100;
        public static byte MusicVolume
        {
            get { return _musicVolume; }
            set
            {
                switch(value)
                {
                    case > 100:
                        _musicVolume = (byte)100;
                        break;
                    case <= 0:
                        _musicVolume = (byte)0;
                        break;
                    default:
                        _musicVolume = value;
                        break;
                }

                SoundManager.MusicVol = Convert.ToInt32(_musicVolume);
            }
        }

        //Game
        public static string AccountID = "",
                             Password = "";

        public static bool
            SkillMode = false,
            SkillBar = true,
            //SkillSet = true,
            Effect = true,
            LevelEffect = true,
            DropView = true,
            NameView = true,
            HPView = true,
            TransparentChat = false,
            ModeView = false,
            DuraView = false,
            DisplayDamage = true,
            TargetDead = false,
            HighlightTarget = true,
            ExpandedBuffWindow = true,
            ExpandedHeroBuffWindow = true,
            DisplayBodyName = false,
            NewMove = false;

        public static int[,] SkillbarLocation = new int[2, 2] { { 0, 0 }, { 216, 0 }  };

        //Quests
        public static int[] TrackedQuests = new int[5];

        //Chat
        public static bool
            ShowNormalChat = true,
            ShowYellChat = true,
            ShowWhisperChat = true,
            ShowLoverChat = true,
            ShowMentorChat = true,
            ShowGroupChat = true,
            ShowGuildChat = true;

        //Filters
        public static bool
            FilterNormalChat = false,
            FilterWhisperChat = false,
            FilterShoutChat = false,
            FilterSystemChat = false,
            FilterLoverChat = false,
            FilterMentorChat = false,
            FilterGroupChat = false,
            FilterGuildChat = false;


        //自动修补程序
        public static bool P_Patcher = true;
        public static string P_Host = @"http://127.0.0.1/mir2/cmir/patch/";//默认 http://mirfiles.com/mir2/cmir/patch/
        public static string P_PatchFileName = @"PList.gz";
        public static bool P_NeedLogin = false;
        public static string P_Login = string.Empty;
        public static string P_Password = string.Empty;
        public static string P_ServerName = string.Empty;
        public static string P_BrowserAddress = "http://127.0.0.1/mir2-patchsite/";//默认 https://www.lomcn.org/mir2-patchsite/
        public static string P_Client = Application.StartupPath + "\\";
        public static bool P_AutoStart = false;
        public static int P_Concurrency = 1;
        // 辅助外挂
        [InI("Assist")]
        public static bool FreeShift = true;

        [InI("Assist")]
        public static bool StruckShield = false;

        [InI("Assist")]
        public static bool SmartCrsHit = false;

        [InI("Assist")]
        public static bool SmartFireHit = true;

        [InI("Assist")]
        public static bool SmartSheild = false;

        [InI("Assist")]
        public static bool SmartChangeSign = true;

        [InI("Assist")]
        public static bool SmartChangePoison = true;

        [InI("Assist")]
        public static bool SmartElementalBarrier = true;

        [InI("Assist")]
        public static bool 开启保护 = true;

        [InI("Assist")]
        public static bool AutoPick = true;

        [InI("Assist")]
        public static bool 自动双龙斩 = false;

        [InI("Assist")]
        public static bool ShowLevel = false;

        [InI("Assist")]
        public static bool ShowTransform = true;

        [InI("Assist")]
        public static bool ShowGuildName = true;

        [InI("Assist")]
        public static int NumBackCityHP = 50;

        [InI("Assist")]
        public static int UseItemInterval = 3000;

        [InI("Assist")]
        public static bool ShowPing = true;

        [InI("Assist")]
        public static bool ShowHealth = true;

        [InI("Assist")]
        public static string PercentItem0 = "体力恢复药";

        [InI("Assist")]
        public static string PercentItem1 = "魔力恢复药";

        [InI("Assist")]
        public static string PercentItem2 = "回城卷";

        [InI("Assist")]
        public static int ProtectPercent0 = 50;

        [InI("Assist")]
        public static int ProtectPercent1 = 50;

        [InI("Assist")]
        public static int ProtectPercent2 = 5;

        [InI("Assist")]
        public static bool ShowGroupInfo = true;

        [InI("Assist")]
        public static bool ShowHeal = true;

        [InI("Assist")]
        public static bool ShowDamage = true;

        [InI("Assist")]
        public static bool ShowMonsterName = true;

        [InI("Assist")]
        public static bool ShowNPCName = true;

        [InI("Assist")]
        public static bool HideDead = false; //false

        [InI("Assist")]
        public static bool HideSystem2 = true; //false

        public static void Load()
        {
            GameLanguage.LoadClientLanguage(@".\Language.ini");

            if (!Directory.Exists(DataPath)) Directory.CreateDirectory(DataPath);
            if (!Directory.Exists(MapPath)) Directory.CreateDirectory(MapPath);
            if (!Directory.Exists(SoundPath)) Directory.CreateDirectory(SoundPath);
           
            //Graphics
            FullScreen = Reader.ReadBoolean("Graphics", "FullScreen", FullScreen);
            Borderless = Reader.ReadBoolean("Graphics", "Borderless", Borderless);
            MouseClip = Reader.ReadBoolean("Graphics", "MouseClip", MouseClip);
            TopMost = Reader.ReadBoolean("Graphics", "AlwaysOnTop", TopMost);
            FPSCap = Reader.ReadBoolean("Graphics", "FPSCap", FPSCap);
            Resolution = Reader.ReadInt32("Graphics", "Resolution", Resolution);
            DebugMode = Reader.ReadBoolean("Graphics", "DebugMode", DebugMode);
            UseMouseCursors = Reader.ReadBoolean("Graphics", "UseMouseCursors", UseMouseCursors);

            //Network
            UseConfig = Reader.ReadBoolean("Network", "UseConfig", UseConfig);
            if (UseConfig)
            {
                IPAddress = Reader.ReadString("Network", "IPAddress", IPAddress);
                Port = Reader.ReadInt32("Network", "Port", Port);
            }

            //Logs
            LogErrors = Reader.ReadBoolean("Logs", "LogErrors", LogErrors);
            LogChat = Reader.ReadBoolean("Logs", "LogChat", LogChat);

            //Sound
            Volume = Reader.ReadByte("Sound", "Volume", Volume);
            SoundOverLap = Reader.ReadInt32("Sound", "SoundOverLap", SoundOverLap);
            MusicVolume = Reader.ReadByte("Sound", "Music", MusicVolume);
            var n = Reader.ReadInt32("Sound", "CleanMinutes", SoundCleanMinutes);
            if (n < 1 || n > 60 * 3) n = SoundCleanMinutes;
            SoundCleanMinutes = n;


            //Game
            AccountID = Reader.ReadString("Game", "AccountID", AccountID);
            Password = Reader.ReadString("Game", "Password", Password);

            SkillMode = Reader.ReadBoolean("Game", "SkillMode", SkillMode);
            SkillBar = Reader.ReadBoolean("Game", "SkillBar", SkillBar);
            //SkillSet = Reader.ReadBoolean("Game", "SkillSet", SkillSet);
            Effect = Reader.ReadBoolean("Game", "Effect", Effect);
            LevelEffect = Reader.ReadBoolean("Game", "LevelEffect", Effect);
            DropView = Reader.ReadBoolean("Game", "DropView", DropView);
            NameView = Reader.ReadBoolean("Game", "NameView", NameView);
            HPView = Reader.ReadBoolean("Game", "HPMPView", HPView);
            ModeView = Reader.ReadBoolean("Game", "ModeView", ModeView);
            FontName = Reader.ReadString("Game", "FontName", FontName);
            TransparentChat = Reader.ReadBoolean("Game", "TransparentChat", TransparentChat);
            DisplayDamage = Reader.ReadBoolean("Game", "DisplayDamage", DisplayDamage);
            TargetDead = Reader.ReadBoolean("Game", "TargetDead", TargetDead);
            HighlightTarget = Reader.ReadBoolean("Game", "HighlightTarget", HighlightTarget);
            ExpandedBuffWindow = Reader.ReadBoolean("Game", "ExpandedBuffWindow", ExpandedBuffWindow);
            ExpandedHeroBuffWindow = Reader.ReadBoolean("Game", "ExpandedHeroBuffWindow", ExpandedHeroBuffWindow);
            DuraView = Reader.ReadBoolean("Game", "DuraWindow", DuraView);
            DisplayBodyName = Reader.ReadBoolean("Game", "DisplayBodyName", DisplayBodyName);
            NewMove = Reader.ReadBoolean("Game", "NewMove", NewMove);

            for (int i = 0; i < SkillbarLocation.Length / 2; i++)
            {
                SkillbarLocation[i, 0] = Reader.ReadInt32("Game", "Skillbar" + i.ToString() + "X", SkillbarLocation[i, 0]);
                SkillbarLocation[i, 1] = Reader.ReadInt32("Game", "Skillbar" + i.ToString() + "Y", SkillbarLocation[i, 1]);
            }

            //Chat
            ShowNormalChat = Reader.ReadBoolean("Chat", "ShowNormalChat", ShowNormalChat);
            ShowYellChat = Reader.ReadBoolean("Chat", "ShowYellChat", ShowYellChat);
            ShowWhisperChat = Reader.ReadBoolean("Chat", "ShowWhisperChat", ShowWhisperChat);
            ShowLoverChat = Reader.ReadBoolean("Chat", "ShowLoverChat", ShowLoverChat);
            ShowMentorChat = Reader.ReadBoolean("Chat", "ShowMentorChat", ShowMentorChat);
            ShowGroupChat = Reader.ReadBoolean("Chat", "ShowGroupChat", ShowGroupChat);
            ShowGuildChat = Reader.ReadBoolean("Chat", "ShowGuildChat", ShowGuildChat);

            //Filters
            FilterNormalChat = Reader.ReadBoolean("Filter", "FilterNormalChat", FilterNormalChat);
            FilterWhisperChat = Reader.ReadBoolean("Filter", "FilterWhisperChat", FilterWhisperChat);
            FilterShoutChat = Reader.ReadBoolean("Filter", "FilterShoutChat", FilterShoutChat);
            FilterSystemChat = Reader.ReadBoolean("Filter", "FilterSystemChat", FilterSystemChat);
            FilterLoverChat = Reader.ReadBoolean("Filter", "FilterLoverChat", FilterLoverChat);
            FilterMentorChat = Reader.ReadBoolean("Filter", "FilterMentorChat", FilterMentorChat);
            FilterGroupChat = Reader.ReadBoolean("Filter", "FilterGroupChat", FilterGroupChat);
            FilterGuildChat = Reader.ReadBoolean("Filter", "FilterGuildChat", FilterGuildChat);

            //AutoPatcher
            P_Patcher = Reader.ReadBoolean("Launcher", "Enabled", P_Patcher);
            P_Host = Reader.ReadString("Launcher", "Host", P_Host);
            P_PatchFileName = Reader.ReadString("Launcher", "PatchFile", P_PatchFileName);
            P_NeedLogin = Reader.ReadBoolean("Launcher", "NeedLogin", P_NeedLogin);
            P_Login = Reader.ReadString("Launcher", "Login", P_Login);
            P_Password = Reader.ReadString("Launcher", "Password", P_Password);
            P_AutoStart = Reader.ReadBoolean("Launcher", "AutoStart", P_AutoStart);
            P_ServerName = Reader.ReadString("Launcher", "ServerName", P_ServerName);
            P_BrowserAddress = Reader.ReadString("Launcher", "Browser", P_BrowserAddress);
            P_Concurrency = Reader.ReadInt32("Launcher", "ConcurrentDownloads", P_Concurrency);
            

            if (!P_Host.EndsWith("/")) P_Host += "/";
            if (P_Host.StartsWith("www.", StringComparison.OrdinalIgnoreCase)) P_Host = P_Host.Insert(0, "http://");
            if (P_BrowserAddress.StartsWith("www.", StringComparison.OrdinalIgnoreCase)) P_BrowserAddress = P_BrowserAddress.Insert(0, "http://");

            //临时检查以更新每个人的地址
            if (P_Host.ToLower() == "http://127.0.0.1/mir2/cmir/patch/")//默认 http://mirfiles.co.uk/mir2/cmir/patch/
            {
                P_Host = "http://127.0.0.1/mir2/cmir/patch/";//默认 http://mirfiles.com/mir2/cmir/patch/
            }

            if (P_Concurrency < 1) P_Concurrency = 1;
            if (P_Concurrency > 100) P_Concurrency = 100;
        }

        public static void Save()
        {
            //Graphics
            Reader.Write("Graphics", "FullScreen", FullScreen);
            Reader.Write("Graphics", "Borderless", Borderless);
            Reader.Write("Graphics", "MouseClip", MouseClip);
            Reader.Write("Graphics", "AlwaysOnTop", TopMost);
            Reader.Write("Graphics", "FPSCap", FPSCap);
            Reader.Write("Graphics", "Resolution", Resolution);
            Reader.Write("Graphics", "DebugMode", DebugMode);
            Reader.Write("Graphics", "UseMouseCursors", UseMouseCursors);

            //Sound
            Reader.Write("Sound", "Volume", Volume);
            Reader.Write("Sound", "SoundOverLap", SoundOverLap);
            Reader.Write("Sound", "Music", MusicVolume);
            Reader.Write("Sound", "CleanMinutes", SoundCleanMinutes);

            //Game
            Reader.Write("Game", "AccountID", AccountID);
            Reader.Write("Game", "Password", Password);
            Reader.Write("Game", "SkillMode", SkillMode);
            Reader.Write("Game", "SkillBar", SkillBar);
            //Reader.Write("Game", "SkillSet", SkillSet);
            Reader.Write("Game", "Effect", Effect);
            Reader.Write("Game", "LevelEffect", LevelEffect);
            Reader.Write("Game", "DropView", DropView);
            Reader.Write("Game", "NameView", NameView);
            Reader.Write("Game", "HPMPView", HPView);
            Reader.Write("Game", "ModeView", ModeView);
            Reader.Write("Game", "FontName", FontName);
            Reader.Write("Game", "TransparentChat", TransparentChat);
            Reader.Write("Game", "DisplayDamage", DisplayDamage);
            Reader.Write("Game", "TargetDead", TargetDead);
            Reader.Write("Game", "HighlightTarget", HighlightTarget);
            Reader.Write("Game", "ExpandedBuffWindow", ExpandedBuffWindow);
            Reader.Write("Game", "ExpandedHeroBuffWindow", ExpandedBuffWindow);
            Reader.Write("Game", "DuraWindow", DuraView);
            Reader.Write("Game", "DisplayBodyName", DisplayBodyName);
            Reader.Write("Game", "NewMove", NewMove);

            for (int i = 0; i < SkillbarLocation.Length / 2; i++)
            {

                Reader.Write("Game", "Skillbar" + i.ToString() + "X", SkillbarLocation[i, 0]);
                Reader.Write("Game", "Skillbar" + i.ToString() + "Y", SkillbarLocation[i, 1]);
            }

            //Chat
            Reader.Write("Chat", "ShowNormalChat", ShowNormalChat);
            Reader.Write("Chat", "ShowYellChat", ShowYellChat);
            Reader.Write("Chat", "ShowWhisperChat", ShowWhisperChat);
            Reader.Write("Chat", "ShowLoverChat", ShowLoverChat);
            Reader.Write("Chat", "ShowMentorChat", ShowMentorChat);
            Reader.Write("Chat", "ShowGroupChat", ShowGroupChat);
            Reader.Write("Chat", "ShowGuildChat", ShowGuildChat);

            //Filters
            Reader.Write("Filter", "FilterNormalChat", FilterNormalChat);
            Reader.Write("Filter", "FilterWhisperChat", FilterWhisperChat);
            Reader.Write("Filter", "FilterShoutChat", FilterShoutChat);
            Reader.Write("Filter", "FilterSystemChat", FilterSystemChat);
            Reader.Write("Filter", "FilterLoverChat", FilterLoverChat);
            Reader.Write("Filter", "FilterMentorChat", FilterMentorChat);
            Reader.Write("Filter", "FilterGroupChat", FilterGroupChat);
            Reader.Write("Filter", "FilterGuildChat", FilterGuildChat);

            //AutoPatcher
            Reader.Write("Launcher", "Enabled", P_Patcher);
            Reader.Write("Launcher", "Host", P_Host);
            Reader.Write("Launcher", "PatchFile", P_PatchFileName);
            Reader.Write("Launcher", "NeedLogin", P_NeedLogin);
            Reader.Write("Launcher", "Login", P_Login);
            Reader.Write("Launcher", "Password", P_Password);
            Reader.Write("Launcher", "ServerName", P_ServerName);
            Reader.Write("Launcher", "Browser", P_BrowserAddress);
            Reader.Write("Launcher", "AutoStart", P_AutoStart);
            Reader.Write("Launcher", "ConcurrentDownloads", P_Concurrency);

            Reader.Save();
            if (AssistReader != null)
            {
                InIAttribute.WriteInI<Settings>(AssistReader);
                AssistReader.Save();
            }
        }

        public static void LoadTrackedQuests(string charName)
        {
            //Quests
            for (int i = 0; i < TrackedQuests.Length; i++)
            {
                TrackedQuests[i] = QuestTrackingReader.ReadInt32(charName, "Quest-" + i.ToString(), -1);
            }
        }

        public static void SaveTrackedQuests(string charName)
        {
            //Quests
            for (int i = 0; i < TrackedQuests.Length; i++)
            {
                QuestTrackingReader.Write(charName, "Quest-" + i.ToString(), TrackedQuests[i]);
            }
        }
    }

    
}
