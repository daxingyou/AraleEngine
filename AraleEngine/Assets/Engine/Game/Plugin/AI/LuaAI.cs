using UnityEngine;
using System.Collections;
using Arale.Engine;

public class LuaAI : AIPlugin
{
    bool   mPlaying;  
	float  mTime;
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
        mUnit.bindLua(aiClass, false);
        mPlaying = mUnit.mLO != null;
        return mPlaying;
	}

	public override void stopAI()
	{
        if (!mPlaying)return;
        mUnit.unbindLua();
	}

	public override bool isPlay
	{
        get{ return mPlaying; }
	}
}
