﻿using Client.MirControls;
using Client.MirGraphics;
using Client.MirScenes;
using Client.MirSounds;
using Client.MirScenes.Dialogs;
using SlimDX;

namespace Client.MirObjects
{
    public abstract class MapObject
    {
        public static Font ChatFont = new Font(Settings.FontName, 10F);
        public static List<MirLabel> LabelList = new List<MirLabel>();

        public static UserObject User;
        public static UserHeroObject Hero;
        public static HeroObject HeroObject;
        public static MapObject MouseObject, TargetObject, MagicObject;

        private static uint mouseObjectID;
        public static uint MouseObjectID
        {
            get { return mouseObjectID; }
            set
            {
                if (mouseObjectID == value) return;
                mouseObjectID = value;
                MouseObject = MapControl.Objects.Find(x => x.ObjectID == value);
            }
        }

        private static uint lastTargetObjectId;
        private static uint targetObjectID;
        public static uint TargetObjectID
        {
            get { return targetObjectID; }
            set
            {
                if (targetObjectID == value) return;
                lastTargetObjectId = value;
                targetObjectID = value;
                TargetObject = value == 0 ? null : MapControl.Objects.Find(x => x.ObjectID == value);
            }
        }

        private static uint magicObjectID;
        public static uint MagicObjectID
        {
            get { return magicObjectID; }
            set
            {
                if (magicObjectID == value) return;
                magicObjectID = value;
                MagicObject = MapControl.Objects.Find(x => x.ObjectID == value);
            }
        }

        public abstract ObjectType Race { get; }
        public abstract bool Blocking { get; }

        public uint ObjectID;
        public string Name = string.Empty;
        public Point CurrentLocation, MapLocation;
        public MirDirection Direction;
        public bool Dead, Hidden, SitDown, Sneaking;
        public PoisonType Poison;
        public long DeadTime;
        public uint AI;
        public bool InTrapRock;
        public int JumpDistance;

        public bool Blend = true;

        public long BlindTime;
        public byte BlindCount;
        public uint Health, MaxHealth;
        private byte percentHealth;
        public virtual byte PercentHealth
        {
            get { return percentHealth; }
            set
            {
                if (percentHealth == value) return;

                percentHealth = value;
            }
        }
        public long HealthTime;

        private byte percentMana;
        public virtual byte PercentMana
        {
            get { return percentMana; }
            set
            {
                if (percentMana == value) return;

                percentMana = value;
            }
        }

        public uint LastTargetObjectId => lastTargetObjectId;

        public List<QueuedAction> ActionFeed = new List<QueuedAction>();
        public QueuedAction NextAction
        {
            get { return ActionFeed.Count > 0 ? ActionFeed[0] : null; }
        }

        public List<Effect> Effects = new List<Effect>();
        public List<BuffType> Buffs = new List<BuffType>();

        public MLibrary BodyLibrary;
        public Color DrawColour = Color.White, NameColour = Color.White, LightColour = Color.White;
        public MirLabel NameLabel, ChatLabel, GuildLabel, HealthBarLabel;
        public long ChatTime;
        public int DrawFrame, DrawWingFrame;
        public Point DrawLocation, Movement, FinalDrawLocation, OffSetMove;
        public Rectangle DisplayRectangle;
        public int Light, DrawY;
        public long NextMotion, NextMotion2;
        public MirAction CurrentAction;
        public byte CurrentActionLevel;
        public bool SkipFrames;
        public FrameLoop FrameLoop = null;

        //Sound
        public int StruckWeapon;

        public MirLabel TempLabel;

        public static List<MirLabel> DamageLabelList = new List<MirLabel>();
        public List<Damage> Damages = new List<Damage>();

        protected Point GlobalDisplayLocationOffset
        {
            get { return new Point(0, 0); }
        }

        protected MapObject() { }

        protected MapObject(uint objectID)
        {
            ObjectID = objectID;

            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = MapControl.Objects[i];
                if (ob.ObjectID != ObjectID) continue;
                ob.Remove();
            }

            MapControl.Objects.Add(this);

