using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UICDImage : Image
{
    float mDur=1f;
    float mTime;
    public void play(float dur, float start = 0f)
    {
        
    }

    void Update ()
    {
        if (mTime < 0)return;
        this.fillAmount = mTime / mDur;
        mTime -= Time.unscaledDeltaTime;
    }
}
