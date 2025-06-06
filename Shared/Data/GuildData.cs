﻿public class GuildRank
{
    public List<GuildMember> Members = new List<GuildMember>();
    public string Name = "";
    public int Index = 0;
    public GuildRankOptions Options = (GuildRankOptions)0;

    public GuildRank() { }

    public GuildRank(BinaryReader reader, bool offline = false)
    {
        Name = reader.ReadString();
        Options = (GuildRankOptions)reader.ReadByte();

        if (!offline)
        {
            Index = reader.ReadInt32();
        }

        int Membercount = reader.ReadInt32();
        for (int j = 0; j < Membercount; j++)
        {
            Members.Add(new GuildMember(reader, offline));
        }
    }
    public void Save(BinaryWriter writer, bool save = false)
    {
        writer.Write(Name);
        writer.Write((byte)Options);
        if (!save)
        {
            writer.Write(Index);
        }

        writer.Write(Members.Count);

        for (int j = 0; j < Members.Count; j++)
        {
            Members[j].Save(writer);
        }
    }
}

public class GuildStorageItem
{
    public UserItem Item;
    public long UserId = 0;
    public GuildStorageItem() { }

    public GuildStorageItem(BinaryReader reader)
    {
        Item = new UserItem(reader);
        UserId = reader.ReadInt64();
    }
    public void Save(BinaryWriter writer)
    {
        Item.Save(writer);
        writer.Write(UserId);
    }
}

public class GuildMember
{
    public string Name = "";
    public int Id;
    public object Player;
    public DateTime LastLogin;
    public bool hasvoted;
    public bool Online;

    public GuildMember() { }

    public GuildMember(BinaryReader reader, bool offline = false)
    {
        Name = reader.ReadString();
        Id = reader.ReadInt32();
        LastLogin = DateTime.FromBinary(reader.ReadInt64());
        hasvoted = reader.ReadBoolean();
        Online = reader.ReadBoolean();
        Online = offline ? false : Online;
    }
    public void Save(BinaryWriter writer)
    {
        writer.Write(Name);
        writer.Write(Id);
        writer.Write(LastLogin.ToBinary());
        writer.Write(hasvoted);
        writer.Write(Online);
    }
}

public class GuildBuffInfo
{
    public int Id;
    public int Icon = 0;
    public string Name = "";
    public byte LevelRequirement;
    public byte PointsRequirement = 1;
    public int TimeLimit;
    public int ActivationCost;

    public Stats Stats;

    public GuildBuffInfo() 
    {
        Stats = new Stats();
    }

    public GuildBuffInfo(BinaryReader reader)
    {
        Id = reader.ReadInt32();
        Icon = reader.ReadInt32();
        Name = reader.ReadString();
        LevelRequirement = reader.ReadByte();
        PointsRequirement = reader.ReadByte();
        TimeLimit = reader.ReadInt32();
        ActivationCost = reader.ReadInt32();

        Stats = new Stats(reader);
    }

    public GuildBuffInfo(InIReader reader, int i)
    {
        Id = reader.ReadInt32("Buff-" + i.ToString(), "Id", 0);
        Icon = reader.ReadInt32("Buff-" + i.ToString(), "Icon", 0);
        Name = reader.ReadString("Buff-" + i.ToString(), "Name", "");
        LevelRequirement = reader.ReadByte("Buff-" + i.ToString(), "LevelReq", 0);
        PointsRequirement = reader.ReadByte("Buff-" + i.ToString(), "PointsReq", 1);
        TimeLimit = reader.ReadInt32("Buff-" + i.ToString(), "TimeLimit", 0); ;
        ActivationCost = reader.ReadInt32("Buff-" + i.ToString(), "ActivationCost", 0);

        Stats = new Stats();
        Stats[Stat.最大防御] = reader.ReadByte("Buff-" + i.ToString(), "BuffAc", 0);
        Stats[Stat.最大魔御] = reader.ReadByte("Buff-" + i.ToString(), "BuffMAC", 0);
        Stats[Stat.最大攻击] = reader.ReadByte("Buff-" + i.ToString(), "BuffDc", 0);
        Stats[Stat.最大魔法] = reader.ReadByte("Buff-" + i.ToString(), "BuffMc", 0);
        Stats[Stat.最大道术] = reader.ReadByte("Buff-" + i.ToString(), "BuffSc", 0);
        Stats[Stat.HP] = reader.ReadInt32("Buff-" + i.ToString(), "BuffMaxHp", 0);
        Stats[Stat.MP] = reader.ReadInt32("Buff-" + i.ToString(), "BuffMaxMp", 0);
        Stats[Stat.采矿率百分比] = reader.ReadByte("Buff-" + i.ToString(), "BuffMineRate", 0);
        Stats[Stat.宝玉成功率] = reader.ReadByte("Buff-" + i.ToString(), "BuffGemRate", 0);
        Stats[Stat.捕鱼率百分比] = reader.ReadByte("Buff-" + i.ToString(), "BuffFishRate", 0);
        Stats[Stat.经验率百分比] = reader.ReadByte("Buff-" + i.ToString(), "BuffExpRate", 0);
        Stats[Stat.工艺率百分比] = reader.ReadByte("Buff-" + i.ToString(), "BuffCraftRate", 0);
        Stats[Stat.技能熟练度] = reader.ReadByte("Buff-" + i.ToString(), "BuffSkillRate", 0);
        Stats[Stat.体力恢复] = reader.ReadByte("Buff-" + i.ToString(), "BuffHpRegen", 0);
        Stats[Stat.法力恢复] = reader.ReadByte("Buff-" + i.ToString(), "BuffMpRegen", 0);
        Stats[Stat.功力] = reader.ReadByte("Buff-" + i.ToString(), "BuffAttack", 0);
        Stats[Stat.物品爆率百分比] = reader.ReadByte("Buff-" + i.ToString(), "BuffDropRate", 0);
        Stats[Stat.金币爆率百分比] = reader.ReadByte("Buff-" + i.ToString(), "BuffGoldRate", 0);
    }

