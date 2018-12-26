using UnityEngine;
using System.Collections;

public class DirMove : Move
{
	Vector3 mDir;
	float   mDistance;
	float   mSpeed;
	protected override void init(Unit unit)
	{
		mDir = vTarget;
		mSpeed = table.speed;
		mDistance = table.life;
	}

	protected override void update(Unit unit)
	{
		if (mDistance <= 0)
		{//达到飞行距离
			arrived();
		}
		else
		{
			Vector3 d = mDir * mSpeed;
			mDistance -= d.magnitude;
			unit.pos += d;
		}
	}
}
