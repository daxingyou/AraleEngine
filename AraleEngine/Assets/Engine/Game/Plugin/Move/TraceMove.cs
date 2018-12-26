using UnityEngine;
using System.Collections;

public class TraceMove : Move
{
	uint mTarget;
	Vector3 mPos;
	float mSpeed;
	protected override void init(Unit unit)
	{
		mTarget = uTarget;
		mPos = unit.pos;
		mSpeed = table.speed;
	}

	protected override void update(Unit unit)
	{
		Unit u = unit.mgr.getUnit (mTarget);
		if (u != null)mPos = u.pos;
		Vector3 dv = mPos - unit.pos;
		if (dv.sqrMagnitude <= mSpeed * mSpeed)
		{
			arrived ();
		}
		else
		{
			Vector3 dir = dv.normalized;
			unit.dir  = dir;
			unit.pos += dir * mSpeed;
		}
	}
}
