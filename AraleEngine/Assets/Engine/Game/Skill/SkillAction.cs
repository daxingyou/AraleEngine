using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Arale.Engine;

public partial class SkillAction : SkillNode
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
    public List<SkillBullet> bullets = new List<SkillBullet>();

    public override void read(BinaryReader r)
    {
        time = r.ReadSingle();
        loopInterval = r.ReadSingle();
        loopTimes = r.ReadInt32();
        anim = r.ReadString();
        state = r.ReadInt32();
        mask = r.ReadInt32();
        bullets = AraleSerizlize.read<SkillBullet>(r);
        #if UNITY_EDITOR
        for (int i = 0; i < bullets.Count; ++i)
        {
            bullets[i].setAction(this);
        }
        #endif
    }

    public override void write(BinaryWriter w)
    {
        w.Write(time);
        w.Write(loopInterval);
        w.Write(loopTimes);
        w.Write(anim);
        w.Write(state);
        w.Write(mask);
        AraleSerizlize.write<SkillBullet>(bullets, w);
    } 
}