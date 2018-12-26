using UnityEngine;
using System.Collections;

public class PosMove : Move
{
	Vector3 mPos;
	float   mSpeed;
	protected override void init(Unit unit)
	{
		mPos = vTarget;
		mSpeed = table.speed;
		if(mSpeed == 0)
		{
			unit.dir = (mPos - unit.pos).normalized;
			unit.pos = mPos;
			arrived();
		}
	}


	protected override void update(Unit unit)
	{
		Vector3 dv = mPos - unit.pos;
		if (dv.sqrMagnitude <= mSpeed*mSpeed)
		{//达到目的地
			unit.pos = mPos;
			arrived ();
		}
		else
		{
			unit.pos += dv.normalized* mSpeed;
		}
	}
}
