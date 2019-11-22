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
    };
    public Type type; 
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
} 

public partial class SkillBullet : SkillNode
{
    public enum Mode
    {
        None,
        Scatter,//散射
        Chain,  //链式
    }

    public int id;
    public Mode mode;
    public SkillTarget target = new SkillTarget();

    public override void read(BinaryReader r)
    {
        base.read(r);
        id = r.ReadInt32();
        mode = (Mode)r.ReadInt32();
        target = SkillTarget.readType(r);
    }

    public override void write(BinaryWriter w)
    {
        base.write(w);
        w.Write(id);
        w.Write((int)mode);
        target.write(w);
    }
}

public partial class SkillHarm : SkillNode
{
    public int harm;
    public SkillTarget target = new SkillTarget();

    public override void read(BinaryReader r)
    {
        base.read(r);
        harm = r.ReadInt32();
        target = SkillTarget.readType(r);
    }

    public override void write(BinaryWriter w)
    {
        base.write(w);
        w.Write(harm);
        target.write(w);
    }

    public override void read(XmlNode n)
    {
        base.read(n);
        harm = int.Parse(n.Attributes["harm"].Value);
        target = SkillTarget.readType(n.ChildNodes[0]);
    }
}

public partial class SkillBuff : SkillNode
{
    public int id;
    public SkillTarget target = new SkillTarget();

    public override void read(BinaryReader r)
    {
        base.read(r);
        id = r.ReadInt32();;
        target = SkillTarget.readType(r);
    }

    public override void write(BinaryWriter w)
    {
        base.write(w);
        w.Write(id);
        target.write(w);
    }
}

public partial class SkillEvent : SkillNode
{
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
}

public partial class SkillMove : SkillNode
{
    public int id;
    public SkillTarget target = new SkillTarget();

    public override void read(BinaryReader r)
    {
        base.read(r);
        id = r.ReadInt32();;
        target = SkillTarget.readType(r);
    }

    public override void write(BinaryWriter w)
    {
        base.write(w);
        w.Write(id);
        target.write(w);
    }
}
