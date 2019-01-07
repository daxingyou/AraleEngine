#define USE_XLUA
using System.Xml;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Text;

#if USE_ULUA
using LuaInterface;
#elif USE_XLUA
using XLua;
#endif

namespace Arale.Engine
{

	public class LuaRoot : MonoBehaviour
	{
		public string LUA_PATH   = null;
		public const string ENCODESTRING = "aralehuan";
		static LuaRoot mThis;
		#if USE_ULUA
		//定义一个注解属性,实现自动注册Lua函数,lua可以调用这个注解了的函数,[RegLuaFunc("luafuncname")]
		public class RegLuaFunc:Attribute
		{
		private String functionName;
		public RegLuaFunc(String funcName)
		{
		functionName = funcName;
		}
		public String getFuncName()
		{
		return functionName;
		}
		}
		[NonSerialized]public LuaState mL = null;
		#elif USE_XLUA
		[NonSerialized]public LuaEnv   mL = null;
		#endif
		delegate byte[] ReadLuaFile(string path, bool encode=false);
		private static LuaFunction mNewLuaObject    = null;
		private static LuaFunction mPushLuaEvent    = null;
		private static LuaFunction mProcessLuaEvent = null;
		private static ReadLuaFile mReadLuaFile     = null;
		//即使processLuaEvent什么不做,因c#与lua交互调用也将耗时,当lua没有事件处理时应当将hasEvent置为false
		public static bool hasEvent = false;
		public static bool dirty    = true;
		bool mEncode = false;
		void Awake() {
			mThis = this;
			DontDestroyOnLoad (gameObject);
			Init ();
		}

		public void Init()
		{
			Log.i("Lua init begin");
			if (!dirty)return;
			//====================
			if (mL != null) 
			{//释放以便重加载
				Log.i("Lua Dispose");
				mL.Dispose ();
				mL = null;
			}
			//====================
			LUA_PATH = ResLoad.resPath + "/lua/";
			mReadLuaFile = readLuaFile;
			if (!Directory.Exists (LUA_PATH)) 
			{
				LUA_PATH = Application.streamingAssetsPath + "/Lua/";//路径区分大小写
				#if UNITY_ANDROID&&!UNITY_EDITOR
				mReadLuaFile = readAssetLuaFile;
				#endif
			}
			//====================
			#if USE_ULUA
			mL = new LuaState();
			bindLuaClass(this);
			#elif USE_XLUA
			mL = new LuaEnv();
			mL.AddLoader(LuaLoader);
			//引入第三方lua库
			mL.AddBuildin("rapidjson", XLua.LuaDLL.Lua.LoadRapidJson);
			mL.AddBuildin("lpeg", XLua.LuaDLL.Lua.LoadLpeg);
			mL.AddBuildin("pb", XLua.LuaDLL.Lua.LoadLuaProfobuf);
			//mL.AddBuildin("ffi", XLua.LuaDLL.Lua.LoadFFI);存在兼容问题
			#endif
			LuaHelp.ExportToLua ();
			byte[] tags = mReadLuaFile (LUA_PATH+"main.lua", false);
			mEncode = tags [2] == 0x3d ? false : true;
			//====================
			//设置Lua脚本根路径列表，并执行入口脚本main.lua
			mL.DoString ("package.path = package.path .. ';' .. '"+LUA_PATH+"?.lua';require 'main';");
			//====================
			mNewLuaObject    = (LuaFunction)mL["newLuaObject"];
			mPushLuaEvent    = (LuaFunction)mL["pushLuaEvent"];
			mProcessLuaEvent = (LuaFunction)mL["processLuaEvent"];
			mGameConfig      = (LuaTable)mL["LGameConfig"];
			((LuaFunction)mL ["main"]).Call();
			//====================
			dirty=false;
			Log.i("Lua init end");
		}

		public static LuaRoot single{
			get{return mThis;}
		}

