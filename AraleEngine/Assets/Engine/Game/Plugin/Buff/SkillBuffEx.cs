using UnityEngine;
using System.Collections;
using Arale.Engine;
using System.Collections.Generic;

public class SkillBuffEx : Buff
{
    Unit mUnit;
    GameSkill mGS;
    GameSkill.Action mCurAction;
    protected override void onInit(Unit unit)
    {
        mUnit  = unit;
        mUnit.sendUnitEvent ((int)UnitEvent.SkillBegin,null,true);
        string path = table.lua;
        int idx = path.LastIndexOf('/');
        mGS = GameSkill.get(path.Remove(idx), path.Substring(idx + 1));
        mUnitState = mGS.initState;
        mUnit.forward (mUnit.skill.targetPos);
        if (string.IsNullOrEmpty(mGS.initAnim))
        {
            mUnit.anim.sendEvent(AnimPlugin.PlayAnim, mGS.initAnim);
        }

        for (int i = 0,max=mGS.actions.Count; i<max; ++i)
        {
            GameSkill.Action act = mGS.actions[i];
            TimeMgr.Action a = timer.AddAction (new TimeMgr.Action ());
            a.doTime   = act.time;
            a.onAction = onAction;
            a.userData = act;
        }
        state = 1;
    }

    void onAction(TimeMgr.Action a)
    {
        GameSkill.Action act = a.userData as GameSkill.Action;
        mCurAction = act;
        mUnitState = act.state;
        if (string.IsNullOrEmpty(act.anim))
        {
            mUnit.anim.sendEvent(AnimPlugin.PlayAnim, act.anim);
        }

        for (int i = 0; i < act.bullets.Count; ++i)
        {
            bulletProcess(act.bullets[i]);
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

    void bulletProcess(GameSkill.Bullet b)
    {
        if (b.id == 0)
        {//直接效果
            if (b.target.type == GameSkill.Target.Type.Area)
            {
                GameSkill.AreaTarget t = b.target as GameSkill.AreaTarget;
                IArea garea = GameArea.fromString (t.area);
                Matrix4x4 mt = Matrix4x4.TRS (mUnit.pos, Quaternion.LookRotation (mUnit.dir), Vector3.one).inverse;
                List<Unit> units = mUnit.mgr.getUnitInArea (UnitType.Monster|UnitType.Player, garea, mt);
                for (int i = 0; i < units.Count; ++i)
                {
                    Unit u = units [i];
                    if ((b.target.relation & UnitRelation.Self) == 0 && u.guid == mUnit.guid)continue;
                    affectUnit(b, u);
                }
            }
            else
            {
                if ((b.target.relation & UnitRelation.Self) != 0)
                {
                    affectUnit(b, mUnit);
                }
                Unit u = mUnit.skill.targetUnit;
                if (u == null)return;
                affectUnit(b, u);
            }
        }
        else
        {//子弹效果
            Bullet bt = mUnit.mgr.getUnit (0, UnitType.Bullet, b.id) as Bullet;
            bt.setParam (mUnit.pos, mUnit.dir, mUnit);
            bt.mHarm = b.harm;
            bt.mOwner = mUnit.guid;
            if (b.buffId>0)
            {
                bt.buff.addBuff(b.buffId, mUnit);
            }

            switch (b.target.type)
            {
                case GameSkill.Target.Type.None:
                    switch (b.target.noneType)
                    {
                        case GameSkill.Target.NoneType.Dir:
                            Vector3 v = mUnit.skill.targetPos;
                            v.y = bt.pos.y;
                            bt.play ((v - bt.pos).normalized, mUnit.skill.targetGUID);
                            break;
                        case GameSkill.Target.NoneType.Unit:
                            bt.play (mUnit.skill.targetPos, mUnit.skill.targetGUID);
                            break;
                        default:
                            bt.play (mUnit.skill.targetPos, mUnit.skill.targetGUID);
                            break;
                    }
                    break;
                case GameSkill.Target.Type.Dir:
                    {
                        GameSkill.VecctorTarget t = b.target as GameSkill.VecctorTarget;
                        bt.play (t.local?bt.transform.localToWorldMatrix.MultiplyVector(t.vct):t.vct, mUnit.skill.targetGUID);
                        break;
                    }
                case GameSkill.Target.Type.Pos:
                    {
                        GameSkill.VecctorTarget t = b.target as GameSkill.VecctorTarget;
                        bt.play (t.local?bt.transform.localToWorldMatrix.MultiplyPoint(t.vct):t.vct, mUnit.skill.targetGUID);
                        break;
                    }
                case GameSkill.Target.Type.Area:
                    {
                        GameSkill.AreaTarget t = b.target as GameSkill.AreaTarget;
                        //bt.play (t.local?bt.transform.localToWorldMatrix.MultiplyPoint(t.vct):t.vct, mUnit.skill.targetGUID);
                        break;
                    }
            }
        }
    }

    void affectUnit(GameSkill.Bullet bt, Unit unit, IArea area=null)
    {
        bool notSelf = unit.guid != mUnit.guid;
        if (bt.buffId != 0)
        {
            unit.buff.addBuff(bt.buffId, mUnit);
        }

        if (bt.harm != 0)
        {//bt.harm<0为增益
            if (bt.harm > 0&&notSelf)
            {
                unit.dir = (mUnit.pos - unit.pos).normalized;
                unit.anim.sendEvent(AnimPlugin.Hit);
            }
            AttrPlugin ap = unit.attr;
            ap.HP -= bt.harm;
            ap.sync(); 
            unit.sendUnitEvent((int)UnitEvent.BeHit, mUnit.guid, true);
        }

        if (bt.moveId == 0)return;
        if(notSelf)unit.dir = (mUnit.pos - unit.pos).normalized;
        if(bt.moveId>0)
        {//击退到攻击范围的最远点
            unit.move.play(bt.moveId,mUnit.pos-area.R*unit.dir,null,true);
        }
        else
        {//击退指定距离
            unit.move.play(Mathf.Abs(bt.moveId),-unit.dir,null,true);
        }
    }

    protected override void onDeinit()
    {
        mCurAction = null;
        mUnitState = UnitState.ALL;
        mUnit = null;
        mUnit.sendUnitEvent ((int)UnitEvent.SkillEnd, null, true);
    }
}
