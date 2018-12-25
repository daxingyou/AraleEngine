#if USE_NGUI
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class UILayout : MonoBehaviour {
	//美术制图大小/
	public static int baseWidth  = 1136;
	public static int baseHeight = 640;
	//铺满屏幕的大小
	public static int fullWidth = 0;
	public static int fullHeight= 0;
	//
	public static bool uiAutoFit = false;
	public OrientationType mOrientation = OrientationType.Horizontal;
	public SizeType mSizeType = SizeType.Auto;
	public ArrangeType mArrangeType = ArrangeType.ToRight;
	public SpaceType mSpaceType = SpaceType.compact;
	public AlignType mAlignType = AlignType.Center;
	
	public int mWidth;
	public int mHeight;
	public float mHPercent;
	public float mVPercent;
	public UISprite backGround;
	
	public int mGridW;  //>0,将以此值作为每个widget的宽度/
	public int mGridH;  //>0,将以此值作为每个widget的高度/
	public Vector2 min; //小于该值会被缩小/
	public Vector2 max=new Vector3(10000,10000); //大于该值会被拉伸/

	UILayout mParent;
	List<UILayout> layouts = new List<UILayout>();//子布局/
	public List<GameObject> widgets = new List<GameObject>();//子控件/
	float mScale;
	public static bool disableScale;
	bool mReposition;
	public bool repositionNow { set { if (value) { mReposition = true; enabled = true; } } }
	static FitType mFitType;
//===================================
	public enum FitType
	{
		KeepAspect,//保持纵横比，上下或左右留黑边(因特效不受缩放影响，所以该方式最简单，只用一套资源)
		ScaleFull, //拉伸铺满屏幕，为保证不会比例失真太大，做多套比例资源
		AutoFit,   //自动调整大小，layout会自动计算每个管理列表中的控件大小
	}

	public enum OrientationType
	{
		Horizontal,
		Vertical,
		Frame,
		Table,
	}

	public enum SizeType
	{
		FixSize, //固定大小/
		Percent, //固定百分比/
		Auto,    //自动大小/
	}

	public enum ArrangeType
	{
		ToRight,//从左向右排列/
		ToLeft, //从右向左排列/
		ToUp,   //从下向上排列/
		ToDown, //从上向下排列/
		HCenter,//水平中心排列/
		VCenter,//垂直中心排列/
	}

	public enum SpaceType
	{
		compact,//紧凑分布/
		average,//平均分布
	}

	public enum AlignType
	{
		Top,    //上侧对齐/
		Left,   //左侧对齐/
		Center, //中心对齐/
		Right,  //右侧对齐/
		Bottom, //下侧对齐/
	}

	#region 分辨率相关
	private static Vector2[] uiSizes = new Vector2[]{new Vector2(1024,768),new Vector3(960,640),new Vector2(1136,640)};//ui制图大小
	private static float[] aspects = new float[]{1.33f/*4/3*/,1.5f/*3/2*/,1.77f/*16/9*/};//主流纵横比,比值小到大
	private static int aspectIndex = -1;
	private static void uiFit()
	{
		//wanghuan暂时屏蔽该功能
		if(!uiAutoFit)
		{
			aspectIndex = 2;
			return;
		}

		float sc = 1f*Screen.width / Screen.height;
		for (int i=1; i<aspects.Length; ++i,++aspectIndex)
		{
			if(aspects[1]<sc)continue;
			aspectIndex = (aspects[i]-sc<sc-aspects[i-1])?i:i-1;
			break;
		}
		Debug.Log("screenw"+Screen.width+",screenh="+Screen.height+",aspectindex="+aspectIndex);
	}
	public static float aspect{
		get{return aspects[aspectIndex];}
	}
	public static Vector2 uiSize{
		get{return uiSizes[aspectIndex];}
	}
	public static string uiRes{
		get{
			switch(aspectIndex)
			{
			case 0:
				return "UI4B3/";
			case 1:
				return "UI3B2/";
			default:
				return "UI/";
			}
		}
	}
	#endregion

	//禁用layout的缩放，此时只用到layout的布局，缩放通过UIRoot完成，使用camera拉伸填充屏幕/
	//禁用缩放时,为了保证正确的布局，窗口大小取制图的大小，当前使用iphone5屏幕为制图标准/
	public static void setUILayout(FitType fitType,UIRoot uiRoot,Camera uiCamera)
	{
		mFitType = fitType;
		switch(fitType)
		{
		case FitType.AutoFit:
			UILayout.disableScale = false;
			aspectIndex = 2;
			uiRoot.minimumHeight= 1;
			uiRoot.maximumHeight= 10000;
			fullWidth=baseWidth;
			fullHeight=baseHeight;
			break;
		case FitType.KeepAspect:
			uiFit();
			UILayout.disableScale = true;
			fullWidth=baseWidth=(int)uiSize.x;
			fullHeight=baseHeight=(int)uiSize.y;
			//========================
			//该方法缩放ui根节点，但特效不受缩放影响
			//float k1 = 1.0f*baseHeight/Screen.height;
			//float k2 = 1.0f*baseWidth/Screen.width;
			//uiRoot.minimumHeight=uiRoot.maximumHeight=(int)(baseHeight*k2/k1);
			//========================
			uiRoot.minimumHeight= 1;
			uiRoot.maximumHeight= 10000;
			uiRoot.scalingStyle = UIRoot.Scaling.FixedSize;
			uiRoot.manualHeight=baseHeight;//高度决定了root的缩放比例,让根节点scale固定,否则特效和ui不一致
			//uiCamera.pixelRect=getGameCameraViewport();//裁剪viewport方式
			//=========================
			float uirootscale = 1.0f*Screen.height/baseHeight;
			float screenAspect = 1.0f*Screen.width/Screen.height;
            //WindowManager.camera_size = uiCamera.orthographicSize = screenAspect > aspect ? 1 : uirootscale * baseWidth / Screen.width;
			fullWidth =screenAspect>aspect?(int)(baseHeight*screenAspect+0.5f):baseWidth;
			fullHeight=screenAspect>aspect?baseHeight:(int)(baseWidth/screenAspect+0.5f);
			setLace("Lace");
			break;
		case FitType.ScaleFull:
			uiFit();
			UILayout.disableScale = true;
			baseWidth=(int)uiSize.x;
			baseHeight=(int)uiSize.y;
			fullWidth=baseWidth;
			fullHeight=baseHeight;
			uiRoot.minimumHeight=uiRoot.maximumHeight=UILayout.baseHeight;
			//========================
			//该方法非对称比例拉伸会导致剪切区域问题/
			//float k = (1.0f*UILayout.baseHeight*Screen.width)/(UILayout.baseWidth*Screen.height);
			//uiCamera.transform.localScale = new Vector3(k,1,1);
			//========================
			uiCamera.aspect = 1.0f*baseWidth/baseHeight;
			break;
		}
	} 

	//设置花边
	static void setLace(string laceName)
	{
		GameObject lace = GameObject.Find(laceName);
		if (null != lace)
		{
			if(Screen.height==baseHeight&&Screen.width==baseWidth)
			{
				lace.SetActive(false);
				return;
			}
			UISprite[] sp = lace.GetComponentsInChildren<UISprite>();
			if(baseHeight == fullHeight)
			{
				float w = (fullWidth - baseWidth)/2+2f;
				lace.SetActive(w>0);
				for(int i=0;i<sp.Length;++i)
				{
					if(w<8)sp[i].color=Color.black;//让花纹变成黑色
					sp[i].height = baseHeight+20;
					sp[i].width = (int)w;
				}
			}
			else
			{
				float h = (fullHeight-baseHeight)/2+2f;
				lace.SetActive(h>0);
				for(int i=0;i<sp.Length;++i)
				{
					if(h<8)sp[i].color=Color.black;
					sp[i].width  = baseWidth+20;
					sp[i].height = (int)h;
				}
			}
		}
	}

	//获取适配后场景camera的渲染视窗
	public static Rect getGameCameraViewport()
	{
		if (mFitType != FitType.KeepAspect)
		{
			return new Rect (0, 0, Screen.width, Screen.height);
		}

		float uirootscale = 1.0f*Screen.height/baseHeight;
		float w = uirootscale*baseWidth;
		if(w<Screen.width)
		{//纵横比大于1.77
			return new Rect((Screen.width - w)/2,0,w,Screen.height);
		}
		else
		{
			float h = 1.0f*Screen.height*Screen.width/w;
			return new Rect(0,(Screen.height - h)/2,Screen.width,h);
		}
	}

	//添加一个控件到layout管理/
	public void addWidget(int idx, GameObject go)
	{
		widgets.Insert (idx, go);
		mReposition = true;
	}

	public void removeWidget(int idx)
	{
		widgets.RemoveAt (idx);
		mReposition = true;
	}

	//重新布局/
	void reposition()
	{
		if (!mReposition)return;
		mParent = transform.parent.GetComponent<UILayout>();
		if (mParent == null) {
			//根节点取屏幕大小/
			if(disableScale)
			{
				mWidth = baseWidth;
				mHeight = baseHeight;
			}
			else
			{
				Vector2 sz = getViewSize();
				mWidth = (int)sz.x;
				mHeight = (int)sz.y;
			}
		}
		if (null != backGround) {
			//拉伸layout背景/
			backGround.width=mWidth;
			backGround.height=mHeight;
		}
		//添加子layout到layout管理列表，并设置重布局标志/
		layouts.Clear ();
		Transform t = transform;
		for (int i=0,max=t.childCount; i<max; ++i) 
		{
			UILayout ly = t.GetChild(i).GetComponent<UILayout>();
			if(ly==null)continue;
			ly.repositionNow = true;
			layouts.Add(ly);
		}
		//设置子layout大小位置/
		switch (mOrientation) 
		{
		case OrientationType.Horizontal:
			horizontalReposition ();break;
		case OrientationType.Vertical:
			verticalReposition ();break;
		case OrientationType.Frame:
			frameReposition();break;
		case OrientationType.Table:
			tableReposition();break;
		}
		//设置控件大小/
		resizeWidget ();
		//设置控件位置/
		repositionWidget ();
#if !UNITY_EDITOR
		mReposition = false;
#endif
	}

	void horizontalReposition()
	{
		int w = mWidth;
		int h = mHeight;
		for (int i=0; i<layouts.Count; ++i) {
			UILayout c = layouts[i];
			if(c.mSizeType == SizeType.FixSize)
			{
				w-=c.mWidth;
				c.mHeight=h;
			}
		}
		int pw = 0;
		for (int i=0; i<layouts.Count; ++i) {
			UILayout c = layouts[i];
			if(c.mSizeType == SizeType.Percent)
			{
				c.mWidth = (int)(mWidth*c.mHPercent);
				pw+=c.mWidth;
				c.mHeight= h;
			}
		}

		w -= pw;
		int autoCount = 0;
		for (int i=0; i<layouts.Count; ++i) {
			UILayout c = layouts[i];
			if(c.mSizeType == SizeType.Auto)
			{
				++autoCount;
			}
		}
		for (int i=0; i<layouts.Count; ++i) {
			UILayout c = layouts[i];
			if(c.mSizeType == SizeType.Auto)
			{
				c.mWidth = (int)(1.0f*w/autoCount);
				c.mHeight = h;
			}
		}

		int l = -mWidth/2;
		for (int i=0; i<layouts.Count; ++i) {
			Vector3 p = layouts[i].transform.localPosition;
			p.y = 0;
			p.x = l+layouts[i].mWidth/2;
			l+=layouts[i].mWidth;
			layouts[i].transform.localPosition=p;
		}
	}

	void verticalReposition()
	{
		int w = mWidth;
		int h = mHeight;
		for (int i=0; i<layouts.Count; ++i) {
			UILayout c = layouts[i];
			if(c.mSizeType == SizeType.FixSize)
			{
				h-=c.mHeight;
				c.mWidth=w;
			}
		}
		int ph = 0;
		for (int i=0; i<layouts.Count; ++i) {
			UILayout c = layouts[i];
			if(c.mSizeType == SizeType.Percent)
			{
				c.mHeight = (int)(mHeight*c.mVPercent);
				ph+=c.mHeight;
				c.mWidth= w;
			}
		}
		h -= ph;
		int autoCount = 0;
		for (int i=0; i<layouts.Count; ++i) {
			UILayout c = layouts[i];
			if(c.mSizeType == SizeType.Auto)
			{
				++autoCount;
			}
		}
		for (int i=0; i<layouts.Count; ++i) {
			UILayout c = layouts[i];
			if(c.mSizeType == SizeType.Auto)
			{
				c.mHeight = (int)(1.0f*h/autoCount);
				c.mWidth = w;
			}
		}

		int t = mHeight/2;
		for (int i=0; i<layouts.Count; ++i) {
			Vector3 p = layouts[i].transform.localPosition;
			p.x = 0;
			p.y = t-layouts[i].mHeight/2;
			t-=layouts[i].mHeight;
			layouts[i].transform.localPosition=p;
		}
	}

	void frameReposition()
	{
		for (int i=0; i<layouts.Count; ++i) {
			UILayout c = layouts[i];
			c.mHeight = mHeight;
			c.mWidth  = mWidth;
			Vector3 p = c.transform.localPosition;
			p.x = 0;
			p.y = 0;
			p.z = 0;
			c.transform.localPosition=p;
		}
	}

	void tableReposition()
	{
		int l = -mWidth / 2;
		int t = mHeight / 2;
		int maxH = 0;
		for (int i=0; i<layouts.Count; ++i) {
			UILayout c = layouts[i];
			if(c.mSizeType==SizeType.Auto)continue;
			else if(c.mSizeType==SizeType.Percent)
			{
				c.mWidth = (int)(c.mHPercent*mWidth);
				c.mHeight= (int)(c.mVPercent*mHeight);
			}
			Vector3 p = c.transform.localPosition;
			p.x = l+c.mWidth/2;
			p.y = t-c.mHeight/2;
			c.transform.localPosition=p;
			if(maxH<c.mHeight)maxH=c.mHeight;
			l+=c.mWidth;
			if(2*l>mWidth-0.1f*c.mWidth)
			{
				l=-mWidth/2;
				t-=maxH;
				maxH = 0;
			}
		}
	}

	void resizeWidget()
	{
		if(disableScale||min.x==0||min.y==0)
		{
			mScale=1;
			return;
		}

		mScale = Mathf.Min(mWidth/min.x,mHeight/min.y);
		mScale = mScale >= 1 && mScale < 1.2f ? 1 : mScale;
		for (int i=0; i<widgets.Count; ++i) 
		{
			Vector3 s = widgets [i].transform.localScale;
			s.x = s.y = s.z = mScale;//如果x,y,z不相等会造成剪切区域不正确/
			widgets [i].transform.localScale = s;
		}
	}
	
	void repositionWidget()
	{
		switch (mArrangeType) {
		case ArrangeType.ToRight:
			repositionWidgetToRight();break;
		case ArrangeType.ToLeft:
			repositionWidgetToLeft();break;
		case ArrangeType.ToDown:
			repositionWidgetToDown();break;
		case ArrangeType.ToUp:
			repositionWidgetToUp();break;
		case ArrangeType.HCenter:
			repositionWidgetHCenter();break;
		case ArrangeType.VCenter:
			repositionWidgetVCenter();break;
		}
	}

	void repositionWidgetToRight()
	{
		int t = 0;
		switch (mAlignType) 
		{
		case AlignType.Top:
			t = mHeight/2;break;
		case AlignType.Bottom:
			t = -mHeight/2;break;
		}
		int l = -mWidth/2;
		if (SpaceType.compact == mSpaceType)
		{
			if(mGridW>0)
			{
				for (int i=0; i<widgets.Count; ++i) 
				{
					Vector3 p = widgets [i].transform.localPosition;
					p.y = t;
					p.x = l + mScale*mGridW / 2;
					l += (int)mScale*mGridW;
					widgets [i].transform.localPosition = p;
				} 
			}
			else
			{
				for (int i=0; i<widgets.Count; ++i) 
				{
					Bounds bd = NGUIMath.CalculateRelativeWidgetBounds (widgets [i].transform);
					//对于list，因该获取剪切区域大小，而不是控件大小，因此可以使用mGrid来制定大小，或者用一个背景来决定list的大小/
					Vector3 p = widgets [i].transform.localPosition;
					p.y = t;
					p.x = l + mScale*bd.size.x / 2;
					l += (int)(mScale*bd.size.x);
					widgets [i].transform.localPosition = p;
				} 
			}
		}
		else
		{
			int w = mWidth/widgets.Count;
			for (int i=0; i<widgets.Count; ++i) 
			{
				Vector3 p = widgets [i].transform.localPosition;
				p.y = t;
				p.x = l+w/2;
				l += w;
				widgets [i].transform.localPosition = p;
			} 
		}
	}

	void repositionWidgetToLeft()
	{
		int t = 0;
		switch (mAlignType) 
		{
		case AlignType.Top:
			t = mHeight/2;break;
		case AlignType.Bottom:
			t = -mHeight/2;break;
		}
		int r = mWidth/2;
		if (SpaceType.compact == mSpaceType)
		{
			if(mGridW>0)
			{
				for(int i=0;i<widgets.Count;++i)
				{
					Vector3 p = widgets[i].transform.localPosition;
					p.y = t;
					p.x = r-mScale*mGridW/2;
					r-=(int)(mScale*mGridW);
					widgets[i].transform.localPosition=p;
				}
			}
			else
			{
				for(int i=0;i<widgets.Count;++i)
				{
					Bounds bd = NGUIMath.CalculateRelativeWidgetBounds(widgets[i].transform);
					Vector3 p = widgets[i].transform.localPosition;
					p.y = t;
					p.x = r-mScale*bd.size.x/2;
					r-=(int)(mScale*bd.size.x);
					widgets[i].transform.localPosition=p;
				}
			}
		}
		else
		{
			int w = mWidth/widgets.Count;
			for (int i=0; i<widgets.Count; ++i) 
			{
				Vector3 p = widgets [i].transform.localPosition;
				p.y = t;
				p.x = r-w/2;
				r -= w;
				widgets [i].transform.localPosition = p;
			}
		}
	}

	void repositionWidgetToDown()
	{
		int l = 0;
		switch (mAlignType) 
		{
		case AlignType.Left:
			l = -mWidth/2;break;
		case AlignType.Right:
			l = mWidth/2;break;
		}
		int t = mHeight/2;
		if (SpaceType.compact == mSpaceType) 
		{
			if(mGridH>0)
			{
				for (int i=0; i<widgets.Count; ++i) 
				{
					Vector3 p = widgets [i].transform.localPosition;
					p.x = l;
					p.y = t - mGridH / 2;
					t -= (int)mGridH;
					widgets [i].transform.localPosition = p;
				}
			}
			else
			{
				for (int i=0; i<widgets.Count; ++i) 
				{
					Bounds bd = NGUIMath.CalculateRelativeWidgetBounds (widgets [i].transform);
					Vector3 p = widgets [i].transform.localPosition;
					p.x = l;
					p.y = t - bd.size.y / 2;
					t -= (int)bd.size.y;
					widgets [i].transform.localPosition = p;
				}
			}
		}
		else
		{
			int h = mHeight/widgets.Count;
			for (int i=0; i<widgets.Count; ++i) 
			{
				Vector3 p = widgets [i].transform.localPosition;
				p.x = l;
				p.y = t - h / 2;
				t -= h;
				widgets [i].transform.localPosition = p;
			}
		}
	}

	void repositionWidgetToUp()
	{
		int l = 0;
		switch (mAlignType) 
		{
		case AlignType.Left:
			l = -mWidth/2;break;
		case AlignType.Right:
			l = mWidth/2;break;
		}
		int b = -mHeight/2;
		if (SpaceType.compact == mSpaceType)
		{
			if(mGridH>0)
			{
				for(int i=0;i<widgets.Count;++i)
				{
					Vector3 p = widgets[i].transform.localPosition;
					p.x = l;
					p.y = b+mScale*mGridH/2;
					b+=(int)(mScale*mGridH);
					widgets[i].transform.localPosition=p;
				}
			}
			else
			{
				for(int i=0;i<widgets.Count;++i)
				{
					Bounds bd = NGUIMath.CalculateRelativeWidgetBounds(widgets[i].transform);
					Vector3 p = widgets[i].transform.localPosition;
					p.x = l;
					p.y = b+mScale*bd.size.y/2;
					b+=(int)(mScale*bd.size.y);
					widgets[i].transform.localPosition=p;
				}
			}
		}
		else
		{
			int h = mHeight/widgets.Count;
			for(int i=0;i<widgets.Count;++i)
			{
				Vector3 p = widgets[i].transform.localPosition;
				p.x = l;
				p.y = b+h/2;
				b+=h;
				widgets[i].transform.localPosition=p;
			}
		}
	}

	void repositionWidgetHCenter()
	{
		int t = 0;
		switch (mAlignType) 
		{
		case AlignType.Top:
			t = mHeight/2;break;
		case AlignType.Bottom:
			t = -mHeight/2;break;
		}
		float tw = 0;
		if (mGridW > 0)
		{
			tw = mScale*mGridW*widgets.Count;
			int l = (int)-tw/2;
			for (int i=0; i<widgets.Count; ++i) 
			{
				Vector3 p = widgets [i].transform.localPosition;
				p.y = t;
				p.x = l + mScale*mGridW / 2;
				l += (int)(mScale*mGridW);
				widgets [i].transform.localPosition = p;
			}
		}
		else
		{
			for (int i=0; i<widgets.Count; ++i) 
			{
				Bounds bd = NGUIMath.CalculateRelativeWidgetBounds (widgets [i].transform);
				tw += mScale*bd.size.x;
			}
			int l = (int)-tw/2;
			for (int i=0; i<widgets.Count; ++i) 
			{
				Bounds bd = NGUIMath.CalculateRelativeWidgetBounds (widgets [i].transform);
				Vector3 p = widgets [i].transform.localPosition;
				p.y = t;
				p.x = l + mScale*bd.size.x / 2;
				l += (int)(mScale*bd.size.x);
				widgets [i].transform.localPosition = p;
			}
		}
	}

	void repositionWidgetVCenter()
	{
		int l = 0;
		switch (mAlignType) 
		{
		case AlignType.Left:
			l = -mWidth/2;break;
		case AlignType.Right:
			l = mWidth/2;break;
		}
		float th = 0;
		if (mGridH > 0) 
		{
			th = mScale*mGridH*widgets.Count;
			int t = (int)th/2;
			for (int i=0; i<widgets.Count; ++i) 
			{
				Vector3 p = widgets [i].transform.localPosition;
				p.x = l;
				p.y = t - mScale*mGridH / 2;
				t -= (int)(mScale*mGridH);
				widgets [i].transform.localPosition = p;
			}
		} 
		else 
		{
			for (int i=0; i<widgets.Count; ++i) 
			{
				Bounds bd = NGUIMath.CalculateRelativeWidgetBounds (widgets [i].transform);
				th += mScale*bd.size.y;
			}
			int t = (int)th/2;
			for (int i=0; i<widgets.Count; ++i) 
			{
				Bounds bd = NGUIMath.CalculateRelativeWidgetBounds (widgets [i].transform);
				Vector3 p = widgets [i].transform.localPosition;
				p.x = l;
				p.y = t - mScale*bd.size.y / 2;
				t -= (int)(mScale*bd.size.y);
				widgets [i].transform.localPosition = p;
			}
		}
	}
//===================================
	public Camera uiCamera = null;
	public bool runOnce = true;
	public enum LayoutType
	{
		None,
		FullBaseHeight,
		FullBaseWidth,
	}
	public LayoutType layoutType = LayoutType.None;
	UIRoot mRoot;
	Rect mRect;
//==================================
	// Use this for initialization
	void Start () {
		mReposition = true;
		if (backGround == null) {
			Transform t = transform.FindChild ("backGround");
			if(t!=null)
			{
				backGround = t.GetComponent<UISprite>();
			}
		}
		if (uiCamera == null) uiCamera = NGUITools.FindCameraForLayer(gameObject.layer);
		mRoot = NGUITools.FindInParents<UIRoot>(gameObject);
	}

	// Update is called once per frame
	void Update () {
		reposition ();
		if (layoutType == LayoutType.None)
			return;
		if(layoutType==LayoutType.FullBaseWidth)FullBaseWidth ();
		if(layoutType == LayoutType.FullBaseHeight)FullBaseHeight ();
		if (runOnce)enabled = false;
	}

#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		GameObject go = UnityEditor.Selection.activeGameObject;
		bool selected = (go != null) && (NGUITools.FindInParents<UILayout>(go) == this);

		//Vector2 viewSize = UIPanel.GetMainGameViewSize ();
		Transform t = transform;
		Matrix4x4 old = Gizmos.matrix;
		Gizmos.matrix = t.localToWorldMatrix;
		if (selected) {
			Gizmos.color = new Color (0, 0, 1);
			Gizmos.DrawWireCube (Vector3.zero, new Vector3 (mWidth+2, mHeight+2, 0));
		} else {
			Gizmos.DrawWireCube (Vector3.zero, new Vector3 (mWidth, mHeight, 0));
		}
		Gizmos.matrix = old;
	}
