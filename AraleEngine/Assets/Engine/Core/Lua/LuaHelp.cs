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
	}
	public static List<object> List_object{get{return new List<object>();}}

	//类型导出
	[LuaCallCSharp]
	public static List<Type> lua_call_cs = new List<Type>(){
		typeof(List<object>),
	};

	//delegate导出
	[CSharpCallLua]
	public static List<Type> cs_call_lua = new List<Type>(){
		typeof(Action),
		typeof(Action<LuaTable>),
		typeof(EventMgr.EventCallback),
		typeof(EventListener.VoidDelegate),
		typeof(UISwitch.OnValueChange),
		typeof(VoidDelegate),
		typeof(BoolDelegateI)
	};

	//黑名单
	[BlackList]
	public static List<List<string>> BlackList = new List<List<string>>()  {
		//第1个参数为类型,第2参数为类型的方法或属性,表示不导出该类型的该方法
		new List<string>(){"UnityEngine.UI.Text", "OnRebuildRequested"},
		new List<string>(){"UnityEngine.WWW", "movie"},
		new List<string>(){"UnityEngine.Texture2D", "alphaIsTransparency"},
		new List<string>(){"UnityEngine.Security", "GetChainOfTrustValue"},
		new List<string>(){"UnityEngine.CanvasRenderer", "onRequestRebuild"},
		new List<string>(){"UnityEngine.Light", "areaSize"},
		new List<string>(){"UnityEngine.AnimatorOverrideController", "PerformOverrideClipListCleanup"},
		#if !UNITY_WEBPLAYER
		new List<string>(){"UnityEngine.Application", "ExternalEval"},
		#endif
		new List<string>(){"UnityEngine.GameObject", "networkView"}, //4.6.2 not support
		new List<string>(){"UnityEngine.Component", "networkView"},  //4.6.2 not support
		new List<string>(){"System.IO.FileInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
		new List<string>(){"System.IO.FileInfo", "SetAccessControl", "System.Security.AccessControl.FileSecurity"},
		new List<string>(){"System.IO.DirectoryInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
		new List<string>(){"System.IO.DirectoryInfo", "SetAccessControl", "System.Security.AccessControl.DirectorySecurity"},
		new List<string>(){"System.IO.DirectoryInfo", "CreateSubdirectory", "System.String", "System.Security.AccessControl.DirectorySecurity"},
		new List<string>(){"System.IO.DirectoryInfo", "Create", "System.Security.AccessControl.DirectorySecurity"},
		new List<string>(){"UnityEngine.MonoBehaviour", "runInEditMode"},
	};
}
