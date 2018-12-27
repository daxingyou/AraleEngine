using System.Collections;
using System;

namespace Arale.Engine
{

    public class LuaEvent
    {
    	public string window;
    	public string ctrl;
    	public string eventId;
    	public Params param = new Params();
    	public class Params
    	{
    		internal protected Object userData;
    		internal protected Object[] param;
    		public Object getUserData()
    		{
    			return userData;
    		}
    		
    		public Object get(int idx)
    		{
    			return param[idx];
    		}
    		
    		public int count()
    		{
    			return param.Length;
    		}
    	}
    	
    	public LuaEvent(string windowId, string ctrlId, string eventid)
    	{
    		window = windowId;
    		ctrl = ctrlId;
    		eventId = eventid;
    	}
    	
    	public LuaEvent(string windowId, string eventid)
    	{
    		window = windowId;
    		eventId = eventid;
    	}
    	
    	public void setUserData(Object data)
    	{
    		param.userData = data;
    	}

    	public void send()
    	{
    		LuaRoot.pushEvent(this);
    	}
    	
    	public void send(Object param1)
    	{
    		param.param = new object[]{param1};
    		LuaRoot.pushEvent(this);
    	}
    	
    	public void send(Object param1, Object param2)
    	{
    		param.param = new object[]{param1, param2};
    		LuaRoot.pushEvent(this);
    	}
    	
    	public void send(Object param1, Object param2, Object param3)
    	{
    		param.param = new object[]{param1, param2, param3};
    		LuaRoot.pushEvent(this);
    	}
    }

}
