using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Arale.Engine;

public partial class GameSkill : AraleSerizlize
{//所有变量只允许读,不允许设置,属于静态共享数据
    float  lastUseTime;
    public int initState=UnitState.ALL;
    public string initAnim="";
    public List<Action> actions = new List<Action>();
    public string name{ get; protected set;}

    public abstract partial class Node : AraleSerizlize{}

    public partial class Action : Node
    {
        public float  time;
        public float  loopInterval=1;
        public int    loopTimes;
        public string anim="";
        public int    state=UnitState.ALL;
        int    mask;
        public bool   end{get{return (mask&0x0001)!=0;}}
        public bool   breakable{get{return (mask&0x0002)!=0;}}
        public bool   cancelable{get{return (mask&0x0004)!=0;}}
        public List<Bullet> bullets = new List<Bullet>();

        public override void read(BinaryReader r)
        {
            time = r.ReadSingle();
            loopInterval = r.ReadSingle();
            loopTimes = r.ReadInt32();
            anim = r.ReadString();
            state = r.ReadInt32();
            mask = r.ReadInt32();
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
            w.Write(mask);
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
        public Target target = new Target();

        public override void read(BinaryReader r)
        {
            id = r.ReadInt32();
            harm = r.ReadInt32();
            buffId = r.ReadInt32();
            moveId = r.ReadInt32();
            mode = (Mode)r.ReadInt32();
            target = Target.readType(r);
        }

        public override void write(BinaryWriter w)
        {
            w.Write(id);
            w.Write(harm);
            w.Write(buffId);
            w.Write(moveId);
            w.Write((int)mode);
            target.write(w);
        }
    }

    public partial class Target : Node
    {
        public enum Type
        {
            None,
            Pos,
            Dir,
            Area,
            Unit,
        }

        public enum NoneType
        {
            Pos,
            Dir,
            Unit,
        }

        public enum Selector
        {
            None,
            Nearst,
            MinHp,
            MaxHp,
            MinDF,
            MaxDF,
        }

        int mask;
        public Type type{get{return (Type)(mask&0x000000ff);} protected set{mask&=0x7fffff00; mask|=((int)value&0x000000ff);}}
        public NoneType noneType{get{return (NoneType)((mask&0x0000ff00)>>8);} protected set{mask &= 0x7fff00ff; mask|=(((int)value&0x000000ff)<<8);}}
        public UnitRelation relation{get{return (UnitRelation)((mask&0x00ff0000)>>16);} protected set{mask &= 0x7f00ffff; mask|=(((int)value&0x000000ff)<<16);}}
        public Selector selector{get{return (Selector)((mask&0xff000000)>>24);} protected set{mask &= 0x00ffffff; mask|=(((int)value&0x000000ff)<<24);}}
        public static Target newType(Type tp)
        {
            Target t = null;
            switch (tp)
            {
                case Type.Dir:
                    t = new VecctorTarget();
                    break;
                case Type.Pos:
                    t = new VecctorTarget();
                    break;
                case Type.Area:
                    t = new AreaTarget();
                    break;
                default:
                    t = new Target();
                    break;
            }
            t.type = tp;
            return t;
        }

        public static Target readType(BinaryReader r)
        {
            int mask = r.ReadInt32();
            Target t = newType((Type)(mask&0x000f));
            t.mask = mask;
            t.read(r);
            return t;
        }

        public override void read(BinaryReader r)
        {
        }

        public override void write(BinaryWriter w)
        {
            w.Write(mask);
        }
    }

    public partial class VecctorTarget : Target
    {
        public bool  local;
        public Vector3 vct;
        public override void read(BinaryReader r)
        {
            base.read(r);
            local = r.ReadBoolean();
            vct.x = r.ReadSingle();
            vct.y = r.ReadSingle();
            vct.z = r.ReadSingle();
        }

        public override void write(BinaryWriter w)
        {
            base.write(w);
            w.Write(local);
            w.Write(vct.x);
            w.Write(vct.y);
            w.Write(vct.z);
        }
    }

    public partial class AreaTarget : Target
    {
        public string area;
        public override void read(BinaryReader r)
        {
            base.read(r);
            area = r.ReadString();
        }

        public override void write(BinaryWriter w)
        {
            base.write(w);
            w.Write(area);
        }
    }

    public override void read(BinaryReader r)
    {
        name = r.ReadString();
        initAnim = r.ReadString();
        initState = r.ReadInt32();
        actions = AraleSerizlize.read<Action>(r);
    }

    public override void write(BinaryWriter w)
    {
        w.Write(name);
        w.Write(initAnim);
        w.Write(initState);
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
        catch(System.Exception e)
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
            if(!isSkillFile(ta.bytes))throw new System.Exception("not skill file");
            fs = new MemoryStream(ta.bytes);
            fs.Seek(5, SeekOrigin.Begin);
            BinaryReader r = new BinaryReader(fs);
            int v = r.ReadInt16();
            //新版本应对老代码兼容，根据版本使用对应的读取序列化
            if(v>ver)throw new System.Exception("version error!v="+v);
            AraleSerizlize.read<GameSkill>(skills, r);
            fs.Close();
            return true;
        }
        catch(System.Exception e)
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
