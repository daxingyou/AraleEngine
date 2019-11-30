using UnityEngine;
using System.Collections;
using Arale.Engine;
using XLua;

public class LuaBuff : Buff
{
	LuaObject mL;
	public delegate bool OnEvent (LuaTable table, int evt, object param);
	public OnEvent luaOnEvent;

	protected override bool onEvent(int evt, object param)
	{
        if (mL == null)return false;
		return luaOnEvent (mL.mLT, evt, param);
	}

	protected override void onInit(Unit unit)
	{
		mL = LuaObject.newObject (mTB.lua, this);
        onEvent (EvtInit, unit);
	}

	protected override void onDeinit()
	{
        onEvent(EvtDeinit, null);
		mL.Dispose ();
		mL = null;
	}

    protected override void onMutex(Unit unit, TBBuff buff, ref bool reject)
	{
        if (reject = onEvent (EvtMutex, buff))return;
        base.onMutex (unit, buff, ref reject);
	}
}
