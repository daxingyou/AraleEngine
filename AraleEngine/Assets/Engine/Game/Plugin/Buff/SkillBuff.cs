using UnityEngine;
using System.Collections;
using Arale.Engine;
using System.Collections.Generic;

public class GameSkillBuff : Buff
{
    public GameSkillBuff(GameSkill gs)
    {
        mGS = gs;
    }

    GameSkill mGS;
    SkillAction mCurAction;
    protected override void onInit(Unit unit)
    {
        mTB = TableMgr.single.GetData<TBBuff>(mGS.buff);
        mUnit.sendUnitEvent ((int)UnitEvent.SkillBegin,null,true);
        setUnitState(mGS.state,true);
        mUnit.forward (mUnit.skill.targetPos);
        if (!string.IsNullOrEmpty(mGS.anim))
        {
            mUnit.anim.sendEvent(AnimPlugin.PlayAnim, mGS.anim);
        }

        for (int i = 0,max=mGS.actions.Count; i<max; ++i)
        {
            SkillAction act = mGS.actions[i];
            TimeMgr.Action a = timer.AddAction (new TimeMgr.Action ());
            a.doTime   = act.time;
            a.onAction = onAction;
            a.userData = act;
        }
        state = 1;
    }

    void onAction(TimeMgr.Action a)
    {
        SkillAction act = a.userData as SkillAction;
        mCurAction = act;
        setUnitState(act.state,true);
        for (int i = 0; i < act.nodes.Count; ++i)
        {
            SkillNode n = act.nodes[i];
            switch (n.type)
            {
                case SkillNode.Type.Anim:
                    break;
                case SkillNode.Type.Harm:
                    harmProcess(n as SkillHarm);
                    break;
                case SkillNode.Type.Bullet:
                    bulletProcess(n as SkillBullet);
                    break;
                case SkillNode.Type.Buff:
                    buffProcess(n as SkillBuff);
                    break;
                case SkillNode.Type.Event:
                    eventProcess(n as SkillEvent);
                    break;
                case SkillNode.Type.Move:
                    moveProcess(n as SkillMove);
                    break;
            }
        }

        if (act.loopTimes-- > 0)
        {
            a.Loop(act.loopInterval);
        }
        else if (act.end)
        {
            state = 0;
        }
    }

    void animProcess(SkillAnim n)
    {
        mUnit.anim.sendEvent(AnimPlugin.PlayAnim, n.anim);
    }

    void harmProcess(SkillHarm n)
    {
        if (n.target == SkillTarget.Target.Area)
        {//区域伤害
            IArea garea = GameArea.fromString (n.area);
            Matrix4x4 mt = Matrix4x4.TRS (mUnit.pos, Quaternion.LookRotation (mUnit.dir), Vector3.one).inverse;
            List<Unit> units = mUnit.mgr.getUnitInArea (UnitType.Monster|UnitType.Player, garea, mt);
            for (int i = 0; i < units.Count; ++i)
            {
                Unit u = units [i];
                if (!mUnit.isRelation(u, (int)n.relation))continue;
                affectUnit(u, n);
            }
        }
        else
        {//目标伤害
            if ((n.relation & UnitRelation.Self)!=0)affectUnit(mUnit, n);
            Unit u = mUnit.skill.targetUnit;
            if (u != null && mUnit.isRelation(u, (int)n.relation))affectUnit(u, n);
        }
    }

