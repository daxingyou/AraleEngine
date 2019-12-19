using UnityEngine;
using System.Collections;

public class TraceMove : Move
{
	protected override void start(Unit unit)
	{
        unit.move.moveState = State.Move;
        vTarget = unit.hitPos;
		mSpeed = table.speed;
	}

	protected override void update(Unit unit)
	{
        Unit u = uTarget;
        if (u != null)vTarget = u.hitPos;
        Vector3 dv = vTarget - unit.hitPos;
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
