using UnityEngine;
using System.Collections;

public class MoveAnim : Anim {
	public Transform mTrans;
	public float mSpeed;
	protected bool IsArrive(ref Vector3 pos, ref Vector3 dst)
	{
		Vector3 cpos = mTrans.position;
		float d1 = (pos - cpos).sqrMagnitude;
		float d2 = (dst - cpos).sqrMagnitude;
		if(d1<d2)
		{
			mTrans.position = pos;
			return false;
		}
		else
		{
			mTrans.position = dst;
			return true;
		}
	}
}
