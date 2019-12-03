using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System;

namespace Arale.Engine
{
    
    public class TableBuilder
    {
        Type recoderType;
        public TableBuilder(Type type)
        {
            recoderType = type;
        }

        public Dictionary<int, TableBase> build()
    	{
            Dictionary<int, TableBase> dic = new Dictionary<int, TableBase>();
            if (TableMgr.TestModel)
            {
                string path = string.Format("Table/{0}", recoderType.Name);
                TextAsset ta = ResLoad.get(path).asset<TextAsset>();
                string[] datas = ta.text.Split(new char[]{'\n'}, System.StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < datas.Length; ++i)
                {
                    if(string.IsNullOrEmpty(datas[i])||datas[i][0]!='{')continue;
    				try
    				{
                        TableBase record = JsonUtility.FromJson(datas[i], recoderType) as TableBase;
    					dic.Add(record.id, record);
    				}
    				catch(Exception e)
    				{
    					Log.e ("fmt error:" + datas [i]+",path="+path);
    				}
                }
            }
            else
            {
                /*string path = string.Format("Table/{0}.json", typeof(T).Name);
                TextAsset ta = ResLoad.get(path).asset<TextAsset>();
                string[] texts = ta.text.Split('\n');
                int JsonNodeCount = texts.Length;

                for (int i = jsonStartIndex; i < JsonNodeCount; i++)
                {
                    T _t = new T();
                    _t.Init(texts[i].Split('♂'));

                    if (true == dic.ContainsKey(_t.id))
                    {
                        Log.e("表[" + typeof(T) + "]有重复id:" + _t.id, Log.Tag.DB);
                        continue;
                    }
                    dic.Add(_t.id, _t);
                }
                ResLoad.clearByPath(path);*/
            }
    		return dic;
    	}
    }

}
