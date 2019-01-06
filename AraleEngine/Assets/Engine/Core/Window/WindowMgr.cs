using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Arale.Engine
{
        
    public class WindowMgr : MgrBase<WindowMgr>
    {
    	static Dictionary<string,string> mWinResMap = new  Dictionary<string,string>();
    	public static void SetWindowRes(string winName, string prefabPath)
    	{
    		mWinResMap [winName] = prefabPath;
    	}

        public override void Init()
        {
            mWinRoot = GRoot.single.transform.FindChild("WinRoot");
            EventMgr.single.AddListener ("Window", OnWindowMessage);
        }

        public override void Deinit()
        {
			EventMgr.single.RemoveListener ("Window", OnWindowMessage);
        }


    	Transform mWinRoot;
        public Transform winRoot{get{return mWinRoot;}}
    	Dictionary<string,Window> mWindows = new  Dictionary<string,Window>();
    	List<Window> mZOrder = new List<Window>();
    	public Window GetWindow(string name, bool create = false, string id=null)
    	{
    		Window win = null;
    		string key = id == null ? name : name + id;
    		if(mWindows.TryGetValue(key, out win))
    		{
    			return win;
    		}
    		if(create)
    		{
    			GameObject go = ResLoad.get (mWinResMap [name]).gameObject ();
    			if (go == null)return null;
    			go.transform.SetParent (mWinRoot, false);
    			/*//======================
    			RectTransform rt = go.transform as RectTransform;
    			Vector2 offsetMin = rt.offsetMin;
    			Vector2 offsetMax = rt.offsetMax;
    			rt.SetParent(WindowMgr.single.mWinRoot);
    			rt.localPosition = Vector3.zero;
    			rt.localScale = Vector3.one;
    			//必须设置，否则setParent后,prefab原来的size被改变了
    			rt.offsetMin = offsetMin;
    			rt.offsetMax = offsetMax;
    			//=======================*/
    			win = go.GetComponent<Window>();
    			win.winName = key;
    			mWindows[key] = win;
    			mZOrder.Add(win);
    			WindowMgr.single.UpdateZOrder ();
    			return win;
    		}
    		return null;
    	}

    	public void BringToTop(Window win)
    	{
    		if(mZOrder.Remove (win))
    		{
    			mZOrder.Add(win);
    			UpdateZOrder();
    		}
    	}

    	public void CloseWindow(string name, bool immediate = false)
    	{
    		if(!mWindows.ContainsKey(name))return;
    		Window win = mWindows [name];
    		mWindows.Remove (name);
    		mZOrder.Remove (win);
    		win.Close (immediate);
    	}

    	public void CloseAllWindow()
    	{
    		ArrayList ls = new ArrayList ();
    		foreach (Window item in mWindows.Values) 
    		{
    			if (item == null)continue;//closewindow时不会清除key
    			if(item.mReside)continue;
    			ls.Add (item.winName);
    		}
    		for (int i = 0; i < ls.Count; ++i)
    		{
    			CloseWindow (ls [i] as string);
    		}
    	}

    	public void UpdateZOrder()
    	{
    		int depth = 0;
    		int z = 0;
    		for(int i=0,max=mZOrder.Count;i<max;++i)
    		{
    			mZOrder[i].UpdateZOrder(ref depth, ref z);
    		}
    	}

    	public void ShowUI(bool bshow)
    	{
    		mWinRoot.gameObject.SetActive (bshow);
    	}


    	#region 窗口消息机制
    	class WinMsg
    	{
    		public string winName;
            public string metho;
    		public object param;
    	}

        public static void PostWindowMessage(string winName, string metho, object param)
    	{
    		WinMsg wm = new WinMsg ();
    		wm.winName = winName;
            wm.metho = metho;
            wm.param = param;
            EventMgr.single.PostEvent("Window", wm);
    	}

        public static void SendWindowMessage(string winName, string metho, object param)
        {
            WinMsg wm = new WinMsg ();
            wm.winName = winName;
            wm.metho = metho;
            wm.param = param;
            EventMgr.single.SendEvent("Window", wm);
        }

        void OnWindowMessage(EventMgr.EventData eb)
        {
    		WinMsg wm = eb.data as WinMsg;
    		if (wm.winName == null)
    		{//广播
                foreach (Window item in mWindows.Values) item.OnWindowMessage (wm.metho,wm.param);
    		}
    		else
    		{
                
    			Window w = GetWindow (wm.winName, false);
    			if (w == null)return;
                w.OnWindowMessage (wm.metho,wm.param);
    		}
    	}
    	#endregion
    }

}