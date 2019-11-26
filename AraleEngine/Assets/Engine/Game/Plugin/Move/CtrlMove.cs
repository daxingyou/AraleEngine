using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Arale.Engine;
using System;

public class CtrlMove : Move
{
    Vector3 mDir;
    protected override void start(Unit unit)
    {
        if (!unit.isState(UnitState.Move))return;
        mSpeed = unit.speed;
        unit.move.moveState = State.Run;
        if (mDir != vTarget)
        {
            unit.dir = mDir = vTarget;
            sync(unit);
        }
    }

    protected override void update(Unit unit)
    {
        if (!unit.isState(UnitState.Move))return;
        mSpeed = unit.speed;
        unit.setDir(vTarget);
        unit.pos += Time.deltaTime * vTarget * mSpeed;
    }

    protected override void stop(Unit unit, bool arrived)
    {
        mDir = Vector3.zero;
        unit.move.moveState = State.None;
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
        msg.speed = vTarget;
        msg.navState = (byte)unit.move.moveState;
        unit.sendMsg((short)MyMsgId.Nav, msg);
    }
}
