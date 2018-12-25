using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Arale.Engine
{

    using TableData = System.Collections.Generic.Dictionary<int, TableBase>;
    public class TableMgr : MgrBase<TableMgr>
    {
        public const string TestTablePath = "/Engine/Sample/Resources/Table/";
        public static bool  testModel;
        Dictionary<Type, TableData> DataPoolDic = new Dictionary<Type, TableData>();
        Dictionary<Type, ITableBuilder> builders = new Dictionary<Type, ITableBuilder>();
        public override void Init()
    	{
            builders.Add(typeof(TBBuff), new TableBuilder<TBBuff>());
            builders.Add(typeof(TBSkill), new TableBuilder<TBSkill>());
            builders.Add(typeof(TBPlayer), new TableBuilder<TBPlayer>());
            builders.Add(typeof(TBMonster), new TableBuilder<TBMonster>());
            builders.Add(typeof(TBBullet), new TableBuilder<TBBullet>());
            builders.Add(typeof(TBSound), new TableBuilder<TBSound>());
            mDirty = true;
    	}

    	bool mDirty;
    	public bool dirty{set{mDirty=value;}get{return mDirty;}}
    	public void Build()
    	{
    		//Released to reload
    		if(!mDirty)return;
    		DataPoolDic.Clear ();
    		GRoot.single.StartCoroutine(PreBuildTable());
    		mDirty=false;
    	}

    	IEnumerator PreBuildTable()
    	{
    		WaitForEndOfFrame w = new WaitForEndOfFrame ();
            yield return w;
    	}

    	public void Release()
    	{
    		foreach (var node in DataPoolDic) 
    		{
    			node.Value.Clear();		
    		}
    		DataPoolDic.Clear();
    	}

        public TableBase GetDataByKey(Type type, int key)
    	{
            TableData data_pool = GetDataPool(type);
            if (null != data_pool)
            {
                TableBase table_base;
                if (true == data_pool.TryGetValue(key, out table_base))
                {
                    return table_base;
                }
            }

    		Log.e(type + "配表没有找到对应的id：" + key + ". 谁该请吃可爱多啊?");
    		return null;
    	}

        public T GetData<T>(int key) where T : TableBase
        {
            TableData data_pool = GetDataPool(typeof(T));
            if (null != data_pool)
            {
                TableBase table_base;
                if (data_pool.TryGetValue(key, out table_base))
                {
                    return table_base as T;
                }

            }
            return null;
        }

        public TableData GetDataPool(Type type)
    	{
            TableData data_pool;
            if (DataPoolDic.TryGetValue(type, out data_pool))
    		{
                return data_pool;
    		}
    		else
    		{
    			ITableBuilder buider;
    			if(builders.TryGetValue(type, out buider))
    			{
                    data_pool = buider.build();
    				DataPoolDic.Add(type, data_pool);
                    return data_pool;
    			}
    			else
    			{
    				Log.e("表 " + type + " 找不到对应Builder");
    			}
    		}
    		return null;
    	}

    	public int GetPoolSize(Type type)
    	{
            TableData data_pool = GetDataPool(type);
    		return data_pool != null ? data_pool.Count : 0;
    	}

        public void AddDataPool(Type type, TableData data_pool)
        {
            if (DataPoolDic.ContainsKey(type) == false)
            {
                DataPoolDic.Add(type, data_pool);
            }
        }
    }

}
