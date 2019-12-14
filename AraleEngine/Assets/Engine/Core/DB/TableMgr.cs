using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Arale.Engine
{

    using TableData = System.Collections.Generic.Dictionary<int, TableBase>;
    public class TableMgr : MgrBase<TableMgr>
    {
        public const string TestTablePath = "/Demo/Resources/Table/";
        public static bool  TestModel;
        Dictionary<Type, TableData> DataPoolDic = new Dictionary<Type, TableData>();
        Dictionary<Type, TableBuilder> builders = new Dictionary<Type, TableBuilder>();
        public override void Init()
    	{
            Type[] types = Assembly.GetExecutingAssembly().GetTypes();
            for (int i = 0; i < types.Length; ++i)
            {
                Type t = types[i];
                if (t.BaseType != typeof(TableBase))continue;
                builders.Add(t, new TableBuilder(t));
       
            }
            mDirty = true;
    	}

    	bool mDirty;
    	public bool dirty{set{mDirty=value;}get{return mDirty;}}
    	public void Build()
    	{
    		//Released to reload
    		if(!mDirty)return;
    		DataPoolDic.Clear ();
    		GRoot.single.StartCoroutine(preBuildTable());
    		mDirty=false;
    	}

    	IEnumerator preBuildTable()
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

    		Log.e(type + "配表没有找到对应的id：" + key);
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
			Log.e(typeof(T) + "配表没有找到对应的id：" + key);
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
    			TableBuilder buider;
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

		public void ReloadData(Type type)
		{
			if (DataPoolDic.ContainsKey (type))
			{
				DataPoolDic.Remove (type);
			}
			GetDataPool (type);
            mDirty = true;
		}

		public string GenLuaExtend(Type type)
		{//导出lua表字符串
			TableData td = GetDataPool (type);
			StringBuilder sb = new StringBuilder(string.Format("L{0}", type.Name));
			sb.Append ("={");
			TableData.Enumerator  e = td.GetEnumerator ();
			while (e.MoveNext ())
			{
				int id = e.Current.Key;
				string extend = (e.Current.Value as TableBase)._extend;
				if (string.IsNullOrEmpty (extend))continue;
				sb.AppendFormat ("[{0}]={1}", id, "{id="+id+";"+extend+"};");
			}
			sb.Append ("}");
			return sb.ToString();
		}

		#if UNITY_EDITOR
		//运行时重新加载选中的lua文件
		[MenuItem("Assets/DevelopTools/Reload Table")]
		public static void Reload()
		{
			string tableName = Selection.activeObject.name;
			System.Type t = System.Type.GetType ("Arale.Engine." + tableName);
			if (t == null)
			{
				Debug.LogError ("请选中要重加载的配表文件");
				return;
			}
			single.ReloadData (t);
		}
		#endif
    }

}
