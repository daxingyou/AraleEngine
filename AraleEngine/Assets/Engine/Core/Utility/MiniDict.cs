using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Arale.Engine
{
        
    public class MiniDict
    {
    	Dictionary<int, string> mDict = new Dictionary<int, string>();

    	public MiniDict(string ctx)
    	{
    		string[] texts = ctx.Split('\n');
    		for(int i=0;i<texts.Length;++i)
    		{
    			string s = texts[i];
    			int idx = s.IndexOf(' ');
    			int id = int.Parse(s.Substring(0,idx));
    			if (true == mDict.ContainsKey(id))
    			{
    				Debug.LogError("miniDict has id="+id);
    				continue;
    			}
    			mDict.Add(id, s.Substring(idx+1));
    		}
    	}

    	public string id2str(int id)
    	{
    		string val = null;
    		if (mDict.TryGetValue (id, out val)) {
    			return val;
    		} else {
    			return null;
    		}
    	}
    }

}

