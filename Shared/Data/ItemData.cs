﻿using System.Text.RegularExpressions;

public class ItemInfo
{
    public int Index;
    public string Name = string.Empty;
    public ItemType Type;
    public ItemGrade Grade;
    public RequiredType RequiredType = RequiredType.Level;
    public RequiredClass RequiredClass = RequiredClass.全职业;
    public RequiredGender RequiredGender = RequiredGender.性别不限;
    public ItemSet Set;

    public short Shape;
    public byte Weight, Light, RequiredAmount;

    public ushort Image, Durability;

    public uint Price; 
    public ushort StackSize = 1;

    public bool StartItem;
    public byte Effect;

    public bool NeedIdentify, ShowGroupPickup, GlobalDropNotify;
    public bool ClassBased;
    public bool LevelBased;
    public bool CanMine;
    public bool CanFastRun;
    public bool CanAwakening;

    public BindMode Bind = BindMode.None;

    public SpecialItemMode Unique = SpecialItemMode.None;
    public byte RandomStatsId;
    public RandomItemStat RandomStats;
    public string ToolTip = string.Empty;

    public byte Slots;

    public Stats Stats;

    public bool IsConsumable
    {
        get { return Type == ItemType.药水 || Type == ItemType.卷轴 || Type == ItemType.坐骑食物 || Type == ItemType.外形物品 || Type == ItemType.特殊消耗品; }
    }
    public bool IsFishingRod
    {
        get { return Globals.FishingRodShapes.Contains(Shape); }
    }

    public string FriendlyName
    {
        get
        {
            string temp = Name;
            temp = Regex.Replace(temp, @"\d+$", string.Empty); //hides end numbers
            temp = Regex.Replace(temp, @"\[[^]]*\]", string.Empty); //hides square brackets

            return temp;
        }
    }

    public ItemInfo() 
    {
        Stats = new Stats();
    }

    public ItemInfo(BinaryReader reader, int version = int.MaxValue, int customVersion = int.MaxValue)
    {
        Index = reader.ReadInt32();
        Name = reader.ReadString();
        Type = (ItemType)reader.ReadByte();
        Grade = (ItemGrade)reader.ReadByte();
        RequiredType = (RequiredType)reader.ReadByte();
        RequiredClass = (RequiredClass)reader.ReadByte();
        RequiredGender = (RequiredGender)reader.ReadByte();
        Set = (ItemSet)reader.ReadByte();

        Shape = reader.ReadInt16();
        Weight = reader.ReadByte();
        Light = reader.ReadByte();
        RequiredAmount = reader.ReadByte();

        Image = reader.ReadUInt16();
        Durability = reader.ReadUInt16();

        if (version <= 84)
        {
            StackSize = (ushort)reader.ReadUInt32();
        }
        else
        {
            StackSize = reader.ReadUInt16();
        }

        Price = reader.ReadUInt32();

        if (version <= 84)
        {
            Stats = new Stats();
            Stats[Stat.最小防御] = reader.ReadByte();
            Stats[Stat.最大防御] = reader.ReadByte();
            Stats[Stat.最小魔御] = reader.ReadByte();
            Stats[Stat.最大魔御] = reader.ReadByte();
            Stats[Stat.最小攻击] = reader.ReadByte();
            Stats[Stat.最大攻击] = reader.ReadByte();
            Stats[Stat.最小魔法] = reader.ReadByte();
            Stats[Stat.最大魔法] = reader.ReadByte();
            Stats[Stat.最小道术] = reader.ReadByte();
            Stats[Stat.最大道术] = reader.ReadByte();
            Stats[Stat.HP] = reader.ReadUInt16();
            Stats[Stat.MP] = reader.ReadUInt16();
            Stats[Stat.准确] = reader.ReadByte();
            Stats[Stat.敏捷] = reader.ReadByte();

            Stats[Stat.幸运] = reader.ReadSByte();
            Stats[Stat.攻击速度] = reader.ReadSByte();
        }

        StartItem = reader.ReadBoolean();

        if (version <= 84)
        {
            Stats[Stat.背包负重] = reader.ReadByte();
            Stats[Stat.手腕负重] = reader.ReadByte();
            Stats[Stat.佩戴负重] = reader.ReadByte();
        }

        Effect = reader.ReadByte();

        if (version <= 84)
        {
            Stats[Stat.强度] = reader.ReadByte();
            Stats[Stat.魔法躲避] = reader.ReadByte();
            Stats[Stat.毒药抵抗] = reader.ReadByte();
            Stats[Stat.体力恢复] = reader.ReadByte();
            Stats[Stat.法力恢复] = reader.ReadByte();
            Stats[Stat.中毒恢复] = reader.ReadByte();
            Stats[Stat.HPRatePercent] = reader.ReadByte();
            Stats[Stat.MPRatePercent] = reader.ReadByte();
            Stats[Stat.暴击率] = reader.ReadByte();
            Stats[Stat.暴击伤害] = reader.ReadByte();
        }


        byte bools = reader.ReadByte();
        NeedIdentify = (bools & 0x01) == 0x01;
        ShowGroupPickup = (bools & 0x02) == 0x02;
        ClassBased = (bools & 0x04) == 0x04;
        LevelBased = (bools & 0x08) == 0x08;
        CanMine = (bools & 0x10) == 0x10;

        if (version >= 77)
        {
            GlobalDropNotify = (bools & 0x20) == 0x20;
        }

        if (version <= 84)
        {
            Stats[Stat.MaxACRatePercent] = reader.ReadByte();
            Stats[Stat.MaxMACRatePercent] = reader.ReadByte();
            Stats[Stat.神圣] = reader.ReadByte();
            Stats[Stat.冰冻] = reader.ReadByte();
            Stats[Stat.毒攻] = reader.ReadByte();
        }

        Bind = (BindMode)reader.ReadInt16();

        if (version <= 84)
        {
            Stats[Stat.Reflect] = reader.ReadByte();
            Stats[Stat.吸血] = reader.ReadByte();
            Stats[Stat.星星等级] = reader.ReadByte();
            Stats[Stat.词条1] = reader.ReadByte();
            Stats[Stat.词条2] = reader.ReadByte();
            Stats[Stat.词条3] = reader.ReadByte();
        }

        Unique = (SpecialItemMode)reader.ReadInt16();
        RandomStatsId = reader.ReadByte();

        CanFastRun = reader.ReadBoolean();

        CanAwakening = reader.ReadBoolean();

        if (version > 83)
        {
            Slots = reader.ReadByte();
        }

        if (version > 84)
        {
            Stats = new Stats(reader, version, customVersion);
        }

        bool isTooltip = reader.ReadBoolean();
        if (isTooltip)
        {
            ToolTip = reader.ReadString();
        }

        if (version < 70) //在db70版本之前，所有特殊物品都禁用了结婚戒指，之后增加它成为服务器自行设置选项
        {
            if ((Type == ItemType.戒指) && (Unique != SpecialItemMode.None))
                Bind |= BindMode.NoWeddingRing;
        }
    }



