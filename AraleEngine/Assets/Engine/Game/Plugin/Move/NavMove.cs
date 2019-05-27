using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Arale.Engine;
using System;

public class NavMove : Move {
    public  NavMeshAgent mAgent;
    public  Action<bool>  mCallback;
    Vector3 mNavDir;
	// Use this for initialization
    protected override void start(Unit unit)
    {
        mNavDir= Vector3.zero;
        mSpeed = unit.speed;
        mAgent.enabled = true;
        mAgent.SetDestination(vTarget);
        unit.move.moveState = State.Run;
        unit.sendUnitEvent ((int)UnitEvent.NavBegin, 0);
    }

    bool isArrive()
    {
        if(!mAgent.enabled || mAgent.pathPending)
            return false;
        if(mAgent.remainingDistance <= mAgent.stoppingDistance)
            return true;
        return false;
    }

    protected override void update(Unit unit)
    {
        mSpeed = unit.speed;
        if (isArrive ())
        {
            stop (unit, true);
        }
        else
        {//自己控制角色方向和移动
            updateBySelf (unit, mSpeed);    
        }

        //目标跟踪
        if (uTarget != null && uTarget.isState (UnitState.Alive))
        {
            mAgent.SetDestination (uTarget.pos);
        }
        else
        {
            uTarget = null;
        }
    }

    void updateBySelf(Unit unit, float speed)
    {//导航Agent存在加速度问题,在同步时会产生速度方向不一致抖动的效果
        //要自己同步需将角度加速度和移动加速度都设置为0
        if (mAgent.angularSpeed > 0f || mAgent.acceleration > 0f)return;
        Vector3 dir = (mAgent.steeringTarget - unit.pos).normalized;
        if (dir != mNavDir)
        {//关键帧通知
            unit.setDir(mNavDir = dir);
            unit.move.moveState = State.Run;
            sync (unit);
        }

        float d = Time.deltaTime * speed;
        if (d >= Vector3.Distance (mAgent.steeringTarget, unit.pos))
        {
            unit.pos = mAgent.steeringTarget;
        }
        else
        {
            unit.pos += d * unit.dir;
        }
        mAgent.nextPosition = unit.pos;
    }

    protected override void stop(Unit unit, bool arrived)
    {
        if (mAgent.enabled==false)return;
        if (mCallback != null)mCallback(arrived);
        mCallback = null;
        mAgent.Stop ();
        mAgent.enabled = false;
        unit.move.moveState= State.None;
        vTarget  = Vector3.zero;
        uTarget  = null;
        unit.sendUnitEvent((int)UnitEvent.NavEnd, 0);
        sync(unit);
    }

    protected override void sync(Unit unit)
    {
        if (!unit.isAgent)return;
        MsgNav msg = new MsgNav();
        msg.guid  = unit.guid;
        msg.time  = RTime.R.utcTickMs;
        msg.state = unit.state;
        msg.pos   = unit.pos;
        msg.dir   = unit.dir;
        msg.speed = mNavDir;
        msg.navState = (byte)unit.move.moveState;
        unit.sendMsg((short)MyMsgId.Nav, msg);
    }

    int getOffMeshLinkAction(Unit unit)
    {
        if (!mAgent.isOnOffMeshLink)return 0;
        OffMeshLinkData lk = mAgent.currentOffMeshLinkData;
        mAgent.autoTraverseOffMeshLink = false;
        Vector3 b = lk.startPos;
        Vector3 e = lk.endPos;
        if (Vector3.Distance (unit.pos, b) > Vector3.Distance (unit.pos, e))
        {
            b = lk.endPos;
            e = lk.startPos;
        }
        int lnkType = lk.offMeshLink.area;
        switch (lnkType)
        {
            case 2:
                //mUnit.StartCoroutine (jump (b,e));
                return 1;
            case 3:
                //mUnit.StartCoroutine (climb (b,e));
                return 1;
        }
        return 0;
    }
}
