using UnityEngine;
using System.Collections;
using Arale.Engine;

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

            Vector3 v = mUnit.skill.targetPos;
            v.y = bt.pos.y;
            if (true)
            {//第一个参数为位置
                bt.play (mUnit.skill.targetPos, mUnit.skill.targetGUID);
            }
            else
            {//第一个参数为方向
                bt.play ((v - bt.pos).normalized, mUnit.skill.targetGUID);
            }
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