    public void Save(BinaryWriter writer)
    {
        writer.Write(Index);
        writer.Write(Name);
        writer.Write((byte)Type);
        writer.Write((byte)Grade);
        writer.Write((byte)RequiredType);
        writer.Write((byte)RequiredClass);
        writer.Write((byte)RequiredGender);
        writer.Write((byte)Set);

        writer.Write(Shape);
        writer.Write(Weight);
        writer.Write(Light);
        writer.Write(RequiredAmount);

        writer.Write(Image);
        writer.Write(Durability);

        writer.Write(StackSize);
        writer.Write(Price);

        writer.Write(StartItem);

        writer.Write(Effect);

        byte bools = 0;
        if (NeedIdentify) bools |= 0x01;
        if (ShowGroupPickup) bools |= 0x02;
        if (ClassBased) bools |= 0x04;
        if (LevelBased) bools |= 0x08;
        if (CanMine) bools |= 0x10;
        if (GlobalDropNotify) bools |= 0x20;
        writer.Write(bools);
        
        writer.Write((short)Bind);        
        writer.Write((short)Unique);

        writer.Write(RandomStatsId);

        writer.Write(CanFastRun);
        writer.Write(CanAwakening);
        writer.Write(Slots);

        Stats.Save(writer);

        writer.Write(ToolTip != null);
        if (ToolTip != null)
            writer.Write(ToolTip);

    }

    public static ItemInfo FromText(string text)
    {
        return null;
    }

    public string ToText()
    {
        return null;
    }

    public override string ToString()
    {
        return string.Format("{0}: {1}", Index, Name);
    }

}

public class UserItem
{
    public ulong UniqueID;
    public int ItemIndex;

    public ItemInfo Info;
    public ushort CurrentDura, MaxDura;
    public ushort Count = 1,
                GemCount = 0;

    public RefinedValue RefinedValue = RefinedValue.None;
    public byte RefineAdded = 0;
    public int RefineSuccessChance = 0;

    public bool DuraChanged;
    public int SoulBoundId = -1;
    public bool Identified = false;
    public bool Cursed = false;

    public int WeddingRing = -1;

    public UserItem[] Slots = new UserItem[5];

    public DateTime BuybackExpiryDate;

    public ExpireInfo ExpireInfo;
    public RentalInformation RentalInformation;
    public SealedInfo SealedInfo;

    public bool IsShopItem;

    public Awake Awake = new Awake();

    public Stats AddedStats;

    public bool IsAdded
    {
        get { return AddedStats.Count > 0 || Slots.Length > Info.Slots; }
    }

    public int Weight
    {
        get { return (Info.Type == ItemType.护身符 || Info.Type == ItemType.鱼饵) ? Info.Weight : Info.Weight * Count; }
    }

    public string FriendlyName
    {
        get { return Count > 1 ? string.Format("{0} ({1})", Info.FriendlyName, Count) : Info.FriendlyName; }
    }

    public bool GMMade { get; set; }

