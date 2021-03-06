﻿using UnityEngine;
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
    [AraleSerizlize.Field]
    public int id{ get; protected set;}
    [AraleSerizlize.Field]
    public string name{ get; protected set;}
    [AraleSerizlize.Field]
    public Skill.PointType pointType=Skill.PointType.None;
    [AraleSerizlize.Field]
    public Skill.FuncType funcType=Skill.FuncType.Attack;
    [AraleSerizlize.Field]
    public float distance{ get; protected set;}
    [AraleSerizlize.Field]
    public float cd{ get;protected set;}
    [AraleSerizlize.Field]
    public IArea area{ get; protected set;}
    [AraleSerizlize.Field]
    string mIcon;
    public string icon{ get{return "Skill/Icon/" + mIcon;}}
    [AraleSerizlize.Field]
    public string lua;
    [AraleSerizlize.Field]
    public string desc;
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


    #region 技能管理
    public const short ver = 5;
    public static bool isSkillFile(byte[] bs)
    {
        return bs.Length>5 && ((bs[0] == 0x73 && bs[1] == 0x6b && bs[2] == 0x69 && bs[3] == 0x6c && bs[4] == 0x6c)||(bs[0]==0xEF));
    }

    class XmlPack
    {
        [AraleSerizlize.Field]
        public List<GameSkill> skill;
    }
        
    public partial class Mgr
    {
        Dictionary<int, string> skillmap;
        Dictionary<int, GameSkill> skills = new Dictionary<int, GameSkill>();
        public void clear()
        {
            skills.Clear();
        }

        public GameSkill get(int id)
        {
            GameSkill gs;
            if (!skills.TryGetValue(id, out gs))
            {
                if (!loadSkill("Skill/"+getFile(id)))return null;
                gs = skills[id];
            }
            gs.lastUseTime = Time.realtimeSinceStartup;
            return gs;
        }

        string getFile(int id)
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

        public bool loadSkill(string skillPath)
        {
            TextAsset ta = ResLoad.get(skillPath).asset<TextAsset>();
            if (ta == null)
            {
                Log.e("Skill not find by ResLoad path="+skillPath, Log.Tag.Skill);
                return false;
            }
            return ta.bytes[0] == 0x73?loadSkills(ta.bytes):loadSkills(ta.text);
        }

        bool loadSkills(byte[] buff)
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

        bool loadSkills(string ctx)
        {
            try
            {
                XmlPack pack = AraleSerizlize.fromXml(typeof(XmlPack), ctx) as XmlPack;
                for(int i=0;i<pack.skill.Count;++i)
                {
                    skills[pack.skill[i].id] = pack.skill[i];
                }
                return true;
            }
            catch(System.Exception e)
            {
                Log.e(e.Message, Log.Tag.Skill, e);
                return false;
            }
        }

        public bool saveSkill(string skillPath, bool xml=true)
        {
            FileStream fs = null;
            try
            {
                if(xml)
                {
                    XmlPack pack = new XmlPack();
                    pack.skill = new List<GameSkill>();
                    pack.skill.AddRange(skills.Values);
                    AraleSerizlize.saveXml(pack, skillPath);
                }
                else
                {
                    fs = new FileStream(skillPath, FileMode.Create);
                    BinaryWriter w = new BinaryWriter(fs);
                    w.Write(new byte[]{0x73,0x6b,0x69,0x6c,0x6c});
                    w.Write(ver);
                    AraleSerizlize.write<GameSkill>(skills,w);
                    fs.Close();
                }
                return true;
            }
            catch(System.Exception e)
            {
                Log.e(e.Message, Log.Tag.Skill, e);
                if(fs!=null)fs.Close();
                return false;
            }
        }
    }

    static Mgr mMgr;
    public static GameSkill get(int id)
    {
        if(mMgr!=null)return mMgr.get(id);
        mMgr = new Mgr();
        return mMgr.get(id);
    }
    public static void clear()
    {
        if (mMgr == null)return;
        mMgr.clear();
    }
    #endregion
}
