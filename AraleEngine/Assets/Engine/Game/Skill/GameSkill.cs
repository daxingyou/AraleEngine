using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using Arale.Engine;
using System;

public partial class GameSkill : AraleSerizlize
{
    float  lastUseTime;
    List<Action> actions = new List<Action>();
    public string name{ get; protected set;}

    public abstract partial class Node : AraleSerizlize{}

    public partial class Action : Node
    {
        public float  time;
        public float  loopInterval=1;
        public int    loopTimes;
        public string anim="";
        public int    state=UnitState.ALL;
        public bool   breakable;
        public List<Bullet> bullets = new List<Bullet>();

        public override void read(BinaryReader r)
        {
            time = r.ReadSingle();
            loopInterval = r.ReadSingle();
            loopTimes = r.ReadInt32();
            anim = r.ReadString();
            state = r.ReadInt32();
            breakable = r.ReadBoolean();
            bullets = AraleSerizlize.read<Bullet>(r);
            for (int i = 0; i < bullets.Count; ++i)
            {
                bullets[i].setAction(this);
            }
        }

        public override void write(BinaryWriter w)
        {
            w.Write(time);
            w.Write(loopInterval);
            w.Write(loopTimes);
            w.Write(anim);
            w.Write(state);
            w.Write(breakable);
            AraleSerizlize.write<Bullet>(bullets, w);
        } 
    }

    public partial class Bullet : Node
    {
        public enum Mode
        {
            None,
            Scatter,//散射
            Chain,  //链式
        }

        public int id;
        public int harm;
        public int buffId;
        public int moveId;
        public Mode mode;
        public List<Target> targets=new List<Target>();

        public override void read(BinaryReader r)
        {
            id = r.ReadInt32();
            harm = r.ReadInt32();
            buffId = r.ReadInt32();
            moveId = r.ReadInt32();
            mode = (Mode)r.ReadInt32();
            targets = AraleSerizlize.read<Target>(r);
        }

        public override void write(BinaryWriter w)
        {
            w.Write(id);
            w.Write(harm);
            w.Write(buffId);
            w.Write(moveId);
            w.Write((int)mode);
            AraleSerizlize.write<Target>(targets, w);
        }
    }

    public partial class Target : Node
    {
        enum Find
        {
            Pos,
            Dir,
            Unit,
            InArea,
            Nearst,
            MinHp,
            MaxHp,
            MinDF,
            MaxDF,
        }

        public int unitTypeMask;
        public int maxTarget=1;
        public string area="";

        public override void read(BinaryReader r)
        {
            unitTypeMask = r.ReadInt32();
            maxTarget = r.ReadInt32();
            area = r.ReadString();
        }

        public override void write(BinaryWriter w)
        {
            w.Write(unitTypeMask);
            w.Write(maxTarget);
            w.Write(area);
        }
    }

    public override void read(BinaryReader r)
    {
        name = r.ReadString();
        actions = AraleSerizlize.read<Action>(r);
    }

    public override void write(BinaryWriter w)
    {
        w.Write(name);
        actions.Sort(delegate(Action x, Action y)
            {
                return x.time.CompareTo(y.time);
            });
        AraleSerizlize.write<Action>(actions, w);
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
        catch(Exception e)
        {
            Log.e(e.Message, Log.Tag.Skill, e);
            if(fs!=null)fs.Close();
            return false;
        }
    }

    public static bool loadSkill(string skillPath)
    {
        skillPath =FileUtils.toResourcesPath(skillPath);
        skillPath = skillPath.Remove(skillPath.Length-4);
        TextAsset ta = ResLoad.get(skillPath).asset<TextAsset>();
        if (ta == null)
        {
            Log.e("Skill not find by ResLoad path="+skillPath, Log.Tag.Skill);
            return false;
        }

        MemoryStream fs = null;
        try
        {
            if(!isSkillFile(ta.bytes))throw new Exception("not skill file");
            fs = new MemoryStream(ta.bytes);
            fs.Seek(5, SeekOrigin.Begin);
            BinaryReader r = new BinaryReader(fs);
            int v = r.ReadInt16();
            if(v!=ver)throw new Exception("version error!v="+v);
            AraleSerizlize.read<GameSkill>(skills, r);
            fs.Close();
            return true;
        }
        catch(Exception e)
        {
            Log.e(e.Message, Log.Tag.Skill, e);
            if(fs!=null)fs.Close();
            return false;
        }
        return true;
    }

    #region 外部接口
    public const short ver = 5;
    static Dictionary<string, GameSkill> skills = new Dictionary<string, GameSkill>();
    public static void clear()
    {
        skills.Clear();
    }
    public static GameSkill get(string name, string path=null)
    {
        GameSkill gs;
        if (!skills.TryGetValue(name, out gs))
        {
            loadSkill(path);
        }
        gs.lastUseTime = Time.realtimeSinceStartup;
        return gs;
    }


    public static bool isSkillFile(byte[] bs)
    {
        return bs.Length>5 && bs[0] == 0x73 && bs[1] == 0x6b && bs[2] == 0x69 && bs[3] == 0x6c && bs[4] == 0x6c;
    }
    #endregion
}
