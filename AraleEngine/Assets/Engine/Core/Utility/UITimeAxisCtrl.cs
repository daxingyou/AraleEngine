using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Arale.Engine
{

    [RequireComponent(typeof(RawImage))]
    public class UITimeAxisCtrl : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IScrollHandler, IPointerClickHandler{
    	const  int  BandWidth = 8;
    	public Text mTimeLeftInfo;
    	public Text mTimeRightInfo;

    	float mLeftTime   = 0; //左边界时间
    	float mRightTime  = 0; //右边界时间
    	float mMaxTime    = 0; //当前时间轴最大时间长度
    	float mCMTimeUnit = 10;//1cm标尺对应10s
    	int   mCM         = 40;//1cm标尺有40像素
    	int   mYOffset    = 0;
    	int   mMaxY       = 0;

    	RawImage      mAxisRawImage;
    	RectTransform mRectTran;
    	Texture2D     mAxisTexture;
    	int           mImageW;
    	int           mImageH;

        TimeMgr.TimeAxis mParentTimeAxis = TimeMgr.single.realTimeAxis;//当前展开的
        TimeMgr.TimeAxis mSelTimeAxis;//当前选中的
    	// Use this for initialization
    	void Start ()
    	{
    		mAxisRawImage = GetComponent<RawImage> ();
    		mAxisRawImage.color = Color.white;
    		mRectTran = mAxisRawImage.rectTransform;
    		mImageW = (int)mRectTran.rect.width;
    		mImageH = (int)mRectTran.rect.height;
    		mAxisTexture = new Texture2D (mImageW, mImageH, TextureFormat.ARGB32, false);
    		mAxisRawImage.texture = mAxisTexture;
    		mAxisTexture.filterMode = FilterMode.Point;
    		/*
    		TimeManager.TimeAxis t1 = TimeManager.single.createTimeAxis (100, null, "0");
    		t1.addTimeEvent (new TimeManager.EventAction(10, null));
    		t1.addTimeEvent (new TimeManager.EventAction(20, null));
    		t1.addTimeEvent (new TimeManager.EventAction(90, null));
    		t1 = TimeManager.single.createTimeAxis (200, t1, "21");
    		t1 = TimeManager.single.createTimeAxis (200, null, "1");
    		t1.addTimeEvent (new TimeManager.EventAction(50, null));
    		t1 = TimeManager.single.createTimeAxis (300, null, "2");
    		t1.addTimeEvent (new TimeManager.EventAction(100, null));
    		t1 = TimeManager.single.createTimeAxis (400, null, "3");
    		t1.addTimeEvent (new TimeManager.EventAction(100, null));
    		t1 = TimeManager.single.createTimeAxis (500, null, "4");
    		t1.addTimeEvent (new TimeManager.EventAction(100, null));
    		t1 = TimeManager.single.createTimeAxis (600, null, "5");
    		t1.addTimeEvent (new TimeManager.EventAction(100, null));
    		t1 = TimeManager.single.createTimeAxis (700, null, "6");
    		t1.addTimeEvent (new TimeManager.EventAction(100, null));
    		t1 = TimeManager.single.createTimeAxis (800, null, "7");
    		t1.addTimeEvent (new TimeManager.EventAction(100, null));
    		t1 = TimeManager.single.createTimeAxis (900, null, "8");
    		t1.addTimeEvent (new TimeManager.EventAction(100, null));
    		t1 = TimeManager.single.createTimeAxis (1000, null, "9");
    		t1.addTimeEvent (new TimeManager.EventAction(100, null));
    		t1 = TimeManager.single.createTimeAxis (1100, null, "10");
    		t1.addTimeEvent (new TimeManager.EventAction(100, null));
    		t1 = TimeManager.single.createTimeAxis (1200, null, "11");
    		t1.addTimeEvent (new TimeManager.EventAction(100, null));
    		t1 = TimeManager.single.createTimeAxis (1300, null, "12");
    		t1.addTimeEvent (new TimeManager.EventAction(100, null));
    		t1 = TimeManager.single.createTimeAxis (1400, null, "13");
    		t1.addTimeEvent (new TimeManager.EventAction(100, null));
    		*/

    		updateView ();
    		InvokeRepeating ("UpdateSize", 1f, 0.03f);
    	}

    	void UpdateSize()
    	{
    		int w = (int)mRectTran.rect.width;
    		int h = (int)mRectTran.rect.height;
    		if (w != mImageW || h != mImageH)
    		{
    			mAxisTexture.Resize (w, h);
    			mImageW = w;
    			mImageH = h;
    			Debug.LogError ("w="+mImageW+",h="+mImageH);
    			updateView ();
    		}
    	}

    	public void setCMTUnit(float cmt)
    	{
    		mCMTimeUnit= cmt;
    		mRightTime = mLeftTime + dxToTime (mImageW);
    		correctTime ();
    		updateView ();
    	}

    	void updateView()
    	{
    		
    		drawRuler();
    		drawTimeAxis ();
    		mAxisTexture.Apply();//应用到GPU

    		int s = (int)mLeftTime;
    		int ms = (int)(100*(mLeftTime - s));
    		int h = s / 3600;
    		int m = s / 60 - h * 60;
    		s = s % 60;
    		mTimeLeftInfo.text = string.Format("开始{0:D2}:{1:D2}:{2:D2} {3:D3} 单位{4:F3}s", h,m,s,ms,mCMTimeUnit);
    		s = (int)mRightTime;
    		ms = (int)(100*(mRightTime - s));
    		h = s / 3600;
    		m = s / 60 - h * 60;
    		s = s % 60;
    		mTimeRightInfo.text = string.Format("结束{0:D2}:{1:D2}:{2:D2} {3:D3}", h,m,s,ms);
    	}

    	void drawRuler()
    	{
    		drawTimeBand (0, mAxisTexture.width, 0, Color.black, mAxisTexture.height);
    		int k = (int)(mLeftTime / mCMTimeUnit);
    		float offsetX = (mLeftTime-k) * mCM;
    		for (int x = 0; x < mAxisTexture.width; ++x)
    		{
    			if((offsetX+x)%mCM!=0)continue;
    			for (int y = 0; y < mAxisTexture.height; ++y)
    			{
    				mAxisTexture.SetPixel (x, y, Color.gray);
    			}
    		}
    	}

    	void drawTimeBand(int x1, int x2, int y1, Color clr, int width=1)
    	{
    		int y2 = y1 + width;
    		if (x1 < 0)x1 = 0;
    		if (x2 >= mAxisTexture.width)x2 = mAxisTexture.width - 1;
    		if (y1 < 0)y1 = 0;
    		if (y2 >= mAxisTexture.height)y2 = mAxisTexture.height - 1;

    		for (int x = x1; x < x2; ++x)
    		{
    			for (int y = y1; y < y2; ++y)
    			{
    				mAxisTexture.SetPixel (x, y, clr);
    			}
    		}
    	}

    	void drawTimeEvent(int x, int y, Color clr, int width)
    	{
    		int x1 = x - 2;
    		int x2 = x + 2;
    		int y1 = y - (int)(0.5f*width);
    		int y2 = y + (int)(0.5f*width);
    		if (x1 < 0)x1 = 0;
    		if (x2 >= mAxisTexture.width)x2 = mAxisTexture.width - 1;
    		if (y1 < 0)y1 = 0;
    		if (y2 >= mAxisTexture.height)y2 = mAxisTexture.height - 1;
    		for (x = x1; x < x2; ++x)
    		{
    			for (y = y1; y < y2; ++y)
    			{
    				mAxisTexture.SetPixel (x, y, clr);
    			}
    		}
    	}



    	public Color[] bandColor = new Color[]{Color.red, Color.yellow, Color.green, Color.cyan, Color.magenta};
    	void drawTimeAxis()
    	{
    		mMaxTime = 0;
            List<TimeMgr.TimeAxis> tas = mParentTimeAxis.childs;
    		for (int i = 0, max = tas.Count; i < max; ++i)
    		{
                TimeMgr.TimeAxis ta = tas [i];

    			float startTime = ta.startTime;
    			float endTime = ta.endTime + mCMTimeUnit;
    			if (mMaxTime < endTime)
    			{
    				mMaxTime = endTime;
    			}

    			int x1 = timeToX (startTime);
    			int x2 = timeToX (endTime);
    			if (x2 < 0 || x1 >=mAxisTexture.width)continue;
    			bool isSel = object.ReferenceEquals (ta, mSelTimeAxis);
    			drawTimeBand (x1, x2, i * BandWidth-mYOffset, isSel?Color.white:bandColor[i%5], BandWidth);

                List<TimeMgr.Action> tes = ta.actions;
    			for (int j = 0, jmax = tes.Count; j < jmax; ++j)
    			{
    				int x = timeToX (startTime+tes [j].doTime);
    				if (x < 0 || x >=mAxisTexture.width)continue;
    				drawTimeEvent (x, (int)((1.0f*i+0.5f)*BandWidth)-mYOffset, Color.black, BandWidth);
    			}
    		}
    		mMaxY = tas.Count * BandWidth;
    	}

    	#region 时间映射
    	int timeToX(float t)//t=1s
    	{
    		return (int)((t - mLeftTime)/mCMTimeUnit*mCM);
    	}

    	float dxToTime(int dx)
    	{
    		return 1.0f * dx / mCM * mCMTimeUnit;
    	}

    	void correctTime()
    	{
    		if (mRightTime>mMaxTime)
    		{
    			mRightTime = mMaxTime;
    			mLeftTime  = mRightTime - dxToTime (mImageW);
    		}
    		if (mLeftTime < 0)
    		{
    			mLeftTime  = 0;
    			mRightTime = mLeftTime + dxToTime (mImageW);
    		}
    	}

    	float getPointTime(Vector2 v)
    	{
    		int dx = (int)v.x;
    		return mLeftTime + dxToTime (dx);
    	}

        TimeMgr.TimeAxis getPointTimeAxis(Vector2 v)
    	{
    		float t = getPointTime (v);
    		int y = (int)v.y+mYOffset;
    		int i = y / BandWidth;
            List<TimeMgr.TimeAxis> tas = mParentTimeAxis.childs;
    		if (i >= tas.Count)return null;
            TimeMgr.TimeAxis ta = mParentTimeAxis.childs [i];
    		if (ta.startTime < t && ta.endTime + mCMTimeUnit > t)return ta;
    		return null;
    	}
    	#endregion


    	#region 时间轴滑动
    	float oldLeftTime;
    	float oldRightTime;
    	public void OnBeginDrag (PointerEventData eventData)
    	{
    		oldLeftTime  = mLeftTime;
    		oldRightTime = mRightTime;
    	}

    	public void OnEndDrag (PointerEventData eventData)
    	{
    	}

    	public void OnDrag (PointerEventData eventData)
    	{
    		Vector2 offset = eventData.position - eventData.pressPosition;
    		float ot = dxToTime ((int)offset.x);
    		mLeftTime  =  oldLeftTime  - ot;
    		mRightTime =  oldRightTime - ot;
    		correctTime ();
    		updateView ();
    	}

    	public void OnScroll (PointerEventData eventData)
    	{
    		mYOffset += (int)(10*eventData.scrollDelta.y);
    		if (mYOffset < 0)mYOffset = 0;
    		if (mYOffset+mImageH >= mMaxY)mYOffset = mMaxY-mImageH;
    		updateView ();
    	}
    	#endregion

    	public void OnPointerClick (PointerEventData eventData)
    	{
    		Vector2 localPos = Vector2.zero;
    		if (!RectTransformUtility.ScreenPointToLocalPointInRectangle (mRectTran, eventData.position, eventData.pressEventCamera, out localPos))
    			return;
    		localPos.x =  0.5f*mImageW+localPos.x;
    		localPos.y =  0.5f*mImageH+localPos.y;
    		Debug.LogError ("==="+eventData.clickCount);
    		if (eventData.clickCount == 2)
    		{//双击
    			if (eventData.pointerId == -1)
    			{//left button
                    TimeMgr.TimeAxis ta = getPointTimeAxis (localPos);
    				if (ta == null)return;
    				mParentTimeAxis = ta;
    				mSelTimeAxis = null;
    				updateView ();
    			}
    			else if(eventData.pointerId == -2)
    			{//right  button
    				if(mParentTimeAxis.parent==null)return;
    				mParentTimeAxis = mParentTimeAxis.parent;
    				mSelTimeAxis = null;
    				updateView ();
    			}
    		}
    		else if(eventData.clickCount == 1)
    		{
                TimeMgr.TimeAxis ta = getPointTimeAxis(localPos);
    			if (ta==null || object.ReferenceEquals (mSelTimeAxis, ta))return;
    			mSelTimeAxis = ta;
    			updateView ();
    		}
    	}
    }

}
