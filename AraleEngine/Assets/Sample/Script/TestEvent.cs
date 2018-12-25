using UnityEngine;
using System.Collections;
using Arale.Engine;

public class TestEvent : MonoBehaviour {

	// Use this for initialization
	string mCondition1 = "money>=10,kill>5";
	string mCondition2 = "money>=10,kill>6";
	int money = 0;
	int kill  = 0;
	string msg = "";
    TriggerMgr.Trigger[] mTrigger = new TriggerMgr.Trigger[2] ;
	void Start () {
        mTrigger[0] = TriggerMgr.single.AddTrigger(mCondition1,onTrigger0);
        mTrigger[1] = TriggerMgr.single.AddTrigger(mCondition2,onTrigger1);
	}
	
	// Update is called once per frame
	void OnDestroy () {
        TriggerMgr.single.RemoveTrigger (mTrigger[0]);
        TriggerMgr.single.RemoveTrigger (mTrigger[1]);
	}

	void onTrigger0()
	{
		msg += "condtion1 reach\n";
	}

	void onTrigger1()
	{
		msg += "condtion2 reach\n";
	}

	void OnGUI()
	{
		GUILayout.Label (mCondition1);
		GUILayout.Label (mCondition2);
		if(GUILayout.Button("add money:"+money))
		{
            TriggerMgr.single.SendTriggerEvent("money", ++money);
		}
		if(GUILayout.Button("add kill:"+kill))
		{
            TriggerMgr.single.SendTriggerEvent("kill", ++kill);
		}
		GUILayout.Label ("msg:\n"+msg);
	}
}
