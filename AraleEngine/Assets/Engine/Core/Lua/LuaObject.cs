using UnityEngine;
using System.Collections;
using LuaInterface;
using System;
using System.Reflection;
using System.IO;
using System.Text;

namespace Arale.Engine
{

    public class LuaObject
    {
    	public LuaTable mLT;
    	LuaObject(LuaTable luaTable)
    	{
            mLT = luaTable;
    	}

    	public static LuaObject newObject(string luaClassName, object csObject=null)
    	{
    		if (string.IsNullOrEmpty (luaClassName))return null;
    		LuaTable luaTable = LuaRoot.newLuaTable(luaClassName,csObject);
    		if (luaTable == null)return null;

    		return new LuaObject (luaTable);
    	}

    	public void Dispose ()
    	{
            mLT = null;
    	}

        public object[] call(string func)
        {
            LuaFunction f = (LuaFunction)mLT[func];
            if (f == null)return null;
            return f.Call(mLT);
        }

        public object[] call(string func, object param1)
    	{
            LuaFunction f = (LuaFunction)mLT[func];
            if (f == null)return null;
            return f.Call (mLT, param1);
    	}

        public object[] call(string func, object param1, object param2)
    	{
            LuaFunction f = (LuaFunction)mLT[func];
            if (f == null)return null;
            return f.Call (mLT, param1, param2);
    	}

        public object[] call(string func, object param1, object param2, object param3)
    	{
            LuaFunction f = (LuaFunction)mLT[func];
            if (f == null)return null;
            return f.Call (mLT, param1, param2, param3);
    	}

    	public T value<T>(string field)
    	{
            //xlua
            //return mLuaTable.Get<T>(field);
    		//ulua
            return (T)mLT [field];
    	}
    }
}