            RestoreTargetStates();
        }
        public void Remove()
        {
            if (MouseObject == this) MouseObjectID = 0;
            if (TargetObject == this)
            {
                TargetObjectID = 0;
                lastTargetObjectId = ObjectID;
            }
            if (MagicObject == this) MagicObjectID = 0;

            if (this == User.NextMagicObject)
                User.ClearMagic();

            MapControl.Objects.Remove(this);
            GameScene.Scene.MapControl.RemoveObject(this);

            if (ObjectID == Hero?.ObjectID)
                HeroObject = null;

            if (ObjectID != GameScene.NPCID) return;

            GameScene.NPCID = 0;
            GameScene.Scene.NPCDialog.Hide();
        }

        public abstract void Process();
        public abstract void Draw();
        public abstract bool MouseOver(Point p);

        private void RestoreTargetStates()
        {
            if (MouseObjectID == ObjectID)
                MouseObject = this;

            if (TargetObjectID == ObjectID)
                TargetObject = this;

            if (MagicObjectID == ObjectID)
                MagicObject = this;

            if (!this.Dead &&
                TargetObject == null &&
                LastTargetObjectId == ObjectID)
            {
                switch (Race)
                {
                    case ObjectType.Player:
                    case ObjectType.Monster:
                    case ObjectType.Hero:
                        targetObjectID = ObjectID;
                        TargetObject = this;
                        break;
                }
            }
        }

