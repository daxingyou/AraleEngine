using UnityEngine;
using System.Collections;

public class DirMove : Move
{
	float   mDistance;
	protected override void start(Unit unit)
	{
		mSpeed = table.speed;
		mDistance = table.life;
	}

	protected override void update(Unit unit)
	{
		if (mDistance <= 0)
		{//达到飞行距离
            stop(unit,true);
		}
		else
		{
            Vector3 d = vTarget * mSpeed;
			mDistance -= d.magnitude;
			unit.pos += d;
		}
	}
}