    public void Save(InIReader reader, int i)
    {
        reader.Write("Buff-" + i.ToString(), "Id", Id);
        reader.Write("Buff-" + i.ToString(), "Icon", Icon);
        reader.Write("Buff-" + i.ToString(), "Name", Name);
        reader.Write("Buff-" + i.ToString(), "LevelReq", LevelRequirement);
        reader.Write("Buff-" + i.ToString(), "PointsReq", PointsRequirement);
        reader.Write("Buff-" + i.ToString(), "TimeLimit", TimeLimit);
        reader.Write("Buff-" + i.ToString(), "ActivationCost", ActivationCost);
        reader.Write("Buff-" + i.ToString(), "BuffAc", Stats[Stat.最大防御]);
        reader.Write("Buff-" + i.ToString(), "BuffMAC", Stats[Stat.最大魔御]);
        reader.Write("Buff-" + i.ToString(), "BuffDc", Stats[Stat.最大攻击]);
        reader.Write("Buff-" + i.ToString(), "BuffMc", Stats[Stat.最大魔法]);
        reader.Write("Buff-" + i.ToString(), "BuffSc", Stats[Stat.最大道术]);
        reader.Write("Buff-" + i.ToString(), "BuffMaxHp", Stats[Stat.HP]);
        reader.Write("Buff-" + i.ToString(), "BuffMaxMp", Stats[Stat.MP]);
        reader.Write("Buff-" + i.ToString(), "BuffMineRate", Stats[Stat.采矿率百分比]);
        reader.Write("Buff-" + i.ToString(), "BuffGemRate", Stats[Stat.宝玉成功率]);
        reader.Write("Buff-" + i.ToString(), "BuffFishRate", Stats[Stat.捕鱼率百分比]);
        reader.Write("Buff-" + i.ToString(), "BuffExpRate", Stats[Stat.经验率百分比]); ;
        reader.Write("Buff-" + i.ToString(), "BuffCraftRate", Stats[Stat.工艺率百分比]);
        reader.Write("Buff-" + i.ToString(), "BuffSkillRate", Stats[Stat.技能熟练度]);
        reader.Write("Buff-" + i.ToString(), "BuffHpRegen", Stats[Stat.体力恢复]);
        reader.Write("Buff-" + i.ToString(), "BuffMpRegen", Stats[Stat.法力恢复]);
        reader.Write("Buff-" + i.ToString(), "BuffAttack", Stats[Stat.功力]);
        reader.Write("Buff-" + i.ToString(), "BuffDropRate", Stats[Stat.物品爆率百分比]);
        reader.Write("Buff-" + i.ToString(), "BuffGoldRate", Stats[Stat.金币爆率百分比]);
    }

    public void Save(BinaryWriter writer)
    {
        writer.Write(Id);
        writer.Write(Icon);
        writer.Write(Name);
        writer.Write(LevelRequirement);
        writer.Write(PointsRequirement);
        writer.Write(TimeLimit);
        writer.Write(ActivationCost);

        Stats.Save(writer);
    }

    public override string ToString()
    {
        return string.Format("{0}: {1}", Id, Name);
    }

    public string ShowStats()
    {
        string text = string.Empty;

        foreach (var val in Stats.Values)
        {
            var c = val.Value < 0 ? "降低" : "提高";

            var txt = $"{c} {val.Key} : {val.Value}{(val.Key.ToString().Contains("数率") ? "%" : "")}\n";

            text += txt;
        }

        return text;
    }
}

public class GuildBuff
{
    public int Id;
    public GuildBuffInfo Info;
    public bool Active = false;
    public int ActiveTimeRemaining;

    public bool UsingGuildSkillIcon
    {
        get { return Info != null && Info.Icon < 1000; }
    }

    public GuildBuff() { }

    public GuildBuff(BinaryReader reader)
    {
        Id = reader.ReadInt32();
        Active = reader.ReadBoolean();
        ActiveTimeRemaining = reader.ReadInt32();
    }
    public void Save(BinaryWriter writer)
    {
        writer.Write(Id);
        writer.Write(Active);
        writer.Write(ActiveTimeRemaining);
    }
}

//过时但无法删除删除后旧数据库可能会无法加载
public class GuildBuffOld
{
    public GuildBuffOld() { }

    public GuildBuffOld(BinaryReader reader)
    {
        reader.ReadByte();
        reader.ReadInt64();
    }
}