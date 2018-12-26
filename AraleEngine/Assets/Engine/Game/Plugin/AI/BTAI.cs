/*using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BTAI : AIPlugin
{
	BTRoot mBTRoot = new BTRoot();
	public BTAI(Unit unit):base(unit)
	{
		#if UNITY_EDITOR
		mUnit.gameObject.AddComponent<BTDebug> ().mBTRoot = mBTRoot;
		#endif
	}

	public override void update()
	{
		if (!mBTRoot.isOK)return;
		if(target==null)hasTarget.mVal = 0;
		mBTRoot.update();
	}

	public override bool startAI(string btPath)
	{
		bool ret = mBTRoot.load (btPath);
		if (!ret)return false;
		regBTSlot ();
		mBTRoot.play ();
		return true;
	}

	public override void stopAI()
	{
		mBTRoot.stop ();
	}

	public override bool isPlay
	{
		get{ return mBTRoot.isPlaying; }
	}

	public override bool onEvent (int evt, object param, object sender)
	{
		if (!mBTRoot.isPlaying)return false;
		switch (evt)
		{
		case (int)UnitEvent.SkillBegin:
			isSkill.mVal = 1;
			return true;
		case (int)UnitEvent.SkillEnd:
			isSkill.mVal = 0;
			return true;
		case (int)UnitEvent.NavBegin:
			isPatrol.mVal = 1;
			return true;
		case (int)UnitEvent.NavEnd:
			isPatrol.mVal = 0;
			return true;
		}
		return false;
	}

	void regBTSlot()
	{
		if (!mBTRoot.isOK)return;
		isSkill  = mBTRoot.getVSlot ("skilling");
		isPatrol = mBTRoot.getVSlot ("patroling");
		isFlee   = mBTRoot.getVSlot ("fleeing");
		HP       = mBTRoot.getVSlot ("HP");
		HP.mVal  = 30;
		MP       = mBTRoot.getVSlot ("MP");
		hasTarget= mBTRoot.getVSlot ("hasTarget");
		mBTRoot.getFSlot ("doPatrol").mFunc = doPatrol;
		mBTRoot.getFSlot ("doTarget").mFunc = doTarget;
		mBTRoot.getFSlot ("doSkill").mFunc  = doSkill;
		mBTRoot.getFSlot ("doFlee").mFunc  = doFlee;
		mBTRoot.getFSlot ("doShow").mFunc = doShow;
		mBTRoot.getFSlot ("doSay").mFunc = doSay;
	}

	#region AI属性
	BTRoot.VSlot isSkill;
	BTRoot.VSlot isPatrol;
	BTRoot.VSlot isFlee;
	BTRoot.VSlot HP;
	BTRoot.VSlot MP;
	BTRoot.VSlot hasTarget;
	#endregion

	#region AI功能接口
	//巡逻
	int doPatrol(int arg)
	{
		if(base.doPatrol (1))isPatrol.mVal=1;
		return 0;
	}

	//查找目标
	int doTarget(int arg)
	{
		if (base.doTarget (0))hasTarget.mVal = 1;
		return 0;
	}

	//释放技能
	int doSkill(int idx)
	{
		if (base.doSkill (idx))isSkill.mVal = 1;
		return 0;
	}

	//逃离
	int doFlee(int arg)
	{
		if (base.doFlee (1))isFlee.mVal = 1;
		return 0;
	}

	//展示
	int doShow(int arg)
	{
		return 0;
	}

	//说话
	int doSay(int arg)
	{
		return 0;
	}
	#endregion
}
*/