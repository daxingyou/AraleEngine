using UnityEngine;
using System.Collections;

public class JumpMove : Move
{
	float   mHeight;
	float   mTime;
	Vector3 mPos;
	protected override void start(Unit unit)
	{
		mPos = unit.pos;
		mHeight  = float.Parse (table.param);
	}

	protected override void update(Unit unit)
	{
		if (table.speed != 0)
			dirUpdate (unit);
		else
			posUpdate (unit);
	}

	void dirUpdate(Unit unit)
    {
		Vector3 v = unit.pos;
		float k = mTime / table.life;
		v = mPos+vTarget*table.speed*mTime;
		v.y= 4*mHeight*k - 4*mHeight*k*k;
		unit.pos = v;
		mTime+=Time.unscaledDeltaTime;
		if(mTime>=table.life)
		{
            stop(unit,true);
		}
	}

	void posUpdate(Unit unit)
    {
		Vector3 v = unit.pos;
		float k = mTime / table.life;
		v = mPos + k*(vTarget - mPos);
		v.y= 4*mHeight*k - 4*mHeight*k*k;
		unit.pos = v;
		mTime+=Time.unscaledDeltaTime;
		if(mTime>=table.life)
		{
			unit.pos = vTarget;
            stop(unit,true);
		}
	}
}