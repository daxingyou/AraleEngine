using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Arale.Engine
{

    public class ViewportMgr : MgrBase<ViewportMgr>
    {
    	public enum Layout
    	{
    		L22,
    	}
    	
        public override void Init()
    	{
    		mViewports = new List<Viewport>(); 
    		mViewports.Add (new Viewport ("1"));
    		mViewports.Add (new Viewport ("2"));
    		mViewports.Add (new Viewport ("3"));
    		mViewports.Add (new Viewport ("4"));
    		resizeViewport ();
    	}

    	List<Viewport> mViewports;
    	Layout mLayout = Layout.L22;
    	Viewport mMaxViewport;

    	public class Viewport
    	{
    		public string mName;
    		public Camera mCam;
    		public Rect   mRect;
    		public Viewport(string name)
    		{
    			mName = name;
                mCam = CameraMgr.single.CreateCamera(mName, Vector3.zero, Vector3.forward);
    		}

    		public void SetMax()
    		{
    			mRect.x = 0;
    			mRect.y = 0;
    			mRect.width = 1;
    			mRect.height = 1;
    		}

    		public void SetRect(float x, float y, float w, float h)
    		{
    			mRect.x = x;
    			mRect.y = y;
    			mRect.width = w;
    			mRect.height = h;
    			if (mCam != null)mCam.rect = mRect;
    		}

    		public void SetCamera(Camera cam)
    		{
    			mCam = cam;
    			mCam.rect = mRect;
    		}
    	}

    	public Viewport CreateViewport(string name)
    	{
    		Viewport v = new Viewport (name);
    		mViewports.Add (v);
    		return v;
    	}

    	public Viewport GetViewport(int idx)
    	{
    		if (idx >= mViewports.Count)return null;
    		return mViewports[idx];
    	}

    	void resizeViewport()
    	{
    		if (mMaxViewport != null)
    		{
    			mMaxViewport.SetMax ();
    			return;
    		}

    		switch (mLayout)
    		{
    		case Layout.L22:
    			resizeL22 ();
    			break;
    		}
    	}

    	void resizeL22()
    	{
    		mViewports [0].SetRect (0.0f, 0.0f, 0.5f, 0.5f);
    		mViewports [1].SetRect (0.0f, 0.5f, 0.5f, 0.5f);
    		mViewports [2].SetRect (0.5f, 0.0f, 0.5f, 0.5f);
    		mViewports [3].SetRect (0.5f, 0.5f, 0.5f, 0.5f);
    	}
    }

}