        public void AddBuffEffect(BuffType type)
        {
            for (int i = 0; i < Effects.Count; i++)
            {
                if (!(Effects[i] is BuffEffect)) continue;
                if (((BuffEffect)(Effects[i])).BuffType == type) return;
            }

            PlayerObject ob = null;

            if (Race == ObjectType.Player)
            {
                ob = (PlayerObject)this;
            }

            switch (type)
            {
                case BuffType.血龙剑法:
                    Effects.Add(new BuffEffect(Libraries.Magic3, 190, 7, 1400, this, true, type) { Repeat = true });
                    break;
                case BuffType.天务:
                    Effects.Add(new BuffEffect(Libraries.Magic3, 130, 8, 900, this, true, type) { Repeat = false });
                    SoundManager.PlaySound(20000 + (ushort)Spell.CounterAttack * 10 + 0);
                    Effects.Add(new BuffEffect(Libraries.Magic3, 140, 2, 800, this, true, type) { Repeat = true });
                    break;
                case BuffType.金刚不坏:
                    Effects.Add(new BuffEffect(Libraries.Magic3, 570, 5, 1400, this, true, type) { Repeat = true });
                    break;
                case BuffType.金刚不坏秘籍:
                    Effects.Add(new BuffEffect(Libraries.Magic3, 570, 5, 1400, this, true, type) { Repeat = true });
                    break;
                case BuffType.天上秘术:
                    Effects.Add(new BuffEffect(Libraries.Magic3, 210, 7, 1400, this, true, type) { Repeat = true });
                    break;
                case BuffType.轻身步:
                    if (ob != null) ob.Sprint = true;
                    break;
                case BuffType.月影术:
                case BuffType.烈火身:
                    if (ob != null) ob.Sneaking = true;
                    break;
                case BuffType.吸血地闪:
                    Effects.Add(new BuffEffect(Libraries.Magic3, 2110, 6, 1400, this, true, type) { Repeat = false });
                    break;
                case BuffType.毒魔闪:
                    Effects.Add(new BuffEffect(Libraries.Magic3, 2310, 7, 1400, this, true, type) { Repeat = false });
                    break;
                case BuffType.先天气功:
                    BuffEffect effect;

                    Effects.Add(effect = new BuffEffect(Libraries.Magic2, 1880, 9, 900, this, true, type) { Repeat = false });
                    SoundManager.PlaySound(20000 + (ushort)Spell.EnergyShield * 10 + 0);

                    effect.Complete += (o, e) =>
                    {
                        Effects.Add(new BuffEffect(Libraries.Magic2, 1900, 2, 800, this, true, type) { Repeat = true });
                    };
                    break;
                case BuffType.深延术:
					Effects.Add(new BuffEffect(Libraries.Magic3, 90, 6, 1200, this, true, type) { Repeat = true });
                    break;
                case BuffType.血龙水:
                    Effects.Add(new BuffEffect(Libraries.Magic3, 230, 6, 1200, this, true, type) { Repeat = true });
                    break;
				case BuffType.游戏管理:
					Effects.Add(new BuffEffect(Libraries.CHumEffect[5], 0, 1, 1200, this, true, type) { Repeat = true });
					break;
                case BuffType.华丽雨光:
                    Effects.Add(new BuffEffect(Libraries.Magic3, 3470, 8, 1400, this, true, type) { Repeat = true });
                    break;
                case BuffType.龙之特效:
                    Effects.Add(new BuffEffect(Libraries.Magic3, 7040, 32, 1600, this, true, type, Color.FromArgb(158, 255, 158, 98)) { Blend = true, Repeat = true, Delay = 60000 });
                    Effects.Add(new BuffEffect(Libraries.Magic3, 7080, 25, 1200, this, true, type, Color.FromArgb(158, 255, 255, 255)) { Repeat = true, DrawBehind = true , Delay = 10000 });
                    break;
                case BuffType.龙的特效:
                    Effects.Add(new BuffEffect(Libraries.Magic3, 7040, 32, 1600, this, true, type, Color.FromArgb(158, 98, 255, 255)) { Blend = true, Repeat = true, Delay = 60000 });
                    Effects.Add(new BuffEffect(Libraries.Magic3, 7080, 25, 1200, this, true, type, Color.FromArgb(158, 255, 255, 255)) { Repeat = true, DrawBehind = true, Delay = 10000 });
                    break;
                case BuffType.GeneralMeowMeowShield:
                    Effects.Add(new BuffEffect(Libraries.Monsters[(ushort)Monster.GeneralMeowMeow], 569, 7, 700, this, true, type) { Repeat = true, Light = 1 });
                    SoundManager.PlaySound(8322);
                    break;
                case BuffType.御体之力:
                    Effects.Add(new BuffEffect(Libraries.Monsters[(ushort)Monster.PowerUpBead], 62, 6, 600, this, true, type) { Blend = true, Repeat = true });
                    break;
                case BuffType.HornedArcherBuff:
                    Effects.Add(effect = new BuffEffect(Libraries.Monsters[(ushort)Monster.HornedArcher], 524, 6, 600, this, true, type) { Repeat = false });
                    effect.Complete += (o, e) =>
                    {
                        Effects.Add(new BuffEffect(Libraries.Monsters[(ushort)Monster.HornedArcher], 530, 3, 1000, this, true, type) { Blend = true, Repeat = true });
                    };
                    break;
                case BuffType.ColdArcherBuff:
                    Effects.Add(effect = new BuffEffect(Libraries.Monsters[(ushort)Monster.HornedArcher], 533, 7, 700, this, true, type) { Repeat = false });
                    effect.Complete += (o, e) =>
                    {
                        Effects.Add(new BuffEffect(Libraries.Monsters[(ushort)Monster.HornedArcher], 540, 3, 1000, this, true, type) { Blend = true, Repeat = true });
                    };
                    break;
                case BuffType.HornedColdArcherBuff:
                    Effects.Add(effect = new BuffEffect(Libraries.Monsters[(ushort)Monster.HornedArcher], 543, 7, 700, this, true, type) { Repeat = false });
                    effect.Complete += (o, e) =>
                    {
                        Effects.Add(new BuffEffect(Libraries.Monsters[(ushort)Monster.HornedArcher], 550, 3, 1000, this, true, type) { Blend = true, Repeat = true });
                    };
                    break;
                case BuffType.HornedWarriorShield:   //bBUFF 字段内的 HornedWarriorShield 被注释掉了估计是未使用
                    Effects.Add(effect = new BuffEffect(Libraries.Monsters[(ushort)Monster.HornedWarrior], 960, 18, 1800, this, true, type) { Repeat = true });  //特效资源库，指定为“Monster.HornedWarrior”的动画库         // 起始帧960 // 帧数18 // 持续时间1800 // 作用对象this  // 启用混合true // Buff 类型type  // 循环播放= true
                    effect.Complete += (o, e) =>
                    {
                        Effects.Add(new BuffEffect(Libraries.Monsters[(ushort)Monster.HornedWarrior], 960, 18, 1000, this, true, type) { Blend = true, Repeat = true });
                    };
                    break;
                case BuffType.HornedCommanderShield:
                    Effects.Add(new BuffEffect(Libraries.Monsters[(ushort)Monster.RedFaeFlower], 1334, 7, 1800, this, true, type) { Repeat = true });
                    break;		
                case BuffType.ChieftainSwordBuff:
                    Effects.Add(effect = new BuffEffect(Libraries.Monsters[(ushort)Monster.ChieftainSword], 1331, 6, 100, this, true, type) { Repeat = false, Light = 1 });
                    effect.Complete += (o, e) =>
                    {
                        Effects.Add(new BuffEffect(Libraries.Monsters[(ushort)Monster.ChieftainSword], 1337, 3, 600, this, true, type) { Repeat = true, Light = 1 });
                    };
                    break;
                case BuffType.寒冰护甲:
                    Effects.Add(effect = new BuffEffect(Libraries.Monsters[(ushort)Monster.ShardGuardian], 517, 11, 800, this, true, type) { Repeat = false, Light = 1, DrawBehind = true });
                    effect.Complete += (o, e) =>
                    {
                        Effects.Add(new BuffEffect(Libraries.Monsters[(ushort)Monster.ShardGuardian], 528, 8, 1600, this, true, type) { Repeat = true, Light = 1 });
                    };
                    break;
                case BuffType.ReaperPriestBuff:
                    Effects.Add(effect = new BuffEffect(Libraries.Monsters[(ushort)Monster.ReaperPriest], 497, 16, 800, this, true, type) { Repeat = false, Light = 1, DrawBehind = true });
                    effect.Complete += (o, e) =>
                    {
                        Effects.Add(new BuffEffect(Libraries.Monsters[(ushort)Monster.ReaperPriest], 479, 18, 1600, this, true, type) { Repeat = true, Light = 1 });
                    };
                    break;
                case BuffType.至尊威严:
                    Effects.Add(new BuffEffect(Libraries.Monsters[(ushort)Monster.NobleWarrior], 472, 6, 600, this, true, type) { Repeat = true, Light = 1 });
                    break;
                case BuffType.伤口加深:
                    Effects.Add(new BuffEffect(Libraries.Monsters[(ushort)Monster.NobleAssassin], 544, 6, 600, this, true, type) { Repeat = true, Light = 1 });
                    break;
                case BuffType.死亡印记:
                    Effects.Add(effect = new BuffEffect(Libraries.Monsters[(ushort)Monster.Butcher], 690, 1, 100, this, true, type) { Repeat = false, Light = 1 });
                    effect.Complete += (o, e) =>
                    {
                        Effects.Add(new BuffEffect(Libraries.Monsters[(ushort)Monster.Butcher], 690, 10, 1600, this, true, type) { Repeat = true, Light = 1 });
                    };
                    break;
                case BuffType.RiklebitesShield:
                    Effects.Add(effect = new BuffEffect(Libraries.Monsters[(ushort)Monster.Riklebites], 1084, 3, 500, this, true, type) { Repeat = false, Light = 1 });
                    effect.Complete += (o, e) =>
                    {
                    Effects.Add(new BuffEffect(Libraries.Monsters[(ushort)Monster.Riklebites], 1087, 5, 800, this, true, type) { Repeat = true, Light = 1 });
                    Effects.Add(new BuffEffect(Libraries.Monsters[(ushort)Monster.Riklebites], 1094, 8, 800, this, true, type) { Repeat = true, DrawBehind = true });
                    };
                    break;
                case BuffType.绝对封锁:
                    Effects.Add(new BuffEffect(Libraries.Monsters[(ushort)Monster.Mon562N], 648, 10, 600, this, true, type) { Repeat = true, Light = 1 });
                    break;
                case BuffType.Mon564NSealing:
                    Effects.Add(new BuffEffect(Libraries.Monsters[(ushort)Monster.Mon564N], 1016, 10, 600, this, true, type) { Repeat = true, Light = 1 });
                    break;
                case BuffType.烈火焚烧:
                    Effects.Add(new BuffEffect(Libraries.Monsters[(ushort)Monster.Mon571B], 574, 10, 600, this, true, type) { Repeat = true, Light = 1 });
                    break;
                case BuffType.防御诅咒:
                    Effects.Add(new BuffEffect(Libraries.Monsters[(ushort)Monster.Mon573B], 562, 6, 600, this, true, type) { Repeat = true, Light = 1 });
                    break;
                case BuffType.Mon579BShield:
                    Effects.Add(effect = new BuffEffect(Libraries.Monsters[(ushort)Monster.Mon579B], 572, 5, 500, this, true, type) { Repeat = false, Light = 1 });
                    effect.Complete += (o, e) =>
                    {
                    Effects.Add(new BuffEffect(Libraries.Monsters[(ushort)Monster.Mon579B], 577, 10, 600, this, true, type) { Repeat = true, Light = 1 });
                    };
                    break;
                case BuffType.Mon580BShield:
                    Effects.Add(effect = new BuffEffect(Libraries.Monsters[(ushort)Monster.Mon580B], 858, 6, 500, this, true, type) { Repeat = false, Light = 1 });
                    effect.Complete += (o, e) =>
                    {
                    Effects.Add(new BuffEffect(Libraries.Monsters[(ushort)Monster.Mon580B], 864, 18, 800, this, true, type) { Repeat = true, Light = 1 });
                    };
                    break;
            }
        }
        public void RemoveBuffEffect(BuffType type)
        {
            PlayerObject ob = null;

            if (Race == ObjectType.Player)
            {
                ob = (PlayerObject)this;
            }

            for (int i = 0; i < Effects.Count; i++)
            {
                if (!(Effects[i] is BuffEffect)) continue;
                if (((BuffEffect)(Effects[i])).BuffType != type) continue;
                Effects[i].Repeat = false;
            }

            switch (type)
            {
                case BuffType.轻身步:
                    if (ob != null) ob.Sprint = false;
                    break;
                case BuffType.月影术:
                case BuffType.烈火身:
                    if (ob != null) ob.Sneaking = false;
                    break;
            }
        }

