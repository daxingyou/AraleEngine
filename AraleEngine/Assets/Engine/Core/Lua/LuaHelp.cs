using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Arale.Engine;
using XLua;
using System;

public static class LuaHelp
{
	public static void ExportToLua()
	{
		Debug.LogError (typeof(List<object>));
	}

	[LuaCallCSharp]
	public static List<Type> lua_call_cs = new List<Type>(){
		//cs类型导出
		typeof(List<object>),
	};

	[CSharpCallLua]
	public static List<Type> cs_call_lua = new List<Type>(){
		//delegate导出
		typeof(Action),
		typeof(Action<LuaTable>),
		typeof(EventMgr.EventCallback),
		typeof(EventListener.VoidDelegate),
		typeof(UISwitch.OnValueChange),
		typeof(VoidDelegate),
		typeof(BoolDelegateI)
	};
}
