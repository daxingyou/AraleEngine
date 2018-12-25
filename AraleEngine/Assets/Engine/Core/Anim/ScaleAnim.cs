using UnityEngine;
using System.Collections;

public class ScaleAnim : Anim {
	public Transform mTrans;
	public Vector3 mStart;
	public Vector3 mEnd;
	public AnimationCurve mAC;
	protected override void Update ()
	{
		if(mElapse<mDuration)
		{
			mTrans.localScale = mElapse / mDuration * (mEnd - mStart);
		}
		else
		{
			mElapse = mDuration;
			mTrans.localScale = mEnd;
			SendEvent(AnimEvent.End);
		}
	}
}