    public UserItem(ItemInfo info)
    {
        SoulBoundId = -1;
        ItemIndex = info.Index;
        Info = info;
        AddedStats = new Stats();

        SetSlotSize();
    }
    public UserItem(BinaryReader reader, int version = int.MaxValue, int customVersion = int.MaxValue)
    {
        UniqueID = reader.ReadUInt64();
        ItemIndex = reader.ReadInt32();

        CurrentDura = reader.ReadUInt16();
        MaxDura = reader.ReadUInt16();

        if (version <= 84)
        {
            Count = (ushort)reader.ReadUInt32();
        }
        else
        {
            Count = reader.ReadUInt16();
        }

        if (version <= 84)
        {
            AddedStats = new Stats();

            AddedStats[Stat.最大防御] = reader.ReadByte();
            AddedStats[Stat.最大魔御] = reader.ReadByte();
            AddedStats[Stat.最大攻击] = reader.ReadByte();
            AddedStats[Stat.最大魔法] = reader.ReadByte();
            AddedStats[Stat.最大道术] = reader.ReadByte();

            AddedStats[Stat.准确] = reader.ReadByte();
            AddedStats[Stat.敏捷] = reader.ReadByte();
            AddedStats[Stat.HP] = reader.ReadByte();
            AddedStats[Stat.MP] = reader.ReadByte();

            AddedStats[Stat.攻击速度] = reader.ReadSByte();
            AddedStats[Stat.幸运] = reader.ReadSByte();
        }

        SoulBoundId = reader.ReadInt32();
        byte Bools = reader.ReadByte();
        Identified = (Bools & 0x01) == 0x01;
        Cursed = (Bools & 0x02) == 0x02;

        if (version <= 84)
        {
            AddedStats[Stat.强度] = reader.ReadByte();
            AddedStats[Stat.魔法躲避] = reader.ReadByte();
            AddedStats[Stat.毒药抵抗] = reader.ReadByte();
            AddedStats[Stat.体力恢复] = reader.ReadByte();
            AddedStats[Stat.法力恢复] = reader.ReadByte();
            AddedStats[Stat.中毒恢复] = reader.ReadByte();
            AddedStats[Stat.暴击率] = reader.ReadByte();
            AddedStats[Stat.暴击伤害] = reader.ReadByte();
            AddedStats[Stat.冰冻] = reader.ReadByte();
            AddedStats[Stat.毒攻] = reader.ReadByte();
        }

        int count = reader.ReadInt32();

        SetSlotSize(count);

        for (int i = 0; i < count; i++)
        {
            if (reader.ReadBoolean()) continue;
            UserItem item = new UserItem(reader, version, customVersion);
            Slots[i] = item;
        }

        if (version <= 84)
        {
            GemCount = (ushort)reader.ReadUInt32();
        }
        else
        {
            GemCount = reader.ReadUInt16();
        }

        if (version > 84)
        {
            AddedStats = new Stats(reader, version, customVersion);
        }

        Awake = new Awake(reader);

        RefinedValue = (RefinedValue)reader.ReadByte();
        RefineAdded = reader.ReadByte();

        if (version > 85)
        {
            RefineSuccessChance = reader.ReadInt32();
        }

        WeddingRing = reader.ReadInt32();

        if (version < 65) return;

        if (reader.ReadBoolean())
        {
            ExpireInfo = new ExpireInfo(reader, version, customVersion);
        }

        if (version < 76)
            return;

        if (reader.ReadBoolean())
            RentalInformation = new RentalInformation(reader, version, customVersion);

        if (version < 83) return;

        IsShopItem = reader.ReadBoolean();

        if (version < 92) return;

        if (reader.ReadBoolean())
        {
            SealedInfo = new SealedInfo(reader, version, customVersion);
        }

        if (version > 107)
        {
            GMMade = reader.ReadBoolean();
        }
    }

    public void Save(BinaryWriter writer)
    {
        writer.Write(UniqueID);
        writer.Write(ItemIndex);

        writer.Write(CurrentDura);
        writer.Write(MaxDura);

        writer.Write(Count);
       
        writer.Write(SoulBoundId);
        byte Bools = 0;
        if (Identified) Bools |= 0x01;
        if (Cursed) Bools |= 0x02;
        writer.Write(Bools);

        writer.Write(Slots.Length);
        for (int i = 0; i < Slots.Length; i++)
        {
            writer.Write(Slots[i] == null);
            if (Slots[i] == null) continue;

            Slots[i].Save(writer);
        }

        writer.Write(GemCount);


        AddedStats.Save(writer);
        Awake.Save(writer);

        writer.Write((byte)RefinedValue);
        writer.Write(RefineAdded);
        writer.Write(RefineSuccessChance);

        writer.Write(WeddingRing);

        writer.Write(ExpireInfo != null);
        ExpireInfo?.Save(writer);

        writer.Write(RentalInformation != null);
        RentalInformation?.Save(writer);

        writer.Write(IsShopItem);

        writer.Write(SealedInfo != null);
        SealedInfo?.Save(writer);

        writer.Write(GMMade);
    }

    public int GetTotal(Stat type)
    {
        return AddedStats[type] + Info.Stats[type];
    }

