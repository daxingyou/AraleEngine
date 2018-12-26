using UnityEngine;
using System.Collections;
using Arale.Engine;

public class LuaAI : AIPlugin
{
	LuaObject mLAI;
	float     mTime;
	public TimeMgr.TimeAxis timer{ get; protected set;}
	public LuaAI(Unit unit):base(unit)
	{
		timer = new TimeMgr.TimeAxis ();
	}

	public override void update()
	{
		if (!isPlay)return;
		timer.Update(mTime);
		mTime += Time.deltaTime;
	}

	public override bool startAI(string aiClass)
	{
		mLAI = LuaObject.newObject (aiClass, this);
		return true;
	}

	public override void stopAI()
	{
		if (mLAI == null)return;
		mLAI.Dispose ();
		mLAI = null;
	}

	public override bool isPlay
	{
		get{ return mLAI!=null; }
	}

	public override bool onEvent (int evt, object param, object sender)
	{
		if (!isPlay)return false;
		switch(evt)
		{
		case (int)UnitEvent.NavEnd:
			break;
		case (int)UnitEvent.SkillEnd:
			break;
		}

		return false;
	}
}
