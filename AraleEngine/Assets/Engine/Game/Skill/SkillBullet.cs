using UnityEngine;
using System.Collections;
using System.IO;

public partial class SkillBullet : SkillNode
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
    public SkillTarget target = new SkillTarget();

    public override void read(BinaryReader r)
    {
        id = r.ReadInt32();
        harm = r.ReadInt32();
        buffId = r.ReadInt32();
        moveId = r.ReadInt32();
        mode = (Mode)r.ReadInt32();
        target = SkillTarget.readType(r);
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