        public Color ApplyDrawColour()
        {
            Color drawColour = DrawColour;
            if (drawColour == Color.Gray)
            {
                drawColour = Color.White;
                DXManager.SetGrayscale(true);
            }
            return drawColour;
        }

        public virtual Missile CreateProjectile(int baseIndex, MLibrary library, bool blend, int count, int interval, int skip, int lightDistance = 6, bool direction16 = true, Color? lightColour = null, uint targetID = 0)
        {
            return null;
        }

        public void Chat(string text)
        {
            if (ChatLabel != null && !ChatLabel.IsDisposed)
            {
                ChatLabel.Dispose();
                ChatLabel = null;
            }

            const int chatWidth = 200;
            List<string> chat = new List<string>();

            int index = 0;
            for (int i = 1; i < text.Length; i++)
                if (TextRenderer.MeasureText(CMain.Graphics, text.Substring(index, i - index), ChatFont).Width > chatWidth)
                {
                    chat.Add(text.Substring(index, i - index - 1));
                    index = i - 1;
                }
            chat.Add(text.Substring(index, text.Length - index));

            text = chat[0];
            for (int i = 1; i < chat.Count; i++)
                text += string.Format("\n{0}", chat[i]);

            ChatLabel = new MirLabel
            {
                AutoSize = true,
                BackColour = Color.Transparent,
                ForeColour = Color.White,
                OutLine = true,
                OutLineColour = Color.Black,
                DrawFormat = TextFormatFlags.HorizontalCenter,
                Text = text,
            };
            ChatTime = CMain.Time + 5000;
        }

