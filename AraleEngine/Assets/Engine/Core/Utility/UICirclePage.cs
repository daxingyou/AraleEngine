//环形旋转，伪3d径深翻页组件
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class UICirclePage : MonoBehaviour,  IBeginDragHandler, IDragHandler, IEndDragHandler{
    public float mSpeed=120f;//每秒120度 
    public float mResistanceSpeed=640;//阻速，每秒度速
    public int   mRadius=640;
    public float mAng;
    public Camera mUICamera;
    float mUnitAng;
    float mIneriaSpeed;
    float mLastUpdateAng;
	Transform[] mEnters;
	List<Transform> mSort = new List<Transform>();
    Coroutine mCon;
    bool mMultiTouch;
	// Use this for initialization
	void Start () {
		Debug.Assert(mUICamera!=null);
        reset();
        updatePos();
	}

    void OnEnable()
    {
        mMultiTouch = Input.multiTouchEnabled;
        Input.multiTouchEnabled = false;
        mAng = 0;
    }

    void OnDisable()
    {
        Input.multiTouchEnabled = mMultiTouch;
    }

    public void reset(){
        mSort.Clear();
		mEnters = GetComponentsInChildren<Transform>();
        mSort.AddRange(mEnters); 
        mUnitAng = 360f / mEnters.Length;
        mAng = 0;
    }
	
    void updatePos(){
        Vector3 op = new Vector3(0, 0, -mRadius);
        for (int i = 0,max=mEnters.Length; i<max; ++i)
        {
            Transform t = mEnters[i].transform;
            float rd = (mAng + i*mUnitAng) * Mathf.Deg2Rad;
            float z = -Mathf.Cos(rd) * mRadius;//z越小越靠近相机
            float x = Mathf.Sin(rd) * mRadius;
            t.localPosition = new Vector3(x, 0, z);
            float d = Vector3.Distance(op, t.localPosition);
            t.localScale = Vector3.one*(1 - 0.2f*(d+mRadius) / (2*mRadius));
        }

		mSort.Sort(delegate(Transform a, Transform b)
            {
                return (a.localPosition.z -  b.localPosition.z>0)?-1:1;
            });
        for (int i = 0, max = mSort.Count; i < max; ++i)
        {
            mSort[i].transform.SetSiblingIndex(i);
        }
        mLastUpdateAng = mAng;
    }

	// Update is called once per frame
	void Update () {
        if (mIneriaSpeed != 0)
        {
            float k = mIneriaSpeed > 0 ? 1 : -1;
            mAng += mIneriaSpeed * Time.deltaTime;
            mIneriaSpeed -= k*mResistanceSpeed * Time.deltaTime;
            if (k * mIneriaSpeed < 0)
            {
                mIneriaSpeed = 0;
                mCon = StartCoroutine(anim(clamp(mAng)));  
            }
        }
        if (mLastUpdateAng == mAng)return;
        updatePos();
	}

    public void flip(bool left)
    {
        if (mCon!=null)return;
        float endAng = left?mAng-mUnitAng:mAng+mUnitAng;
        mCon = StartCoroutine(anim(clamp(endAng)));
    }

    float clamp(float ang)
    {//限定到正对角度
        int n = (int)(ang / mUnitAng);
        float t = n * mUnitAng;
        if (Mathf.Abs(ang - t) < 0.5f * mUnitAng)
        {//未过半
            return t;
        }
        else
        {
            return ang > t ? t + mUnitAng : t - mUnitAng;
        }
    }

    float clampTo2PI(float ang)
    {
        int n = (int)(ang/360f);
        ang = ang - n * 360f;
        if (ang < 0)ang += 360f;
        return ang;
    }

    IEnumerator anim(float endAng)
    {
        float dur = Math.Abs(endAng - mAng)/mSpeed;
        Debug.LogError("b="+mAng+",e="+endAng+",dur="+dur);
        float t = 0;
        float beginAng = mAng;
        while (t < dur)
        {
            t += Time.deltaTime;
            mAng = beginAng + t / dur*(endAng - beginAng);
            yield return null;
        }
        mAng = clampTo2PI(endAng);
        mCon = null;
    }

    bool  mDrag;
    float mDragAng;
    float mLastAng;
    float mTime;
    public void OnBeginDrag(PointerEventData data)
    {
        if (mCon != null)
        {
            StopCoroutine(mCon);
            mCon = null;
        }
        data.eligibleForClick = false;
        Vector2 localPos;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, data.position, mUICamera, out localPos))return;
        if (localPos.x < -mRadius)localPos.x = -mRadius;
        if (localPos.x > mRadius)localPos.x = mRadius;
        mDrag = true;
        mLastAng = mAng;
        mDragAng = Mathf.Rad2Deg*Mathf.Asin(localPos.x / mRadius);
        mTime = Time.realtimeSinceStartup;
    }

    public void OnDrag(PointerEventData data)
    {
        if (!mDrag)return;
        Vector2 localPos;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, data.position, mUICamera, out localPos))return;
        if (localPos.x < -mRadius)localPos.x = -mRadius;
        if (localPos.x > mRadius)localPos.x = mRadius;
        float dtAng = Mathf.Rad2Deg*Mathf.Asin(localPos.x / mRadius) - mDragAng;
        mAng = mLastAng + dtAng;
    }

    public void OnEndDrag(PointerEventData data)
    {
        if (!mDrag)return;
        mDrag = false;
        Vector2 localPos;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, data.position, mUICamera, out localPos))return;
        if (localPos.x < -mRadius)localPos.x = -mRadius;
        if (localPos.x > mRadius)localPos.x = mRadius;
        float dtAng = Mathf.Rad2Deg*Mathf.Asin(localPos.x / mRadius) - mDragAng;
        mAng = mLastAng + dtAng;
        float dur = Time.realtimeSinceStartup - mTime;
        mIneriaSpeed = dtAng / dur;
        if (Mathf.Abs(mIneriaSpeed) <= mSpeed)
        {
            mIneriaSpeed = 0;
            mCon = StartCoroutine_Auto(anim(clamp(mAng)));
        }
    }
}
