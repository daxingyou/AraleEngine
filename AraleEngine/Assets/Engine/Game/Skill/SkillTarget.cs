using UnityEngine;
using System.Collections;
using System.IO;

public partial class SkillTarget : SkillNode
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
    public static SkillTarget newType(Type tp)
    {
        SkillTarget t = null;
        switch (tp)
        {
            case Type.Dir:
                t = new SkillVecctorTarget();
                break;
            case Type.Pos:
                t = new SkillVecctorTarget();
                break;
            case Type.Area:
                t = new SkillAreaTarget();
                break;
            default:
                t = new SkillTarget();
                break;
        }
        t.type = tp;
        return t;
    }

    public static SkillTarget readType(BinaryReader r)
    {
        int mask = r.ReadInt32();
        SkillTarget t = newType((Type)(mask&0x000f));
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

public partial class SkillVecctorTarget : SkillTarget
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

public partial class SkillAreaTarget : SkillTarget
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
