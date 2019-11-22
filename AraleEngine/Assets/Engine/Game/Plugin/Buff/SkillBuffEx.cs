using UnityEngine;
using System.Collections;
using Arale.Engine;
using System.Collections.Generic;

public class GameSkillBuffEx : Buff
{
    Unit mUnit;
    GameSkill mGS;
    SkillAction mCurAction;
    protected override void onInit(Unit unit)
    {
        mUnit  = unit;
        mUnit.sendUnitEvent ((int)UnitEvent.SkillBegin,null,true);
        string path = table.lua;
        int idx = path.LastIndexOf('/');
        mGS = GameSkill.get(path.Substring(idx + 1), path.Remove(idx));
        mUnitState = mGS.state;
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
        mUnitState = act.state;
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
        if (n.target.type == SkillTarget.Type.Area)
        {
            SkillAreaTarget t = n.target as SkillAreaTarget;
            IArea garea = GameArea.fromString (t.area);
            Matrix4x4 mt = Matrix4x4.TRS (mUnit.pos, Quaternion.LookRotation (mUnit.dir), Vector3.one).inverse;
            List<Unit> units = mUnit.mgr.getUnitInArea (UnitType.Monster|UnitType.Player, garea, mt);
            for (int i = 0; i < units.Count; ++i)
            {
                
                Unit u = units [i];
                if ((n.target.relation & UnitRelation.Self) == 0 && u.guid == mUnit.guid)continue;
                affectUnit(u, n);
            }
        }
        else
        {
            if ((n.target.relation & UnitRelation.Self) != 0)
            {
                affectUnit(mUnit, n);
            }
            Unit u = mUnit.skill.targetUnit;
            if (u == null)return;
            affectUnit(u, n);
        }
    }

    void bulletProcess(SkillBullet n)
    {
        Bullet bt = mUnit.mgr.getUnit (0, UnitType.Bullet, n.id) as Bullet;
        bt.setParam (mUnit.pos, mUnit.dir, mUnit);
        bt.mOwner = mUnit.guid;
        switch (n.target.type)
        {
            case SkillTarget.Type.None:
                switch (n.target.noneType)
                {
                    case SkillTarget.NoneType.Dir:
                        Vector3 v = mUnit.skill.targetPos;
                        v.y = bt.pos.y;
                        bt.play ((v - bt.pos).normalized, mUnit.skill.targetGUID);
                        break;
                    case SkillTarget.NoneType.Unit:
                        bt.play (mUnit.skill.targetPos, mUnit.skill.targetGUID);
                        break;
                    default:
                        bt.play (mUnit.skill.targetPos, mUnit.skill.targetGUID);
                        break;
                }
                break;
            case SkillTarget.Type.Dir:
                {
                    SkillVecctorTarget t = n.target as SkillVecctorTarget;
                    bt.play (t.local?bt.transform.localToWorldMatrix.MultiplyVector(t.vct):t.vct, mUnit.skill.targetGUID);
                    break;
                }
            case SkillTarget.Type.Pos:
                {
                    SkillVecctorTarget t = n.target as SkillVecctorTarget;
                    bt.play (t.local?bt.transform.localToWorldMatrix.MultiplyPoint(t.vct):t.vct, mUnit.skill.targetGUID);
                    break;
                }
            case SkillTarget.Type.Area:
                {
                    SkillAreaTarget t = n.target as SkillAreaTarget;
                    //bt.play (t.local?bt.transform.localToWorldMatrix.MultiplyPoint(t.vct):t.vct, mUnit.skill.targetGUID);
                    break;
                }
        }
    }

    void buffProcess(SkillBuff n)
    {
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
        bool notSelf = u.guid != mUnit.guid;
        if (n.harm > 0&&notSelf)
        {
            u.dir = (mUnit.pos - u.pos).normalized;
            u.anim.sendEvent(AnimPlugin.Hit);
        }
        AttrPlugin ap = u.attr;
        ap.HP -= n.harm;
        ap.sync(); 
        u.sendUnitEvent((int)UnitEvent.BeHit, mUnit.guid, true);
        if(notSelf)u.dir = (mUnit.pos - u.pos).normalized;
    }

    protected override void onDeinit()
    {
        mCurAction = null;
        mUnitState = UnitState.ALL;
        mUnit.sendUnitEvent ((int)UnitEvent.SkillEnd, null, true);
        mUnit = null;
    }
}