#endif

	Vector2 getViewSize()
	{
		mRect = uiCamera.pixelRect;
		float adjustment = 1f;
		if (mRoot != null) adjustment = mRoot.pixelSizeAdjustment;
		Vector2 size = new Vector2 (mRect.width, mRect.height);
		if (adjustment != 1f && size.y > 1f)
		{
			float scale = mRoot.activeHeight / size.y;
			size.x *= scale;
			size.y *= scale;															
		}
		return size;
	}

	void FullBaseHeight()
	{
		mRect = uiCamera.pixelRect;
		float adjustment = 1f;
		if (mRoot != null) adjustment = mRoot.pixelSizeAdjustment;
		float rectWidth = mRect.width;
		float rectHeight = mRect.height;
		if (adjustment != 1f && rectHeight > 1f)
		{
			float scale = mRoot.activeHeight / rectHeight;
			rectWidth *= scale;
			rectHeight *= scale;
		}

		float yScale = rectHeight / baseHeight;
		Vector3 s = transform.localScale;
		s.y = yScale;
		s.x = yScale;
		transform.localScale=s;
	}

	void FullBaseWidth()
	{
		mRect = uiCamera.pixelRect;
		float adjustment = 1f;
		if (mRoot != null) adjustment = mRoot.pixelSizeAdjustment;
		float rectWidth = mRect.width;
		float rectHeight = mRect.height;
		if (adjustment != 1f && rectHeight > 1f)
		{
			float scale = mRoot.activeHeight / rectHeight;
			rectWidth *= scale;
			rectHeight *= scale;
		}

		float xScale = rectWidth / baseWidth;
		Vector3 s = transform.localScale;
		s.x = xScale;
		s.y = xScale;
		transform.localScale=s;
	}
}
#endif