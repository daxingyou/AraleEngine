using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UICDImage : Image
{
    float mDur;
    float mTime;
	public void Play(float dur, float elapse = 0f)
    {
		enabled = true;
		mTime = elapse;
		mDur = dur;
		Update ();
    }

    void Update ()
    {
		if (mTime >= mDur)
		{
			enabled = false;
			return;
		}
        this.fillAmount = 1 - mTime / mDur;
        mTime += Time.unscaledDeltaTime;
    }

	public bool isCD{get{return mTime < mDur;}}
}
