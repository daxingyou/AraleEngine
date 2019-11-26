using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Arale.Engine;
using System.Xml;

public partial class SkillAction : SkillNode
{
    public float  time;
    public float  loopInterval=1;
    public int    loopTimes;
    public int    state;//0默认不生效
    int    mask;
    public bool   end{get{return (mask&0x0001)!=0;} set{mask = value ? mask | 0x0001 : mask & ~0x0001;}}
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

    public override void read(XmlNode n)
    {
        time = float.Parse(n.Attributes["time"].Value);
        XmlAttribute attr = n.Attributes["loopInterval"];
        loopInterval = attr == null ? 0f : float.Parse(attr.Value);
        attr = n.Attributes["loopTimes"];
        loopTimes = attr == null ? 0 : int.Parse(attr.Value);
        attr = n.Attributes["loopTimes"];
        state = attr == null? 0:System.Convert.ToInt32(attr.Value, 16);
        end = n.Attributes["end"] != null;
        for (int i = 0,max=n.ChildNodes.Count; i < max; ++i)
        {
            XmlNode nd = n.ChildNodes[i];
            SkillNode.Type nodeType = (SkillNode.Type)System.Enum.Parse(typeof(SkillNode.Type), nd.Name);
            SkillNode sn = SkillNode.createNode((SkillNode.Type)nodeType);
            sn.read(nd);
            nodes.Add(sn);
            #if UNITY_EDITOR
            sn.action = this;
            #endif
        }
    } 
}