        public virtual void DrawHealthNum()
        {
            if (!Settings.ShowHealth)
                return;

            CreateHealthNum();

            if (HealthBarLabel == null || HealthBarLabel.IsDisposed || Dead) return;
            if (MaxHealth == 0)
                return;

            HealthBarLabel.Text = String.Format("{0}/{1}", Health, MaxHealth);
            HealthBarLabel.Location = new Point(DisplayRectangle.X + (48 - HealthBarLabel.Size.Width) / 2, DisplayRectangle.Y - (65 + HealthBarLabel.Size.Height) - (Dead ? 35 : 0));
            HealthBarLabel.Draw();
        }
        public virtual void DrawChat()
        {
            if (ChatLabel == null || ChatLabel.IsDisposed) return;

            if (CMain.Time > ChatTime)
            {
                ChatLabel.Dispose();
                ChatLabel = null;
                return;
            }

            ChatLabel.ForeColour = Dead ? Color.Gray : Color.White;
            ChatLabel.Location = new Point(DisplayRectangle.X + (48 - ChatLabel.Size.Width) / 2, DisplayRectangle.Y - (60 + ChatLabel.Size.Height) - (Dead ? 35 : 0));
            ChatLabel.Draw();
        }

        public virtual void CreateLabel()
        {
            NameLabel = null;

            for (int i = 0; i < LabelList.Count; i++)
            {
                if (LabelList[i].Text != Name || LabelList[i].ForeColour != NameColour) continue;
                NameLabel = LabelList[i];
                break;
            }


            if (NameLabel != null && !NameLabel.IsDisposed) return;

            NameLabel = new MirLabel
            {
                AutoSize = true,
                BackColour = Color.Transparent,
                ForeColour = NameColour,
                OutLine = true,
                OutLineColour = Color.Black,
                Text = Name,
            };
            NameLabel.Disposing += (o, e) => LabelList.Remove(NameLabel);
            LabelList.Add(NameLabel);



        }
        public virtual void DrawName()
        {
            CreateLabel();

            if (NameLabel == null) return;
            
            NameLabel.Text = Name;
            NameLabel.Location = new Point(DisplayRectangle.X + (50 - NameLabel.Size.Width) / 2, DisplayRectangle.Y - (32 - NameLabel.Size.Height / 2) + (Dead ? 35 : 8)); //was 48 -
            NameLabel.Draw();
        }
        public virtual void DrawBlend()
        {
            DXManager.SetBlend(true, 0.3F); //0.8
            Draw();
            DXManager.SetBlend(false);
        }
        public void DrawDamages()
        {
            for (int i = Damages.Count - 1; i >= 0; i--)
            {
                Damage info = Damages[i];
                if (CMain.Time > info.ExpireTime)
                {
                    if (info.DamageLabel != null)
                    {
                        info.DamageLabel.Dispose();
                    }

                    Damages.RemoveAt(i);
                }
                else
                {
                    info.Draw(DisplayRectangle.Location);
                }
            }
        }
        public virtual bool ShouldDrawHealth()
        {
            return false;
        }
        public void DrawHealth()
        {
            string name = Name;            
            if (Name.Contains("(")) name = Name.Substring(Name.IndexOf("(") + 1, Name.Length - Name.IndexOf("(") - 2);

            if (Dead) return;
            if (Race != ObjectType.Player && Race != ObjectType.Monster && Race != ObjectType.Hero) return;

            if (CMain.Time >= HealthTime)
            {
                if (!ShouldDrawHealth()) return;
            }

            Libraries.Prguse2.Draw(0, DisplayRectangle.X + 8, DisplayRectangle.Y - 64);
            int index = 1;

            switch (Race)
            {
                case ObjectType.Player:
                    if (GroupDialog.GroupList.Contains(name)) index = 10;
                    break;
                case ObjectType.Monster:
                    if (GroupDialog.GroupList.Contains(name) || name == User.Name) index = 11;
                    break;
                case ObjectType.Hero:
                    if (GroupDialog.GroupList.Contains(MapObject.HeroObject?.OwnerName)) // Fails but not game breaking
                    {
                            index = 11; 
                    }
                    if (HeroObject.HeroObject?.OwnerName == User.Name)
                    {
                        index = 1; 
                        if ((MapObject.HeroObject.Class != MirClass.战士 && HeroObject.Level > 7) || (MapObject.HeroObject.Class == MirClass.战士 && HeroObject.Level > 25))
                        {
                           Libraries.Prguse2.Draw(10, new Rectangle(0, 0, (int)(32 * PercentMana / 100F), 4), new Point(DisplayRectangle.X + 8, DisplayRectangle.Y - 60), Color.White, false);
                        }
                    }
                    break;
            }

            Libraries.Prguse2.Draw(index, new Rectangle(0, 0, (int)(32 * PercentHealth / 100F), 4), new Point(DisplayRectangle.X + 8, DisplayRectangle.Y - 64), Color.White, false);
        }

