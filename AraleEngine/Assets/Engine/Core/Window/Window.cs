#define USE_EFFECT

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Arale.Engine
{

    public class Window : LuaMono
    {
    	public enum Event
    	{
    		Create,
    		Show,
    		Hide,
    		Destroy,
    	}

        public string winName{ set; get;}
    	public int  mThink; //窗口厚度,模型夹层使用
    	public bool mReside;//驻留,不会因调用closeAll关闭
    	public int depth{ set; get;}

    	public void Show(bool show)
    	{
    		AnimAction.ActionMask msk = show ? AnimAction.ActionMask.WindowShow : AnimAction.ActionMask.WindowHide;
    		if (mAnimAction==null || !mAnimAction.play (msk))
    		{
    			gameObject.SetActive (show);
    		}
    	}

    	public void Close(bool immediate = false)
    	{
    		if (immediate || mAnimAction==null || !mAnimAction.play (AnimAction.ActionMask.WindowClose))
    		{
    			Destroy (gameObject);
    		}

    		WindowMgr.single.CloseWindow (winName);
    	}

        internal void UpdateZOrder(ref int startDepth, ref int startZ)
    	{
    		depth = startDepth;
    		int z = -startZ;
    		startZ += mThink;
    		Vector3 v = transform.localPosition;
    		v.z = z;
    		transform.localPosition = v;
    #if USE_NGUI
            UIPanel[] panels = GetComponentsInChildren<UIPanel>();
            startDepth += panels.Length;
            System.Array.Sort<UIPanel> (panels, new DepthCompare ());
    		for(int i=0,max=panels.Length;i<max;++i)
    		{
    			panels[i].depth = startDepth + i;
    		}
    #else
        #if USE_EFFECT
                //ui中存在特效夹层，需要复杂的canvas深度管理
                Canvas[] canvass = GetComponentsInChildren<Canvas>();
                startDepth += canvass.Length;
                System.Array.Sort<Canvas>(canvass, new SortOrderCompare());
                for (int i = 0, max = canvass.Length; i < max; ++i)
                {
                    canvass[i].sortingOrder = startDepth + i;
                }
        #endif
    #endif
        }

    #if USE_NGUI
    	class DepthCompare:IComparer<UIPanel>
    	{
    		public int Compare(UIPanel l, UIPanel r)
    		{
    			return l.depth < r.depth ? -1 : 1;
    		}
    	}
    #endif

        class SortOrderCompare : IComparer<Canvas>
        {
            public int Compare(Canvas l, Canvas r)
            {
                return l.sortingOrder < r.sortingOrder ? -1 : 1;
            }
        }


    	#region mono Event
    	AnimAction mAnimAction;
    	bool bStart = false;
    	protected override void onStart ()
        {
    		bStart = true;
    		OnWindowEvent (Window.Event.Create);
    		mAnimAction = GetComponent<AnimAction>();
    		if(mAnimAction) mAnimAction.play (AnimAction.ActionMask.WindowShow);
    		OnWindowEvent (Window.Event.Show);
    	}

        protected override void onEnable()
        {
    		if(bStart)OnWindowEvent (Window.Event.Show);
    	}

        protected virtual void onDisable(){
    		OnWindowEvent (Window.Event.Hide);
    	}

    	protected override void onDestroy()
        {
    		if (mAnimAction) mAnimAction.stop ();
    		OnWindowEvent (Window.Event.Destroy);
    		Close (true);
    	}
    	#endregion


    	#region[扩展]事件处理,可以给lua处理
    	public virtual void OnWindowEvent(Window.Event eventId)
    	{
            if (mLO != null) 
    		{
                mLO.call ("OnWindowEvent", eventId);
    			return;
    		}
    	}

        public virtual void OnActionEvent(int actionId)
        {
            if (mLO != null) 
            {
                mLO.call ("OnActionEvent", actionId);
                return;
            }
        }

        public virtual void OnWindowMessage(string metho, object param)
    	{
            if (mLO != null) 
            {
                mLO.call ("OnWindowMessage", metho, param);
                return;
            }

            MethodInfo m = GetType().GetMethod(metho);
            if (m != null)
            {
                m.Invoke(this, new System.Object[]{ param });
            }
    	}
    	#endregion
    }

}