    void bulletProcess(SkillBullet n)
    {
        switch (n.target)
        {
            case SkillTarget.Target.Unit:
                {
                    Bullet bt = mUnit.mgr.getUnit (0, UnitType.Bullet, n.id) as Bullet;
                    bt.setParam (mUnit.pos, mUnit.dir, mUnit);
                    bt.mOwner = mUnit.guid;
                    bt.mHarm = n.harm;
                    Vector3 projDir = n.locationDir(mUnit);
                    projDir.y = 0;
                    bt.play (projDir.normalized, mUnit.skill.targetGUID);
                    break;
                }
            case SkillTarget.Target.Dir:
                {
                    Bullet bt = mUnit.mgr.getUnit (0, UnitType.Bullet, n.id) as Bullet;
                    bt.setParam (mUnit.pos, mUnit.dir, mUnit);
                    bt.mOwner = mUnit.guid;
                    bt.mHarm = n.harm;
                    Vector3 projDir = n.locationDir(mUnit);
                    projDir.y = 0;
                    bt.play (projDir.normalized, mUnit.skill.targetGUID);
                    break;
                }
            case SkillTarget.Target.Pos:
                {
                    Bullet bt = mUnit.mgr.getUnit (0, UnitType.Bullet, n.id) as Bullet;
                    bt.setParam (mUnit.pos, mUnit.dir, mUnit);
                    bt.mOwner = mUnit.guid;
                    bt.mHarm = n.harm;
                    bt.play (n.locationPos(mUnit), mUnit.skill.targetGUID);
                    break;
                }
            case SkillTarget.Target.Area:
                {
                    IArea garea = GameArea.fromString (n.area);
                    Matrix4x4 mt = Matrix4x4.TRS (mUnit.pos, Quaternion.LookRotation (mUnit.dir), Vector3.one).inverse;
                    List<Unit> units = mUnit.mgr.getUnitInArea (UnitType.Monster|UnitType.Player, garea, mt);
                    for (int i = 0; i < units.Count; ++i)
                    {
                        Bullet bt = mUnit.mgr.getUnit (0, UnitType.Bullet, n.id) as Bullet;
                        bt.setParam (mUnit.pos, mUnit.dir, mUnit);
                        bt.mOwner = mUnit.guid;
                        bt.mHarm = n.harm;
                        bt.play (units[i].pos, units[i].guid);
                    }
                    break;
                }
        }
    }

    void buffProcess(SkillBuff n)
    {
        if (n.target == SkillTarget.Target.Area)
        {
            IArea garea = GameArea.fromString (n.area);
            Matrix4x4 mt = Matrix4x4.TRS (mUnit.pos, Quaternion.LookRotation (mUnit.dir), Vector3.one).inverse;
            List<Unit> units = mUnit.mgr.getUnitInArea (UnitType.Monster|UnitType.Player, garea, mt);
            for (int i = 0; i < units.Count; ++i)
            {
                Unit u = units [i];
                if (!mUnit.isRelation(u, (int)n.relation))continue;
                u.buff.addBuff(n.id);
            }
        }
        else
        {
            if ((n.relation & UnitRelation.Self)!=0)mUnit.buff.addBuff(n.id);
            Unit u = mUnit.skill.targetUnit;
            if (u != null && mUnit.isRelation(u, (int)n.relation))u.buff.addBuff(n.id);
        }
    }

    void eventProcess(SkillEvent n)
    {
        EventMgr.single.SendEvent(n.evt);
    }

    void moveProcess(SkillMove n)
    {
        if(n.id>0)
        {//击退到攻击范围的最远点
            //unit.move.play(bt.moveId,mUnit.pos-area.R*unit.dir,null,true);
        }
        else
        {//击退指定距离
            //unit.move.play(Mathf.Abs(bt.moveId),-unit.dir,null,true);
        }
    }

    void affectUnit(Unit u, SkillHarm n)
    {
        if (u == null)return;
        if (n.harm > 0)
        {
            u.anim.sendEvent(AnimPlugin.Hit);
        }
        AttrPlugin ap = u.attr;
        ap.HP -= n.harm;
        ap.sync(); 
        u.sendUnitEvent((int)UnitEvent.BeHit, mUnit.guid, true);
    }

    protected override void onDeinit()
    {
        mUnit.syncState();
        mUnit.sendUnitEvent ((int)UnitEvent.SkillEnd, null, true);
    }
}