        protected void CreateHealthNum()
        {
            if (HealthBarLabel != null) return;

            HealthBarLabel = new MirLabel
            {
                AutoSize = true,
                BackColour = Color.Transparent,
                ForeColour = Color.White,
                OutLine = true,
                OutLineColour = Color.Black,
                DrawFormat = TextFormatFlags.HorizontalCenter,
                Text = "",
            };
        }
        public void DrawPoison()
        {
            byte poisoncount = 0;
            if (Poison != PoisonType.None)
            {
                if (Poison.HasFlag(PoisonType.Green))
                {
                    DXManager.Draw(DXManager.PoisonDotBackground, new Rectangle(0, 0, 6, 6), new Vector3((float)(DisplayRectangle.X + 7 + (poisoncount * 5)), (float)(DisplayRectangle.Y - 21), 0.0F), Color.Black);
                    DXManager.Draw(DXManager.RadarTexture, new Rectangle(0, 0, 4, 4), new Vector3((float)(DisplayRectangle.X + 8 + (poisoncount * 5)), (float)(DisplayRectangle.Y - 20), 0.0F), Color.Green);
                    poisoncount++;
                }
                if (Poison.HasFlag(PoisonType.Red))
                {
                    DXManager.Draw(DXManager.PoisonDotBackground, new Rectangle(0, 0, 6, 6), new Vector3((float)(DisplayRectangle.X + 7 + (poisoncount * 5)), (float)(DisplayRectangle.Y - 21), 0.0F), Color.Black);
                    DXManager.Draw(DXManager.RadarTexture, new Rectangle(0, 0, 4, 4), new Vector3((float)(DisplayRectangle.X + 8 + (poisoncount * 5)), (float)(DisplayRectangle.Y - 20), 0.0F), Color.Red);
                    poisoncount++;
                }
                if (Poison.HasFlag(PoisonType.Bleeding))
                {
                    DXManager.Draw(DXManager.PoisonDotBackground, new Rectangle(0, 0, 6, 6), new Vector3((float)(DisplayRectangle.X + 7 + (poisoncount * 5)), (float)(DisplayRectangle.Y - 21), 0.0F), Color.Black);
                    DXManager.Draw(DXManager.RadarTexture, new Rectangle(0, 0, 4, 4), new Vector3((float)(DisplayRectangle.X + 8 + (poisoncount * 5)), (float)(DisplayRectangle.Y - 20), 0.0F), Color.DarkRed);
                    poisoncount++;
                }
                if (Poison.HasFlag(PoisonType.Slow))
                {
                    DXManager.Draw(DXManager.PoisonDotBackground, new Rectangle(0, 0, 6, 6), new Vector3((float)(DisplayRectangle.X + 7 + (poisoncount * 5)), (float)(DisplayRectangle.Y - 21), 0.0F), Color.Black);
                    DXManager.Draw(DXManager.RadarTexture, new Rectangle(0, 0, 4, 4), new Vector3((float)(DisplayRectangle.X + 8 + (poisoncount * 5)), (float)(DisplayRectangle.Y - 20), 0.0F), Color.Purple);
                    poisoncount++;
                }
                if (Poison.HasFlag(PoisonType.Stun) || Poison.HasFlag(PoisonType.Dazed))
                {
                    DXManager.Draw(DXManager.PoisonDotBackground, new Rectangle(0, 0, 6, 6), new Vector3((float)(DisplayRectangle.X + 7 + (poisoncount * 5)), (float)(DisplayRectangle.Y - 21), 0.0F), Color.Black);
                    DXManager.Draw(DXManager.RadarTexture, new Rectangle(0, 0, 4, 4), new Vector3((float)(DisplayRectangle.X + 8 + (poisoncount * 5)), (float)(DisplayRectangle.Y - 20), 0.0F), Color.Yellow);
                    poisoncount++;
                }
                if (Poison.HasFlag(PoisonType.Blindness))
                {
                    DXManager.Draw(DXManager.PoisonDotBackground, new Rectangle(0, 0, 6, 6), new Vector3((float)(DisplayRectangle.X + 7 + (poisoncount * 5)), (float)(DisplayRectangle.Y - 21), 0.0F), Color.Black);
                    DXManager.Draw(DXManager.RadarTexture, new Rectangle(0, 0, 4, 4), new Vector3((float)(DisplayRectangle.X + 8 + (poisoncount * 5)), (float)(DisplayRectangle.Y - 20), 0.0F), Color.MediumVioletRed);
                    poisoncount++;
                }
                if (Poison.HasFlag(PoisonType.Frozen))
                {
                    DXManager.Draw(DXManager.PoisonDotBackground, new Rectangle(0, 0, 6, 6), new Vector3((float)(DisplayRectangle.X + 7 + (poisoncount * 5)), (float)(DisplayRectangle.Y - 21), 0.0F), Color.Black);
                    DXManager.Draw(DXManager.RadarTexture, new Rectangle(0, 0, 4, 4), new Vector3((float)(DisplayRectangle.X + 8 + (poisoncount * 5)), (float)(DisplayRectangle.Y - 20), 0.0F), Color.Blue);
                    poisoncount++;
                }
                if (Poison.HasFlag(PoisonType.Paralysis) || Poison.HasFlag(PoisonType.LRParalysis))
                {
                    DXManager.Draw(DXManager.PoisonDotBackground, new Rectangle(0, 0, 6, 6), new Vector3((float)(DisplayRectangle.X + 7 + (poisoncount * 5)), (float)(DisplayRectangle.Y - 21), 0.0F), Color.Black);
                    DXManager.Draw(DXManager.RadarTexture, new Rectangle(0, 0, 4, 4), new Vector3((float)(DisplayRectangle.X + 8 + (poisoncount * 5)), (float)(DisplayRectangle.Y - 20), 0.0F), Color.Gray);
                    poisoncount++;
                }
                if (Poison.HasFlag(PoisonType.DelayedExplosion))
                {
                    DXManager.Draw(DXManager.PoisonDotBackground, new Rectangle(0, 0, 6, 6), new Vector3((float)(DisplayRectangle.X + 7 + (poisoncount * 5)), (float)(DisplayRectangle.Y - 21), 0.0F), Color.Black);
                    DXManager.Draw(DXManager.RadarTexture, new Rectangle(0, 0, 4, 4), new Vector3((float)(DisplayRectangle.X + 8 + (poisoncount * 5)), (float)(DisplayRectangle.Y - 20), 0.0F), Color.Orange);
                    poisoncount++;
                }
            }
        }

        public abstract void DrawBehindEffects(bool effectsEnabled);

        public abstract void DrawEffects(bool effectsEnabled);

        protected void LoopFrame(int start, int frameCount, int frameInterval, int duration)
        {
            if (FrameLoop == null)
            {
                FrameLoop = new FrameLoop
                {
                    Start = start,
                    End = start + frameCount - 1,
                    Loops = (duration / (frameInterval * frameCount)) - 1 //Remove 1 count as we've already done a loop before this is checked
                };
            }
        }
    }

    public class FrameLoop
    {
        public MirAction Action { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
        public int Loops { get; set; }

        public int CurrentCount { get; set; }
    }

}
