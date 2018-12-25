using UnityEngine;
using System.Collections;
using Arale.Engine;

public class TestTime : MonoBehaviour {
	// Update is called once per frame
	void onDateTimeChange(int mask, object val)
	{
		switch (mask)
		{
		case 0:
			Debug.Log ("整秒:" + (int)val);
			break;
		case 1:
			Debug.Log ("整分:" + (int)val);
			break;
		case 2:
			Debug.Log ("整时:" + (int)val);
			break;
		case 3:
			Debug.Log ("整日:" + (int)val);
			break;
		}
	}

    public class LogAction : TimeMgr.Action
    {
        string log;
        public LogAction(float t, string logInfo)
        {
            doTime = t;
            log=logInfo;
            onAction = delegate(TimeMgr.Action ac) {
                Debug.LogError(log);
            };
        }
    }

	void Update () {
		TimeMgr.single.Update ();
	}

	void OnGUI()
	{
		int ox = 0;
		int oy = 30;
		if (GUI.Button(new Rect(ox, oy, 100, 30), "事件序列"))
		{
			TimeMgr.TimeAxis ta1 = TimeMgr.single.GetTimeAxis ("1");
			if (ta1 != null)
			{
				ta1.Remove ();
				return;
			}

			ta1 = TimeMgr.single.GetTimeAxis ("1", true);
			ta1.AddAction (new LogAction (0, "event1"));
			ta1.AddAction (new LogAction (1, "event2"));
			ta1.AddAction (new LogAction (2, "event3"));
			ta1.AddAction (new LogAction (3, "event4"));
			ta1.AddAction (new LogAction (4 , "event5"));
			ta1.SetParent (TimeMgr.single.realTimeAxis, TimeMgr.single.realTimeAxis.time+1);
		}

		if (GUI.Button(new Rect(ox, oy+=30, 100, 30), "事件循环"))
		{
			TimeMgr.TimeAxis ta1 = TimeMgr.single.GetTimeAxis ("2");
			if (ta1 != null)
			{
				ta1.Remove ();
				return;
			}

            ta1 = TimeMgr.single.GetTimeAxis ("2", true);
            TimeMgr.Action a = ta1.AddAction (new LogAction (1, "event1"));
            a.onAction += delegate(TimeMgr.Action self)
                {
                    self.Loop(2);
                };
            ta1.SetParent (TimeMgr.single.realTimeAxis, TimeMgr.single.realTimeAxis.time+1);
		}

		if (GUI.Button(new Rect(ox, oy+=30, 100, 30), "时间轴循环"))
		{
            TimeMgr.TimeAxis ta1 = TimeMgr.single.GetTimeAxis ("3");
			if (ta1 != null)
			{
				ta1.Remove ();
				return;
			}

            ta1 = TimeMgr.single.GetTimeAxis ("3", true);
			ta1.AddAction (new LogAction (0, "event1"));
            TimeMgr.Action a = ta1.AddAction (new LogAction (1, "event2"));
            a.onAction += delegate(TimeMgr.Action self)
                {
                        ta1.Loop();
                };
            ta1.SetParent (TimeMgr.single.realTimeAxis, TimeMgr.single.realTimeAxis.time+1);
		}

		if (GUI.Button(new Rect(ox, oy+=30, 100, 30), "定时播报"))
		{
            RTime.R.syncServerTime (System.DateTime.UtcNow.Ticks, System.DateTime.Now.Ticks, false);
            TimeMgr.TimeAxis ta1 = TimeMgr.single.GetTimeAxis ("4");
			if (ta1 != null)
			{
				ta1.Remove ();
				return;
			}

            ta1 = TimeMgr.single.GetTimeAxis ("4", true);
            TimeMgr.DateAction a = ta1.AddAction(new TimeMgr.DateAction()) as TimeMgr.DateAction;
            a.m = 0;
            a.s = 0;
            a.onAction = delegate(TimeMgr.Action ac) {
                Debug.Log (RTime.R.time.ToString("yyyy.MM.dd hh:mm:ss")+":是你想要的吗");
            };
            ta1.SetParent (TimeMgr.single.dateTimeAxis, 0);
		}

		if (GUI.Button (new Rect (ox, oy += 30, 100, 30), "整点事件"))
		{
            if (RTime.R.onDataChanged == null)
                RTime.R.onDataChanged = onDateTimeChange;
			else
                RTime.R.onDataChanged = null;
		}
	}
}
