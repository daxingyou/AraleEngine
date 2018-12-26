using UnityEngine;
using System.Collections;
using LuaInterface;
using System;
using System.Reflection;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Arale.Engine
{
        
    public class LuaRoot : MonoBehaviour 
    {
    	public static string LUA_PATH;
    	static LuaRoot mThis;
        LuaState mL = null;
    	static LuaFunction mNewLuaObject    = null;
    	static LuaFunction mProcessLuaEvent = null;
    	static LuaFunction mPushLuaEvent    = null;

        //即使processLuaEvent什么不做,因c#与lua交互调用也将耗时,当lua没有事件处理时应当将hasEvent置为false
        public static bool hasEvent = false;
		public static LuaRoot single{get{ return mThis;}}
        void Awake() {
			LUA_PATH = Application.dataPath + "/lua/";
    		mThis = this;
            DontDestroyOnLoad(gameObject);
			Init ();
        }

		public void Init()
		{
			Log.i ("lua init begin path="+LUA_PATH, Log.Tag.Default);
			if (mL != null)
			{
				mL.Dispose (true);
			}
			mL = new LuaState();
			bindLuaClass(this);
			//添加lua查找路径,并执行入口脚本main.lua
    		mL.DoString ("package.path = package.path .. ';' .. '" + LUA_PATH + "?.lua';require 'main';");
    		mNewLuaObject    = (LuaFunction)mL["newLuaObject"];
    		mPushLuaEvent    = (LuaFunction)mL["pushEvent"];
    		mProcessLuaEvent = (LuaFunction)mL["processEvent"];
			LuaHelp.ExportToLua ();
            ((LuaFunction)mL["main"]).Call();
    		Log.i ("lua init end");
        }
	
    	#region lua辅助接口
    	//定义一个注解属性,实现自动注册Lua函数,lua可以调用这个注解了的函数[RegLuaFunc("luaFuncName")]
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

    	void bindLuaClass(System.Object obj)
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

    	public static LuaTable newLuaTable(string luaTableName, object csObject=null)
    	{
    		object[] t = mNewLuaObject.Call(luaTableName,csObject);
    		return (LuaTable)t[0];
    	}

    	public static String readLuaStringFromFile(String path)
    	{//utf8->unicode
    		FileStream fs = new FileStream(LUA_PATH+path, FileMode.Open);
    		if(fs==null)return null;
    		int sz = (int)fs.Length;
    		byte[] bufUTF8 = new byte[sz];
    		fs.Read(bufUTF8, 0, sz);
    		byte[] buf = Encoding.Convert(Encoding.UTF8, Encoding.Unicode, bufUTF8);
    		return (new UnicodeEncoding()).GetString(buf, 0, buf.Length);
    	}

    	public static String readLuaStringFromFile(string path, out int bytes)
    	{//utf8
    		bytes=0;
    		path = LuaRoot.LUA_PATH + path;
    		if (!File.Exists (path))return null;
    		byte[] bufUTF8 = File.ReadAllBytes (path);
    		//Crypt.UnCrypt (bufUTF8, "wanghuan");
    		return (new UTF8Encoding()).GetString(bufUTF8, 0, bytes=bufUTF8.Length);
    		//LuaDLL.luaL_loadbuffer要求传入utf8编码的字符串和字节数
    	}
    	#endregion

    	#region lua事件处理
        void LateUpdate() {
    		if (null != mProcessLuaEvent && hasEvent) mProcessLuaEvent.Call();
        }

        public static void pushEvent(LuaEvent e) {
            hasEvent = true;
            mPushLuaEvent.Call(e);
        }
    	#endregion
    }

}