using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System;

public class RedPointManager : IData
{
	Dictionary<string, int> _Reds = new Dictionary<string, int> ();

	static RedPointManager _this;
	public static RedPointManager Single
	{
		get{
			if (_this != null)return _this;
			_this = new RedPointManager ();
			return _this;
		}
	}

	public void init()
	{
		DateTime  dt = System.DateTime.Now;
		int  date  = (dt.Year<<16)|(dt.Month<<8)|(dt.Day);
		int  sdate = UnityEngine.PlayerPrefs.GetInt ("RedPoint.Activity",0);
		if (sdate < date)set ("Activity", 1);
		sdate = UnityEngine.PlayerPrefs.GetInt ("RedPoint.HWZB",0);
		if (sdate < date)set ("HWZB", 1);
	}

	public void deinit()
	{
		onDataChanged = null;
		_Reds.Clear ();
	}

	public void setRead(string key)
	{
		System.DateTime dt = System.DateTime.Now;
		int date =  (dt.Year<<16)|(dt.Month<<8)|(dt.Day);
		UnityEngine.PlayerPrefs.SetInt ("RedPoint." + key, date); 
		UnityEngine.PlayerPrefs.Save();
		set (key, 0);
	}

    public bool hasRead(string key)
    {
        if (string.IsNullOrEmpty (key))return false;
        int count = 0;
        if (_Reds.TryGetValue (key, out count))return count>0?false:true;

        DateTime  dt = System.DateTime.Now;
        int  date  = (dt.Year<<16)|(dt.Month<<8)|(dt.Day);
        int  sdate = UnityEngine.PlayerPrefs.GetInt ("RedPoint." + key, 0);
        if (sdate < date)
        {//no read
            set(key, 1);
            return false;
        }
        else
        {//has read
            return true;
        }
    }

	public void set(string key, int count)
	{
		_Reds [key] = count;
		notify (0, key);
	}

	public int get(string key)
	{
		if (string.IsNullOrEmpty (key))return 0;
		int count = 0;
		if (_Reds.TryGetValue (key, out count))return count;
		return 0;
	}
}
