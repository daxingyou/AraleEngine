using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Arale.Engine
{
        
    public class TriggerMgr : MgrBase<TriggerMgr>
    {
        #region 触发条件
    	public class Condition
    	{
    		public enum CompareType
    		{
    			None,
    			Bigger,
    			BiggerAndEqual,
    			Smaller,
    			SmallerAndEqual,
    			Equal,
    			Unequal,
    		}
    		
    		public string key;
            public BoolDelegateI comparer;
    		public int val;

    		public Condition(string _key, CompareType type, int _val)
    		{
    			key = _key;
    			switch(type)
    			{
    			case CompareType.Bigger:
    				comparer =  Bigger;break;
    			case CompareType.BiggerAndEqual:
    				comparer =  BiggerAndEqual;break;
    			case CompareType.Smaller:
    				comparer =  Smaller;break;
    			case CompareType.SmallerAndEqual:
    				comparer =  SmallerAndEqual;break;
    			case CompareType.Equal:
    				comparer =  Equal;break;
    			case CompareType.Unequal:
    				comparer = Unequal;break;
    			}
    			val = _val;
    		}

    		bool Bigger(int v)
    		{
    			return v > val ? true : false;
    		}
    		
    		bool BiggerAndEqual(int v)
    		{
    			return v >= val ? true : false;
    		}
    		
    		bool Smaller(int v)
    		{
    			return v < val ? true : false;
    		}
    		
    		bool SmallerAndEqual(int v)
    		{
    			return v <= val ? true : false;
    		}
    		
    		bool Equal(int v)
    		{
    			return v == val ? true : false;
    		}

    		bool Unequal(int v)
    		{
    			return v != val ? true : false;
    		}
    	}
        #endregion


        #region 触发器对象
    	public class Trigger
    	{
    		public Condition[] condition = new Condition[8];
    		public int conditionNum;
    		public byte mask;//最多支持8个触发条件/
    		public byte reach;//达成标志/
            public VoidDelegate onTrigger;

            public Trigger(string[] _triggers, VoidDelegate _onTrigger)
    		{
    			conditionNum = _triggers.Length;
    			for(int i=0; i<conditionNum; ++i)condition[i] = StrToCondition(_triggers[i]);
    			onTrigger = _onTrigger;
    			mask=0x00;
    			reach=(byte)(0xff>>(8-conditionNum));
    		}

    		public void CheckTrigger(string triggerString, int val)
    		{
    			for(int i=0,max=conditionNum;i<max;++i)
    			{
    				if(condition[i].key!=triggerString)continue;
    				if(condition[i].comparer!=null)
    				{
    					bool ret = condition[i].comparer(val);
    					mask = ret?(byte)(mask|(0x01<<i)):(byte)(mask&(~(0x01<<i)));
    					break;
    				}
    			}
    			if(mask==reach)
    			{
    				onTrigger();//所有条件达成/
                    TriggerMgr.single.RemoveTrigger(this);
    			}
    		}

    		//判断第i个条件是否成立
    		public bool ConditionReach(int i)
    		{
    			if (i < 0 || i > 7)return false;
    			return ((mask & (0x01 << i)) == 0) ? false : true;
    		}

    		public void Reset()
    		{
    			mask=0x00;
    		}

    		Condition StrToCondition(string _conditon)
    		{
    			int i=_conditon.IndexOfAny (new char[]{'>','=','<'});
    			if(i<0)return new Condition(_conditon,Condition.CompareType.None,0);
    			switch(_conditon[i])
    			{
    			case '>':
    				if(_conditon[i+1]=='=')
    				{
    					return new Condition(_conditon.Substring(0,i), Condition.CompareType.BiggerAndEqual, int.Parse(_conditon.Substring(i+2)));
    				}
    				else
    				{
    					return new Condition(_conditon.Substring(0,i), Condition.CompareType.Bigger, int.Parse(_conditon.Substring(i+1)));
    				}
    			case '<':
    				if(_conditon[i+1]=='=')
    				{
    					return new Condition(_conditon.Substring(0,i), Condition.CompareType.SmallerAndEqual, int.Parse(_conditon.Substring(i+2)));
    				}
    				else
    				{
    					return new Condition(_conditon.Substring(0,i), Condition.CompareType.Smaller, int.Parse(_conditon.Substring(i+1)));
    				}
    			case '=':
    				if(_conditon[i-1]=='!')
    				{
    					return new Condition(_conditon.Substring(0,i-1), Condition.CompareType.Unequal, int.Parse(_conditon.Substring(i+1)));
    				}
    				else
    				{
    					return new Condition(_conditon.Substring(0,i), Condition.CompareType.Equal, int.Parse(_conditon.Substring(i+1)));
    				}
    			default:
    				return null;
    			}
    		}
    	}
        #endregion


    	Dictionary<string, List<Trigger>> triggerMap = new Dictionary<string, List<Trigger>>();
    	public override void Deinit()
        {
    		triggerMap.Clear ();
    	}

    	//注册trigger字符串和对应的回调/
    	//triggers在格式如下"coin>=5000,gold>=3000"
        public Trigger AddTrigger(string triggers, VoidDelegate onTrigger)
    	{
    		string[] s = triggers.Split (',');
    		Trigger trigger = new Trigger (s, onTrigger);
    		string e;
    		for(int i=0,max=trigger.conditionNum; i<max; ++i)
    		{
    			e = trigger.condition[i].key;
    			if(!triggerMap.ContainsKey(e))
    			{
    				triggerMap[e] = new List<Trigger>();
    			}
    			triggerMap[e].Add(trigger);
    		}
    		return trigger;
    	}

    	public void RemoveTrigger(Trigger trigger)
    	{
    		if(null==trigger)return;
    		string e;
    		for(int i=0,max=trigger.conditionNum;i<max;++i)
    		{
    			e = trigger.condition[i].key;
    			if(!triggerMap.ContainsKey(e))continue;
    			triggerMap[e].Remove(trigger);
    		}
    	}

    	//程序控制中，设置对应的标记，如金币发生改变值达到3000时就调用
    	//sendTriggerEvent("gold",3000)
    	public void SendTriggerEvent(string trigger, int val)
    	{
    		List<Trigger> triggers = triggerMap [trigger];
    		for(int i=triggers.Count-1; i>=0; --i)
    		{
    			triggers[i].CheckTrigger(trigger, val);
    		}
    	}
    }

}