    public uint Price()
    {
        if (Info == null) return 0;

        uint p = Info.Price;


        if (Info.Durability > 0)
        {
            float r = ((Info.Price / 2F) / Info.Durability);

            p = (uint)(MaxDura * r);

            if (MaxDura > 0)
                r = CurrentDura / (float)MaxDura;
            else
                r = 0;

            p = (uint)Math.Floor(p / 2F + ((p / 2F) * r) + Info.Price / 2F);
        }


        p = (uint)(p * (AddedStats.Count * 0.1F + 1F));


        return p * Count;
    }
    public uint RepairPrice()
    {
        if (Info == null || Info.Durability == 0)
            return 0;

        var p = Info.Price;

        if (Info.Durability > 0)
        {
            p = (uint)Math.Floor(MaxDura * ((Info.Price / 2F) / Info.Durability) + Info.Price / 2F);
            p = (uint)(p * (AddedStats.Count * 0.1F + 1F));

        }

        var cost = p * Count - Price();

        if (RentalInformation == null)
            return cost;

        return cost * 2;
    }

    public uint Quality()
    {
        uint q = (uint)(AddedStats.Count + Awake.GetAwakeLevel() + 1);

        return q;
    }

    public uint AwakeningPrice()
    {
        if (Info == null) return 0;

        uint p = 1500;

        p = (uint)((p * (1 + Awake.GetAwakeLevel() * 2)) * (uint)Info.Grade);

        return p;
    }

    public uint DisassemblePrice()
    {
        if (Info == null) return 0;

        uint p = 1500 * (uint)Info.Grade;

        p = (uint)(p * ((AddedStats.Count + Awake.GetAwakeLevel()) * 0.1F + 1F));

        return p;
    }

    public uint DowngradePrice()
    {
        if (Info == null) return 0;

        uint p = 3000;

        p = (uint)((p * (1 + (Awake.GetAwakeLevel() + 1) * 2)) * (uint)Info.Grade);

        return p;
    }

    public uint ResetPrice()
    {
        if (Info == null) return 0;

        uint p = 3000 * (uint)Info.Grade;

        p = (uint)(p * (AddedStats.Count * 0.2F + 1F));

        return p;
    }
    public void SetSlotSize(int? size = null)
    {
        if (size == null)
        {
            switch (Info.Type)
            {
                case ItemType.坐骑:
                    if (Info.Shape < 7)
                        size = 4;
                    else if (Info.Shape < 13)
                        size = 5;
                    break;
                case ItemType.武器:
                    if (Info.Shape == 49 || Info.Shape == 50)
                        size = 5;
                    break;
            }
        }

        if (size == null && Info == null) return;
        if (size != null && size == Slots.Length) return;
        if (size == null && Info != null && Info.Slots == Slots.Length) return;

        Array.Resize(ref Slots, size ?? Info.Slots);
    }

    public ushort Image
    {
        get
        {
            switch (Info.Type)
            {
                #region Amulet and Poison Stack Image changes
                case ItemType.护身符:
                    if (Info.StackSize > 0)
                    {
                        switch (Info.Shape)
                        {
                            case 0: //Amulet
                                if (Count >= 300) return 3662;
                                if (Count >= 200) return 3661;
                                if (Count >= 100) return 3660;
                                return 3660;
                            case 1: //Grey Poison
                                if (Count >= 150) return 3675;
                                if (Count >= 100) return 2960;
                                if (Count >= 50) return 3674;
                                return 3673;
                            case 2: //Yellow Poison
                                if (Count >= 150) return 3672;
                                if (Count >= 100) return 2961;
                                if (Count >= 50) return 3671;
                                return 3670;
                        }
                    }
                    break;
            }

            #endregion

            return Info.Image;
        }
    }

    public UserItem Clone()
    {
        UserItem item = new UserItem(Info)
        {
            UniqueID = UniqueID,
            CurrentDura = CurrentDura,
            MaxDura = MaxDura,
            Count = Count,
            GemCount = GemCount,
            DuraChanged = DuraChanged,
            SoulBoundId = SoulBoundId,
            Identified = Identified,
            Cursed = Cursed,
            Slots = Slots,
            AddedStats = new Stats(AddedStats),
            Awake = Awake,

            RefineAdded = RefineAdded,

            ExpireInfo = ExpireInfo,
            RentalInformation = RentalInformation,
            SealedInfo = SealedInfo,

            IsShopItem = IsShopItem,
            GMMade = GMMade
        };

        return item;
    }

}

public class ExpireInfo
{
    public DateTime ExpiryDate;

    public ExpireInfo() { }

    public ExpireInfo(BinaryReader reader, int version = int.MaxValue, int Customversion = int.MaxValue)
    {
        ExpiryDate = DateTime.FromBinary(reader.ReadInt64());
    }

    public void Save(BinaryWriter writer)
    {
        writer.Write(ExpiryDate.ToBinary());
    }
}

public class SealedInfo
{
    public DateTime ExpiryDate;
    public DateTime NextSealDate;

    public SealedInfo() { }

    public SealedInfo(BinaryReader reader, int version = int.MaxValue, int Customversion = int.MaxValue)
    {
        ExpiryDate = DateTime.FromBinary(reader.ReadInt64());

        if (version > 92)
        {
            NextSealDate = DateTime.FromBinary(reader.ReadInt64());
        }
    }

