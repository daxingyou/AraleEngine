using UnityEngine;
using System.Collections;
using System.IO;
using Arale.Engine;

public partial class SkillTarget : SkillNode
{
    public enum Target
    {
        Unit,
        Pos,
        Dir,
        Area,
    }

    public enum Location
    {
        Self,  //相对自己
        Target,//相对目标
        World, //实际坐标
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
    [AraleSerizlize.Field]
    public Vector3 vct;
    [AraleSerizlize.Field]
    public string  area="";
    [AraleSerizlize.Field]
    public Target target{get{return (Target)(mask&0x000000ff);} protected set{mask&=0x7fffff00; mask|=((int)value&0x000000ff);}}
    [AraleSerizlize.Field]
    public Location location{get{return (Location)((mask&0x0000ff00)>>8);} protected set{mask &= 0x7fff00ff; mask|=(((int)value&0x000000ff)<<8);}}
    [AraleSerizlize.Field]
    public UnitRelation relation{get{return (UnitRelation)((mask&0x00ff0000)>>16);} protected set{mask &= 0x7f00ffff; mask|=(((int)value&0x000000ff)<<16);}}
    [AraleSerizlize.Field]
    public Selector selector{get{return (Selector)((mask&0xff000000)>>24);} protected set{mask &= 0x00ffffff; mask|=(((int)value&0x000000ff)<<24);}}

    public override void read(BinaryReader r)
    {
        mask = r.ReadInt32();
        vct.x = r.ReadSingle();
        vct.y = r.ReadSingle();
        vct.z = r.ReadSingle();
    }

    public override void write(BinaryWriter w)
    {
        w.Write(mask);
        w.Write(vct.x);
        w.Write(vct.y);
        w.Write(vct.z);
    }

    public Vector3 locationDir(Unit u)
    {
        switch (location)
        {
            case Location.Self:
                return Quaternion.Euler(vct)*u.transform.forward;
            case Location.Target:
                return Quaternion.Euler(vct)*u.skill.targetDir+vct;
            case Location.World:
                return Quaternion.Euler(vct)*Vector3.forward;
            default:
                return Quaternion.Euler(vct)*u.transform.forward;
        }
    }

    public Vector3 locationPos(Unit u)
    {
        switch (location)
        {
            case Location.Self:
                return u.transform.localToWorldMatrix.MultiplyPoint(vct);
            case Location.Target:
                return u.skill.targetPos+vct;
            case Location.World:
                return vct;
            default:
                return u.transform.localToWorldMatrix.MultiplyPoint(vct);
        }
    }
}
