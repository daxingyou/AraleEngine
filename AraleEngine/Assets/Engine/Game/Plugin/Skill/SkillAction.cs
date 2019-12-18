using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Arale.Engine;

public partial class SkillAction : SkillNode
{
    [AraleSerizlize.Field]
    public float  time;
    [AraleSerizlize.Field]
    public float  loopInterval;
    [AraleSerizlize.Field]
    public int    loopTimes;
    [AraleSerizlize.Field]
    public int    state;//0默认不生效
    int    mask;
    [AraleSerizlize.Field]
    public bool   end{get{return (mask&0x0001)!=0;} set{mask = value ? mask | 0x0001 : mask & ~0x0001;}}
    [AraleSerizlize.Field]
    public List<SkillNode> nodes = new List<SkillNode>();
    public override void read(BinaryReader r)
    {
        time = r.ReadSingle();
        loopInterval = r.ReadSingle();
        loopTimes = r.ReadInt32();
        state = r.ReadInt32();
        mask = r.ReadInt32();
        int n = r.ReadInt32();
        for (int i = 0; i < n; ++i)
        {
            int nodeType = r.PeekChar();
            SkillNode sn = SkillNode.createNode((SkillNode.Type)nodeType);
            sn.read(r);
            nodes.Add(sn);
            #if UNITY_EDITOR
            sn.action = this;
            #endif
        }
    }

    public override void write(BinaryWriter w)
    {
        w.Write(time);
        w.Write(loopInterval);
        w.Write(loopTimes);
        w.Write(state);
        w.Write(mask);
        AraleSerizlize.write<SkillNode>(nodes, w);
    }
}