using UnityEngine;
using System.Collections;

public class PosMoveAnim : MoveAnim {
	public Vector3 mSrc;
	public Vector3 mDst;
	protected override void Update ()
	{
		float md = mSpeed * Time.deltaTime;
		Vector3 d = mDst - mSrc;
		Vector3 pos = mTrans.position+d.normalized*md;
        if(IsArrive(ref pos, ref mDst))
		{
			SendEvent(AnimEvent.End);
		}
	}
}
