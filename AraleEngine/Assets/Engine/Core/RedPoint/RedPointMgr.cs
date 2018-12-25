using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System;

namespace Arale.Engine
{

    public class RedPointMgr : MgrBase<RedPointMgr>
    {
    	Dictionary<string, int> _Reds = new Dictionary<string, int> ();
        public delegate void OnDataChanged(int mask, object val=null);
        public OnDataChanged onDataChanged;
        public virtual void Notify(int mask, object val=null)
        {
            if (onDataChanged != null)
            {
                onDataChanged(mask, val);
            }
        }
     
    	public override void Init()
    	{
    		DateTime  dt = System.DateTime.Now;
    		int  date  = (dt.Year<<16)|(dt.Month<<8)|(dt.Day);
    		int  sdate = UnityEngine.PlayerPrefs.GetInt ("RedPoint.Activity",0);
    		if (sdate < date)Set ("Activity", 1);
    		sdate = UnityEngine.PlayerPrefs.GetInt ("RedPoint.HWZB",0);
    		if (sdate < date)Set ("HWZB", 1);
    	}

        public override void Deinit()
    	{
    		onDataChanged = null;
    		_Reds.Clear ();
    	}

    	public void SetRead(string key)
    	{
    		System.DateTime dt = System.DateTime.Now;
    		int date =  (dt.Year<<16)|(dt.Month<<8)|(dt.Day);
    		UnityEngine.PlayerPrefs.SetInt ("RedPoint." + key, date); 
    		UnityEngine.PlayerPrefs.Save();
    		Set (key, 0);
    	}

    	public void Set(string key, int count)
    	{
    		_Reds [key] = count;
    		Notify (0, key);
    	}

    	public int Get(string key)
    	{
    		if (string.IsNullOrEmpty (key))return 0;
    		int count = 0;
    		if (_Reds.TryGetValue (key, out count))return count;
    		return 0;
    	}
    }

}
