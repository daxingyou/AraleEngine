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
		return luaOnEvent (mL.mLT, evt, param);
	}

	protected override void onInit(Unit unit)
	{
		mL = LuaObject.newObject (mTB.lua, this);
		onEvent (0, unit);
	}

	protected override void onDeinit()
	{
		onEvent(1, null);
		mL.Dispose ();
		mL = null;
	}

	protected override void onMutex(Unit unit, TBBuff buff)
	{
		if (onEvent (2, buff))return;
		base.onMutex (unit, buff);
	}
}