    public void Save(BinaryWriter writer)
    {
        writer.Write(ExpiryDate.ToBinary());
        writer.Write(NextSealDate.ToBinary());
    }
}

public class RentalInformation
{
    public string OwnerName;
    public BindMode BindingFlags = BindMode.None;
    public DateTime ExpiryDate;
    public bool RentalLocked;

    public RentalInformation() { }

    public RentalInformation(BinaryReader reader, int version = int.MaxValue, int CustomVersion = int.MaxValue)
    {
        OwnerName = reader.ReadString();
        BindingFlags = (BindMode)reader.ReadInt16();
        ExpiryDate = DateTime.FromBinary(reader.ReadInt64());
        RentalLocked = reader.ReadBoolean();
    }

    public void Save(BinaryWriter writer)
    {
        writer.Write(OwnerName);
        writer.Write((short)BindingFlags);
        writer.Write(ExpiryDate.ToBinary());
        writer.Write(RentalLocked);
    }
}

public class GameShopItem
{
    public int ItemIndex;
    public int GIndex;
    public ItemInfo Info;
    public uint GoldPrice = 0;
    public uint CreditPrice = 0;
    public ushort Count = 1;
    public string Class = "";
    public string Category = "";
    public int Stock = 0;
    public bool iStock = false;
    public bool Deal = false;
    public bool TopItem = false;
    public DateTime Date;
    public bool CanBuyGold = false;
    public bool CanBuyCredit = false;

    public GameShopItem()
    {
    }

    public GameShopItem(BinaryReader reader, int version = int.MaxValue, int Customversion = int.MaxValue)
    {
        ItemIndex = reader.ReadInt32();
        GIndex = reader.ReadInt32();
        GoldPrice = reader.ReadUInt32();
        CreditPrice = reader.ReadUInt32();
        if (version <= 84)
        {
            Count = (ushort)reader.ReadUInt32();
        }
        else
        {
            Count = reader.ReadUInt16();
        }
        Class = reader.ReadString();
        Category = reader.ReadString();
        Stock = reader.ReadInt32();
        iStock = reader.ReadBoolean();
        Deal = reader.ReadBoolean();
        TopItem = reader.ReadBoolean();
        Date = DateTime.FromBinary(reader.ReadInt64());
        if (version > 105)
        {
            CanBuyGold = reader.ReadBoolean();
            CanBuyCredit = reader.ReadBoolean();
        }

    }

    public GameShopItem(BinaryReader reader, bool packet = false)
    {
        ItemIndex = reader.ReadInt32();
        GIndex = reader.ReadInt32();
        Info = new ItemInfo(reader);
        GoldPrice = reader.ReadUInt32();
        CreditPrice = reader.ReadUInt32();
        Count = reader.ReadUInt16();
        Class = reader.ReadString();
        Category = reader.ReadString();
        Stock = reader.ReadInt32();
        iStock = reader.ReadBoolean();
        Deal = reader.ReadBoolean();
        TopItem = reader.ReadBoolean();
        Date = DateTime.FromBinary(reader.ReadInt64());
        CanBuyCredit = reader.ReadBoolean();
        CanBuyGold = reader.ReadBoolean();
    }

    public void Save(BinaryWriter writer, bool packet = false)
    {
        writer.Write(ItemIndex);
        writer.Write(GIndex);
        if (packet) Info.Save(writer);
        writer.Write(GoldPrice);
        writer.Write(CreditPrice);
        writer.Write(Count);
        writer.Write(Class);
        writer.Write(Category);
        writer.Write(Stock);
        writer.Write(iStock);
        writer.Write(Deal);
        writer.Write(TopItem);
        writer.Write(Date.ToBinary());
        writer.Write(CanBuyCredit);
        writer.Write(CanBuyGold);
    }

    public override string ToString()
    {
        return string.Format("{0}: {1}", GIndex, Info.Name);
    }

}

public class Awake
{
    //Awake Option
    public static byte AwakeSuccessRate = 70;
    public static byte AwakeHitRate = 70;
    public static int MaxAwakeLevel = 5;
    public static byte Awake_WeaponRate = 1;
    public static byte Awake_HelmetRate = 1;
    public static byte Awake_ArmorRate = 5;
    public static byte AwakeChanceMin = 1;
    public static float[] AwakeMaterialRate = new float[4] { 1.0F, 1.0F, 1.0F, 1.0F };
    public static byte[] AwakeChanceMax = new byte[4] { 1, 2, 3, 4 };
    public static List<List<byte>[]> AwakeMaterials = new List<List<byte>[]>();

    public AwakeType Type = AwakeType.None;
    readonly List<byte> listAwake = new List<byte>();

    public Awake() { }

    public Awake(BinaryReader reader)
    {
        Type = (AwakeType)reader.ReadByte();
        int count = reader.ReadInt32();
        for (int i = 0; i < count; i++)
        {
            listAwake.Add(reader.ReadByte());
        }
    }

    public void Save(BinaryWriter writer)
    {
        writer.Write((byte)Type);
        writer.Write(listAwake.Count);
        foreach (byte value in listAwake)
        {
            writer.Write(value);
        }
    }
    public bool IsMaxLevel() { return listAwake.Count == Awake.MaxAwakeLevel; }

