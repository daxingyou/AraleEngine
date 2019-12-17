using UnityEngine;
using System.Collections;
using Arale.Engine;
using System.IO;
using System.Xml;

public abstract partial class SkillNode : AraleSerizlize
{
    public enum Type
    {
        Action,//行为
        Anim,  //动画
        Harm,  //伤害
        Bullet,//子弹
        Buff,  //buff
        Event, //事件
        Move,  //位移
        Lua,   //Lua事件
    };
    public Type type;
    #if UNITY_EDITOR
    public SkillAction action{ get; set;}
    #endif
    public static SkillNode createNode(Type nodeType)
    {
        SkillNode n = null;
        switch (nodeType)
        {
            case SkillNode.Type.Action:
                n = new SkillAction();
                break;
            case SkillNode.Type.Anim:
                n = new SkillAnim();
                break;
            case SkillNode.Type.Harm:
                n = new SkillHarm();
                break;
            case SkillNode.Type.Bullet:
                n = new SkillBullet();
                break;
            case SkillNode.Type.Buff:
                n = new SkillBuff();
                break;
            case SkillNode.Type.Event:
                n = new SkillEvent();
                break;
            case SkillNode.Type.Move:
                n = new SkillMove();
                break;
            case SkillNode.Type.Lua:
                n = new SkillLua();
                break;
            default:
                Debug.LogError("unsurpport skillnode type:"+nodeType);
                return null;
        }
        n.type = nodeType;
        return n;
    }

    public override void read(BinaryReader r)
    {
        type = (Type)r.ReadInt32();
    }

    public override void write(BinaryWriter w)
    {
        w.Write((int)type);
    }
}

public partial class SkillAnim : SkillNode
{
    [AraleSerizlize.Field]
    public string anim="";
    public override void read(BinaryReader r)
    {
        base.read(r);
        anim = r.ReadString();
    }

    public override void write(BinaryWriter w)
    {
        base.write(w);
        w.Write(anim);
    }

    public override void read(XmlNode n)
    {
        base.read(n);
        anim = n.Attributes["anim"].Value;
    }
} 

public partial class SkillBullet : SkillTarget
{
    public enum Mode
    {
        None,
        Scatter,//散射
        Chain,  //链式
    }

    [AraleSerizlize.Field]
    public int  id;
    [AraleSerizlize.Field]
    public int  harm;
    [AraleSerizlize.Field]
    public int  num;
    [AraleSerizlize.Field]
    public Mode mode;
    public override void read(BinaryReader r)
    {
        base.read(r);
        id = r.ReadInt32();
        harm = r.ReadInt32();
        mode = (Mode)r.ReadInt32();
    }

    public override void write(BinaryWriter w)
    {
        base.write(w);
        w.Write(id);
        w.Write(harm);
        w.Write((int)mode);
    }

    public override void read(XmlNode n)
    {
        base.read(n);
        id = int.Parse(n.Attributes["id"].Value);
        XmlAttribute attr = n.Attributes["harm"];
        harm = attr == null ? 0 : int.Parse(attr.Value);
    }
}

public partial class SkillHarm : SkillTarget
{
    [AraleSerizlize.Field]
    public int harm;
    public override void read(BinaryReader r)
    {
        base.read(r);
        harm = r.ReadInt32();
    }

    public override void write(BinaryWriter w)
    {
        base.write(w);
        w.Write(harm);
    }

    public override void read(XmlNode n)
    {
        base.read(n);
        harm = int.Parse(n.Attributes["harm"].Value);
    }
}

public partial class SkillBuff : SkillTarget
{
    [AraleSerizlize.Field]
    public int id;
    public override void read(BinaryReader r)
    {
        base.read(r);
        id = r.ReadInt32();;
    }

    public override void write(BinaryWriter w)
    {
        base.write(w);
        w.Write(id);
    }

    public override void read(XmlNode n)
    {
        base.read(n);
        id = int.Parse(n.Attributes["id"].Value);
    }
}

public partial class SkillEvent : SkillNode
{
    [AraleSerizlize.Field]
    public string evt="";
    public override void read(BinaryReader r)
    {
        base.read(r);
        evt = r.ReadString();
    }

    public override void write(BinaryWriter w)
    {
        base.write(w);
        w.Write(evt);
    }

    public override void read(XmlNode n)
    {
        base.read(n);
        evt = n.Attributes["evt"].Value;
    }
}

public partial class SkillMove : SkillNode
{
    [AraleSerizlize.Field]
    public int id;
    public override void read(BinaryReader r)
    {
        base.read(r);
        id = r.ReadInt32();;
    }

    public override void write(BinaryWriter w)
    {
        base.write(w);
        w.Write(id);
    }

    public override void read(XmlNode n)
    {
        base.read(n);
        id = int.Parse(n.Attributes["id"].Value);
    }
}

public partial class SkillLua : SkillNode
{
    [AraleSerizlize.Field]
    public int evt;
    [AraleSerizlize.Field]
    public string param;
    public override void read(BinaryReader r)
    {
        base.read(r);
        evt = r.ReadInt32();
        param = r.ReadString();
    }

    public override void write(BinaryWriter w)
    {
        base.write(w);
        w.Write(evt);
        w.Write(param);
    }

    public override void read(XmlNode n)
    {
        base.read(n);
        evt = int.Parse(n.Attributes["evt"].Value);
        XmlAttribute attr = n.Attributes["param"];
        param = attr == null ? null:attr.Value;
    }
}