		#if USE_ULUA
		public void bindLuaClass(System.Object obj)
		{
		foreach (MethodInfo mInfo in obj.GetType().GetMethods())
		{
		foreach (Attribute attr in Attribute.GetCustomAttributes(mInfo,typeof(RegLuaFunc)))
		{
		string luaFuncName = (attr as RegLuaFunc).getFuncName();
		Debug.Log("u3d reg lua func:"+luaFuncName);
		mL.RegisterFunction(luaFuncName, obj, mInfo);
		}
		}
		}
		#elif USE_XLUA
		public byte[] LuaLoader(ref string filepath)
		{
			string luaPath = LUA_PATH + filepath + ".lua";
			return mReadLuaFile (luaPath, mEncode);
		}
		#endif

		public LuaTable newTable(string luaTableName, object csObject=null)
		{
			object[] t = mNewLuaObject.Call(luaTableName, csObject);
			return (LuaTable)t[0];
		}

		public static byte[] readLuaFile(string path, bool encode=false)
		{
			if (!File.Exists (path))
			{
				Log.e ("no such file: " + path);
				return null;
			}
			byte[] bytes = File.ReadAllBytes(path);
			if(encode)Crypt.UnCrypt (bytes, ENCODESTRING);
			return bytes;
		}

		public static byte[] readAssetLuaFile(string path, bool encode=false)
		{
			WWW www = new WWW(path);
			while (!www.isDone)
			{
				System.Threading.Thread.Sleep(10);//比较hacker的做法
				continue;
			}

			if (!string.IsNullOrEmpty (www.error))
			{
				Debug.LogError ("no such file:" + path);
				return null;
			}
			byte[] bytes = www.bytes;
			if(encode)Crypt.UnCrypt (bytes, ENCODESTRING);
			return bytes;
		}

		static bool hasEncFile(string path)
		{
			WWW www = new WWW(path);
			while (!www.isDone)
			{
				System.Threading.Thread.Sleep(10);//比较hacker的做法
				continue;
			}
			return !string.IsNullOrEmpty (www.error);
		}

		#region lua消息机制
		void LateUpdate()
		{
			if (null != mProcessLuaEvent && hasEvent) mProcessLuaEvent.Call();
			gc ();
		}

		public static void pushEvent(LuaEvent e)
		{
			hasEvent = true;
			mPushLuaEvent.Call(e);
		}
		#endregion

		void gc()
		{
			if (mL == null)return;
			if (Time.time - LuaRoot.LastGCTime > LuaRoot.GCInterval)
			{
				LuaRoot.single.mL.Tick();
				LuaRoot.LastGCTime = Time.time;
			}
		}

		#region 原代码兼容接口，后期考虑去掉
		public static float LastGCTime = 0;
		public const float GCInterval = 1.0f;//1秒GC
		#endregion


		#region 游戏常量配置,引擎无关
		LuaTable mGameConfig;
		public T getConst<T>(string key)
		{
			return mGameConfig.Get<T> (key);
		}
		//获取子表的键值"subtable.value"
		public T getPathConst<T>(string path)
		{
			return mGameConfig.GetInPath<T> (path);
		}
		#endregion


		#region 导出枚举
		#if UNITY_EDITOR
		const string RET = "\r\n";
		const string TAB = "\t"; 
		[MenuItem("DevelopTools/Lua/Gen Enum")]
		public static void genEnum()
		{
			string luaCode = "--Auto Gen,Don't modify"+RET;
			luaCode += "Enum={}" + RET;
			luaCode += Enum2Lua(typeof(MyMsgId))+RET;
			luaCode += Enum2Lua(typeof(Log.Tag))+RET;
			luaCode += Enum2Lua(typeof(UnitEvent))+RET;
			luaCode += Enum2Lua(typeof(AttrID))+RET;
			File.WriteAllText(Application.streamingAssetsPath + "/Lua/LuaEnum.lua", luaCode);
		}

		static string Enum2Lua(Type t)
		{//参数为具体的枚举值
			Debug.Assert(t.IsEnum);
			string s = "Enum."+t.Name+"={"+RET;
			string[] names = Enum.GetNames(t);
			Array values = Enum.GetValues(t);
			for (int i = 0; i < names.Length; ++i)
			{
				s += TAB + names[i] + "=" + (int)values.GetValue(i)+","+RET;
			}
			s+="}"+RET;
			return s;
		}
		#endif
		#endregion
	}

}