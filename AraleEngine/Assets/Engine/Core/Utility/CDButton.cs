using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent (typeof (Button))]
public class CDButton : MonoBehaviour, IPointerClickHandler
{
    public Image mMask;
    public float mCDTime;
    float mTime;
    Button mButton;
	// Use this for initialization
	void Start () {
        if(mCDTime>0)mMask.fillAmount = mTime / mCDTime;
	}
	
	// Update is called once per frame
	void Update () {
        if (mTime > 0)
        {
            mMask.fillAmount = mTime / mCDTime;
            mTime -= Time.unscaledDeltaTime;
        }

	}

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isCD)return;
        mTime = mCDTime;
    }

    public bool isCD
    {
        get{return mTime > 0;}
    }
}
