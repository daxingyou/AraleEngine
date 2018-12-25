using UnityEngine;
using System.Collections;

public class TraceMoveAnim : MoveAnim {
	public Vector3 mSrc;
	public Transform mTarget;
	protected override void Update ()
	{
		if (null == mTarget)
		{
			SendEvent(AnimEvent.End);
			return;
		}

		Vector3 dst = mTarget.position;
		Vector3 d = dst - mTrans.position;
		float md =  mSpeed * Time.deltaTime;
		Vector3 pos = mTrans.position + d.normalized * md;
		if(IsArrive(ref pos, ref dst))
		{
			SendEvent(AnimEvent.End);
		}
	}
}