    public int GetAwakeLevel() { return listAwake.Count; }

    public byte GetAwakeValue()
    {
        byte total = 0;

        foreach (byte value in listAwake)
        {
            total += value;
        }

        return total;
    }

    public bool CheckAwakening(UserItem item, AwakeType type)
    {
        if (item.Info.Bind.HasFlag(BindMode.DontUpgrade))
            return false;

        if (item.Info.CanAwakening != true)
            return false;

        if (item.Info.Grade == ItemGrade.None)
            return false;

        if (IsMaxLevel()) return false;

        if (this.Type == AwakeType.None)
        {
            if (item.Info.Type == ItemType.武器)
            {
                if (type == AwakeType.物理攻击 ||
                    type == AwakeType.魔法攻击 ||
                    type == AwakeType.道术攻击)
                {
                    this.Type = type;
                    return true;
                }
                else
                    return false;
            }
            else if (item.Info.Type == ItemType.头盔)
            {
                if (type == AwakeType.物理防御 ||
                    type == AwakeType.魔法防御)
                {
                    this.Type = type;
                    return true;
                }
                else
                    return false;
            }
            else if (item.Info.Type == ItemType.盔甲)
            {
                if (type == AwakeType.生命法力值)
                {
                    this.Type = type;
                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }
        else
        {
            if (this.Type == type)
                return true;
            else
                return false;
        }
    }

    public int UpgradeAwake(UserItem item, AwakeType type, out bool[] isHit)
    {
        //return -1 condition error, -1 = dont upgrade, 0 = failed, 1 = Succeed,  
        isHit = null;
        if (CheckAwakening(item, type) != true)
            return -1;

        Random rand = new Random(DateTime.Now.Millisecond);

        if (rand.Next(0, 100) <= AwakeSuccessRate)
        {
            isHit = Awakening(item);
            return 1;
        }
        else
        {
            isHit = MakeHit(1, out _);
            return 0;
        }
    }

    public int RemoveAwake()
    {
        if (listAwake.Count > 0)
        {
            listAwake.Remove(listAwake[listAwake.Count - 1]);

            if (listAwake.Count == 0)
                Type = AwakeType.None;

            return 1;
        }
        else
        {
            Type = AwakeType.None;
            return 0;
        }
    }

    public int GetAwakeLevelValue(int i) { return listAwake[i]; }

    public byte GetDC() { return (Type == AwakeType.物理攻击 ? GetAwakeValue() : (byte)0); }
    public byte GetMC() { return (Type == AwakeType.魔法攻击 ? GetAwakeValue() : (byte)0); }
    public byte GetSC() { return (Type == AwakeType.道术攻击 ? GetAwakeValue() : (byte)0); }
    public byte GetAC() { return (Type == AwakeType.物理防御 ? GetAwakeValue() : (byte)0); }
    public byte GetMAC() { return (Type == AwakeType.魔法防御 ? GetAwakeValue() : (byte)0); }
    public byte GetHPMP() { return (Type == AwakeType.生命法力值 ? GetAwakeValue() : (byte)0); }

    private bool[] MakeHit(int maxValue, out int makeValue)
    {
        float stepValue = (float)maxValue / 5.0f;
        float totalValue = 0.0f;
        bool[] isHit = new bool[5];
        Random rand = new Random(DateTime.Now.Millisecond);

        for (int i = 0; i < 5; i++)
        {
            if (rand.Next(0, 100) < AwakeHitRate)
            {
                totalValue += stepValue;
                isHit[i] = true;
            }
            else
            {
                isHit[i] = false;
            }
        }

        makeValue = totalValue <= 1.0f ? 1 : (int)totalValue;
        return isHit;
    }

    private bool[] Awakening(UserItem item)
    {
        int minValue = AwakeChanceMin;
        int maxValue = (AwakeChanceMax[(int)item.Info.Grade - 1] < minValue) ? minValue : AwakeChanceMax[(int)item.Info.Grade - 1];

        bool[] returnValue = MakeHit(maxValue, out int result);

        switch (item.Info.Type)
        {
            case ItemType.武器:
                result *= (int)Awake_WeaponRate;
                break;
            case ItemType.盔甲:
                result *= (int)Awake_ArmorRate;
                break;
            case ItemType.头盔:
                result *= (int)Awake_HelmetRate;
                break;
            default:
                result = 0;
                break;
        }

        listAwake.Add((byte)result);

        return returnValue;
    }
}


public class ItemRentalInformation
{
    public ulong ItemId;
    public string ItemName;
    public string RentingPlayerName;
    public DateTime ItemReturnDate;

    public ItemRentalInformation() { }

    public ItemRentalInformation(BinaryReader reader, int version = int.MaxValue, int customVersion = int.MaxValue)
    {
        ItemId = reader.ReadUInt64();
        ItemName = reader.ReadString();
        RentingPlayerName = reader.ReadString();
        ItemReturnDate = DateTime.FromBinary(reader.ReadInt64());
    }

    public void Save(BinaryWriter writer)
    {
        writer.Write(ItemId);
        writer.Write(ItemName);
        writer.Write(RentingPlayerName);
        writer.Write(ItemReturnDate.ToBinary());
    }
}


public class ItemSets
{
    public ItemSet Set;
    public List<ItemType> Type;
    private byte Amount
    {
        get
        {
            switch (Set)
            {
                case ItemSet.世轮套装:
                case ItemSet.绿翠套装:
                case ItemSet.道护套装:
                case ItemSet.贵人战套:
                case ItemSet.贵人法套:
                case ItemSet.贵人道套:
                case ItemSet.贵人刺套:
                case ItemSet.贵人弓套:
                    return 2;
                case ItemSet.赤兰套装:
                case ItemSet.密火套装:
                case ItemSet.破碎套装:
                case ItemSet.幻魔石套:
                case ItemSet.灵玉套装:
                case ItemSet.五玄套装:
                case ItemSet.白骨套装:
                case ItemSet.虫血套装:
                case ItemSet.鏃未套装:
                    return 3;
                case ItemSet.记忆套装:
                case ItemSet.神龙套装:
                    return 4;
                case ItemSet.祈祷套装:
                case ItemSet.白金套装:
                case ItemSet.强白金套:
                case ItemSet.红玉套装:
                case ItemSet.强红玉套:
                case ItemSet.软玉套装:
                case ItemSet.强软玉套:
                case ItemSet.龙血套装:
                case ItemSet.监视套装:
                case ItemSet.暴压套装:
                case ItemSet.贝玉套装:
                case ItemSet.黑术套装:
                case ItemSet.强青玉套:
                case ItemSet.青玉套装:
                case ItemSet.圣龙套装:
                    return 5;
                case ItemSet.天龙套装:
                    return 8;
                default:
                    return 0;
            }
        }
    }
    public byte Count;
    public bool SetComplete
    {
        get
        {
            return Count >= Amount;
        }
    }
}


public class RandomItemStat
{
    public byte MaxDuraChance, MaxDuraStatChance, MaxDuraMaxStat;
    public byte MaxAcChance, MaxAcStatChance, MaxAcMaxStat, MaxMacChance, MaxMacStatChance, MaxMacMaxStat, MaxDcChance, MaxDcStatChance, MaxDcMaxStat, MaxMcChance, MaxMcStatChance, MaxMcMaxStat, MaxScChance, MaxScStatChance, MaxScMaxStat;
    public byte AccuracyChance, AccuracyStatChance, AccuracyMaxStat, AgilityChance, AgilityStatChance, AgilityMaxStat, HpChance, HpStatChance, HpMaxStat, MpChance, MpStatChance, MpMaxStat, StrongChance, StrongStatChance, StrongMaxStat;
    public byte MagicResistChance, MagicResistStatChance, MagicResistMaxStat, PoisonResistChance, PoisonResistStatChance, PoisonResistMaxStat;
    public byte HpRecovChance, HpRecovStatChance, HpRecovMaxStat, MpRecovChance, MpRecovStatChance, MpRecovMaxStat, PoisonRecovChance, PoisonRecovStatChance, PoisonRecovMaxStat;
    public byte CriticalRateChance, CriticalRateStatChance, CriticalRateMaxStat, CriticalDamageChance, CriticalDamageStatChance, CriticalDamageMaxStat;
    public byte FreezeChance, FreezeStatChance, FreezeMaxStat, PoisonAttackChance, PoisonAttackStatChance, PoisonAttackMaxStat;
    public byte AttackSpeedChance, AttackSpeedStatChance, AttackSpeedMaxStat, LuckChance, LuckStatChance, LuckMaxStat;
    public byte CurseChance;
    public byte SlotChance, SlotStatChance, SlotMaxStat;

    public RandomItemStat(ItemType Type = ItemType.技能书)
    {
        switch (Type)
        {
            case ItemType.武器:
                SetWeapon();
                break;
            case ItemType.盔甲:
                SetArmour();
                break;
            case ItemType.头盔:
                SetHelmet();
                break;
            case ItemType.腰带:
            case ItemType.靴子:
                SetBeltBoots();
                break;
            case ItemType.项链:
                SetNecklace();
                break;
            case ItemType.手镯:
                SetBracelet();
                break;
            case ItemType.戒指:
                SetRing();
                break;
            case ItemType.坐骑:
                SetMount();
                break;
        }
    }

    public void SetWeapon()
    {
        MaxDuraChance = 2;
        MaxDuraStatChance = 13;
        MaxDuraMaxStat = 13;

        MaxDcChance = 15;
        MaxDcStatChance = 15;
        MaxDcMaxStat = 13;

        MaxMcChance = 20;
        MaxMcStatChance = 15;
        MaxMcMaxStat = 13;

        MaxScChance = 20;
        MaxScStatChance = 15;
        MaxScMaxStat = 13;

        AttackSpeedChance = 60;
        AttackSpeedStatChance = 30;
        AttackSpeedMaxStat = 3;

        StrongChance = 24;
        StrongStatChance = 20;
        StrongMaxStat = 2;

        AccuracyChance = 30;
        AccuracyStatChance = 20;
        AccuracyMaxStat = 2;

        SlotChance = 0;
        SlotStatChance = 0;
        SlotMaxStat = 4;
    }
    public void SetArmour()
    {
        MaxDuraChance = 2;
        MaxDuraStatChance = 10;
        MaxDuraMaxStat = 3;

        MaxAcChance = 30;
        MaxAcStatChance = 15;
        MaxAcMaxStat = 7;

        MaxMacChance = 30;
        MaxMacStatChance = 15;
        MaxMacMaxStat = 7;

        MaxDcChance = 40;
        MaxDcStatChance = 20;
        MaxDcMaxStat = 7;

        MaxMcChance = 40;
        MaxMcStatChance = 20;
        MaxMcMaxStat = 7;

        MaxScChance = 40;
        MaxScStatChance = 20;
        MaxScMaxStat = 7;

    }
    public void SetHelmet()
    {
        MaxDuraChance = 2;
        MaxDuraStatChance = 10;
        MaxDuraMaxStat = 3;

        MaxAcChance = 30;
        MaxAcStatChance = 15;
        MaxAcMaxStat = 7;

        MaxMacChance = 30;
        MaxMacStatChance = 15;
        MaxMacMaxStat = 7;

        MaxDcChance = 40;
        MaxDcStatChance = 20;
        MaxDcMaxStat = 7;

        MaxMcChance = 40;
        MaxMcStatChance = 20;
        MaxMcMaxStat = 7;

        MaxScChance = 40;
        MaxScStatChance = 20;
        MaxScMaxStat = 7;
    }
    public void SetBeltBoots()
    {
        MaxDuraChance = 2;
        MaxDuraStatChance = 10;
        MaxDuraMaxStat = 3;

        MaxAcChance = 30;
        MaxAcStatChance = 30;
        MaxAcMaxStat = 3;

        MaxMacChance = 30;
        MaxMacStatChance = 30;
        MaxMacMaxStat = 3;

        MaxDcChance = 30;
        MaxDcStatChance = 30;
        MaxDcMaxStat = 3;

        MaxMcChance = 30;
        MaxMcStatChance = 30;
        MaxMcMaxStat = 3;

        MaxScChance = 30;
        MaxScStatChance = 30;
        MaxScMaxStat = 3;

        AgilityChance = 60;
        AgilityStatChance = 30;
        AgilityMaxStat = 3;
    }
    public void SetNecklace()
    {
        MaxDuraChance = 2;
        MaxDuraStatChance = 10;
        MaxDuraMaxStat = 3;

        MaxDcChance = 15;
        MaxDcStatChance = 30;
        MaxDcMaxStat = 7;

        MaxMcChance = 15;
        MaxMcStatChance = 30;
        MaxMcMaxStat = 7;

        MaxScChance = 15;
        MaxScStatChance = 30;
        MaxScMaxStat = 7;

        AccuracyChance = 60;
        AccuracyStatChance = 30;
        AccuracyMaxStat = 7;

        AgilityChance = 60;
        AgilityStatChance = 30;
        AgilityMaxStat = 7;
    }
    public void SetBracelet()
    {
        MaxDuraChance = 2;
        MaxDuraStatChance = 10;
        MaxDuraMaxStat = 3;

        MaxAcChance = 20;
        MaxAcStatChance = 30;
        MaxAcMaxStat = 6;

        MaxMacChance = 20;
        MaxMacStatChance = 30;
        MaxMacMaxStat = 6;

        MaxDcChance = 30;
        MaxDcStatChance = 30;
        MaxDcMaxStat = 6;

        MaxMcChance = 30;
        MaxMcStatChance = 30;
        MaxMcMaxStat = 6;

        MaxScChance = 30;
        MaxScStatChance = 30;
        MaxScMaxStat = 6;
    }
    public void SetRing()
    {
        MaxDuraChance = 2;
        MaxDuraStatChance = 10;
        MaxDuraMaxStat = 3;

        MaxAcChance = 25;
        MaxAcStatChance = 20;
        MaxAcMaxStat = 6;

        MaxMacChance = 25;
        MaxMacStatChance = 20;
        MaxMacMaxStat = 6;

        MaxDcChance = 15;
        MaxDcStatChance = 30;
        MaxDcMaxStat = 6;

        MaxMcChance = 15;
        MaxMcStatChance = 30;
        MaxMcMaxStat = 6;

        MaxScChance = 15;
        MaxScStatChance = 30;
        MaxScMaxStat = 6;
    }

    public void SetMount()
    {
        SetRing();
    }
}

public class ChatItem
{
    public ulong UniqueID;
    public string Title;
    public MirGridType Grid;

    public string RegexInternalName
    {
        get { return $"<{Title.Replace("(", "\\(").Replace(")", "\\)")}>"; }
    }

    public string InternalName
    {
        get { return $"<{Title}/{UniqueID}>"; }
    }

    public ChatItem() { }

    public ChatItem(BinaryReader reader)
    {
        UniqueID = reader.ReadUInt64();
        Title = reader.ReadString();
        Grid = (MirGridType)reader.ReadByte();
    }

    public void Save(BinaryWriter writer)
    {
        writer.Write(UniqueID);
        writer.Write(Title);
        writer.Write((byte)Grid);
    }
}
