using UnityEngine;
using System.Collections;

public class TraceMove : Move
{
	protected override void start(Unit unit)
	{
        unit.move.moveState = State.Move;
        vTarget = unit.pos;
		mSpeed = table.speed;
	}

	protected override void update(Unit unit)
	{
        Unit u = uTarget;
        if (u != null)vTarget = u.pos;
        Vector3 dv = vTarget - unit.pos;
		if (dv.sqrMagnitude <= mSpeed * mSpeed)
		{
            stop(unit,true);
		}
		else
		{
			Vector3 dir = dv.normalized;
			unit.dir  = dir;
			unit.pos += dir * mSpeed;
		}
	}
}
