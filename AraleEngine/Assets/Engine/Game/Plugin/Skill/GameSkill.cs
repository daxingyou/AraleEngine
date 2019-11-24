using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Arale.Engine;
using System.Xml;

public partial class GameSkill : AraleSerizlize
{//所有变量只允许读,不允许设置,属于静态共享数据
    float  lastUseTime;
    public int state=UnitState.ALL;
    public string anim="";
    public List<SkillAction> actions = new List<SkillAction>();
    public string name{ get; protected set;}
    //施法距离(位置,指向,选中,无)
    public float distance;

    public override void read(BinaryReader r)
    {
        name = r.ReadString();
        anim = r.ReadString();
        state = r.ReadInt32();
        actions = AraleSerizlize.read<SkillAction>(r);
    }

    public override void write(BinaryWriter w)
    {
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
        name = n.Attributes["name"].Value;
        XmlAttribute attr = n.Attributes["anim"];
        anim = attr==null?"":attr.Value;
        state = System.Convert.ToInt32(n.Attributes["state"].Value, 16);
        actions = AraleSerizlize.read<SkillAction>(n);
    }

    public static bool saveSkill(string skillPath)
    {
        FileStream fs = null;
        try
        {
            fs = new FileStream(skillPath, FileMode.Create);
            BinaryWriter w = new BinaryWriter(fs);
            w.Write(new byte[]{0x73,0x6b,0x69,0x6c,0x6c});
            w.Write(ver);
            AraleSerizlize.write<GameSkill>(skills,w);
            fs.Close();
            return true;
        }
        catch(System.Exception e)
        {
            Log.e(e.Message, Log.Tag.Skill, e);
            if(fs!=null)fs.Close();
            return false;
        }
    }

    public static bool loadSkill(string skillPath)
    {
        TextAsset ta = ResLoad.get(skillPath).asset<TextAsset>();
        if (ta == null)
        {
            Log.e("Skill not find by ResLoad path="+skillPath, Log.Tag.Skill);
            return false;
        }
        //return loadSkills(ta.bytes);
        return ta.bytes[0] == 0x73?loadSkills(ta.bytes):loadSkills(ta.text);;
    }

    static bool loadSkills(byte[] buff)
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

    static bool loadSkills(string ctx)
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

    #region 外部接口
    public const short ver = 5;
    static Dictionary<string, GameSkill> skills = new Dictionary<string, GameSkill>();
    public static void clear()
    {
        skills.Clear();
    }
    public static GameSkill get(string skillname)
    {
        int idx = skillname.LastIndexOf('/');
        string key = skillname.Substring(idx + 1);
        GameSkill gs;
        if (!skills.TryGetValue(key, out gs))
        {
            if (!loadSkill("Skill/"+skillname.Remove(idx)))return null;
            gs = skills[key];
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
