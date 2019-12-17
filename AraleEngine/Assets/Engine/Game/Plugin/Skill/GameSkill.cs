using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Arale.Engine;
using System.Xml;

public partial class GameSkill : AraleSerizlize
{//所有变量只允许读,不允许设置,属于静态共享数据
    float  lastUseTime;
    [AraleSerizlize.Field]
    public int state=UnitState.ALL;
    [AraleSerizlize.Field]
    public string anim="";
    [AraleSerizlize.Field]
    public List<SkillAction> actions = new List<SkillAction>();
    public int id{ get; protected set;}
    public string name{ get; protected set;}
    public Skill.PointType pointType{ get; protected set;}
    public float distance{ get; protected set;}
    public float cd{ get;protected set;}
    public IArea area{ get; protected set;}
    [AraleSerizlize.Field]
    string mIcon;
    public string icon{ get{return "Skill/Icon/" + mIcon;}}
    public string lua{ get; protected set;}
    public string desc{ get; protected set;}
    public GameSkill()
    {
        desc = "";
    }

    public override void read(BinaryReader r)
    {
        id   = r.ReadInt32();
        name = r.ReadString();
        anim = r.ReadString();
        state = r.ReadInt32();
        actions = AraleSerizlize.read<SkillAction>(r);
    }

    public override void write(BinaryWriter w)
    {
        w.Write(id);
        w.Write(name);
        w.Write(anim);
        w.Write(state);
        actions.Sort(delegate(SkillAction x, SkillAction y)
            {
                return x.time.CompareTo(y.time);
            });
        AraleSerizlize.write<SkillAction>(actions, w);
    }

    public override void read(XmlNode n)
    {
        id = int.Parse(n.Attributes["id"].Value);
        Debug.Assert(id != 0);
        XmlAttribute attr = n.Attributes["name"];
        name = attr==null?"":attr.Value;
        attr = n.Attributes["anim"];
        anim = attr==null?"":attr.Value;
        state = System.Convert.ToInt32(n.Attributes["state"].Value, 16);
        actions = AraleSerizlize.read<SkillAction>(n);
        attr = n.Attributes["pointType"];
        pointType = attr==null?Skill.PointType.None:(Skill.PointType)System.Enum.Parse(typeof(Skill.PointType), attr.Value);
        attr = n.Attributes["distance"];
        distance = attr == null ? 0 : float.Parse(attr.Value);
        attr = n.Attributes["cd"];
        cd = attr== null ? 0 : float.Parse(attr.Value);
        attr = n.Attributes["area"];
        area = attr == null ? null : GameArea.fromString(attr.Value);
        attr = n.Attributes["icon"];
        mIcon = attr== null ? "" : attr.Value;
        attr = n.Attributes["lua"];
        lua = attr== null ? null : attr.Value;
        attr = n.Attributes["desc"];
        desc = attr== null ? "" : attr.Value;
    }


    static bool loadSkill(string skillPath, Dictionary<int, GameSkill> skills)
    {
        TextAsset ta = ResLoad.get(skillPath).asset<TextAsset>();
        if (ta == null)
        {
            Log.e("Skill not find by ResLoad path="+skillPath, Log.Tag.Skill);
            return false;
        }
        return ta.bytes[0] == 0x73?loadSkills(ta.bytes,skills):loadSkills(ta.text,skills);
    }

    static bool loadSkills(byte[] buff, Dictionary<int, GameSkill> skills)
    {
        try
        {
            if(!isSkillFile(buff))throw new System.Exception("not skill file");
            using(MemoryStream fs = new MemoryStream(buff))
            {
                fs.Seek(5, SeekOrigin.Begin);
                BinaryReader r = new BinaryReader(fs);
                int v = r.ReadInt16();
                //新版本应对老代码兼容，根据版本使用对应的读取序列化
                if(v>ver)throw new System.Exception("version error!v="+v);
                AraleSerizlize.read<GameSkill>(skills, r);
            }
            return true;
        }
        catch(System.Exception e)
        {
            Log.e(e.Message, Log.Tag.Skill, e);
            return false;
        }
    }

    static bool loadSkills(string ctx, Dictionary<int, GameSkill> skills)
    {
        try
        {
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(ctx);
            AraleSerizlize.read<GameSkill>(skills, xml.SelectSingleNode("skills"));
            return true;
        }
        catch(System.Exception e)
        {
            Log.e(e.Message, Log.Tag.Skill, e);
            return false;
        }
    }

    static string getFile(int id)
    {
        if (skillmap == null)
        {
            skillmap = new Dictionary<int, string>();
            TextAsset ta = ResLoad.get("Skill/skillmap").asset<TextAsset>();
            string[] ss = ta.text.Split(new string[]{ "," }, System.StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < ss.Length; i += 2)
            {
                skillmap[int.Parse(ss[i])] = ss[i+1];
            }
        }
        return skillmap[id];
    }

    #region 外部接口
    public const short ver = 5;
    static Dictionary<int, string> skillmap;
    static Dictionary<int, GameSkill> skills = new Dictionary<int, GameSkill>();
    public static void clear()
    {
        skills.Clear();
    }
    public static GameSkill get(int id)
    {
        GameSkill gs;
        if (!skills.TryGetValue(id, out gs))
        {
            if (!loadSkill("Skill/"+getFile(id),skills))return null;
            gs = skills[id];
        }
        gs.lastUseTime = Time.realtimeSinceStartup;
        return gs;
    }

    public static bool isSkillFile(byte[] bs)
    {
        return bs.Length>5 && ((bs[0] == 0x73 && bs[1] == 0x6b && bs[2] == 0x69 && bs[3] == 0x6c && bs[4] == 0x6c)||(bs[0]==0xEF));
    }
    #endregion
}
