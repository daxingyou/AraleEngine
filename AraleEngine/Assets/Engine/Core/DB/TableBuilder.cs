using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;

namespace Arale.Engine
{
    
public abstract class ITableBuilder{
	protected const int jsonStartIndex = 0;
	public abstract Dictionary<int, TableBase> build ();
}

public class TableBuilder<T> : ITableBuilder where T:TableBase, new()
{
	public override Dictionary<int, TableBase> build()
	{
        Dictionary<int, TableBase> dic = new Dictionary<int, TableBase>();
        if (TableMgr.testModel)
        {
            string path = string.Format("Table/{0}", typeof(T).Name);
            TextAsset ta = ResLoad.get(path).asset<TextAsset>();
            string[] datas = ta.text.Split(new char[]{'\n'}, System.StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < datas.Length; ++i)
            {
                TableBase record = JsonUtility.FromJson(datas[i], typeof(T)) as TableBase;
                dic.Add(record.id, record);
            }
        }
        else
        {
            string path = string.Format("Table/{0}.json", typeof(T).Name);
            TextAsset ta = ResLoad.get(path).asset<TextAsset>();
            string[] texts = ta.text.Split('\n');
            int JsonNodeCount = texts.Length;

            for (int i = jsonStartIndex; i < JsonNodeCount; i++)
            {
                T _t = new T();
                _t.Init(texts[i].Split('♂'));

                if (true == dic.ContainsKey(_t.id))
                {
                    Log.e("表 " + typeof(T) + " 中有重复id: " + _t.id + " 【谁该请吃可爱多啊？】", Log.Tag.DB);
                    continue;
                }
                dic.Add(_t.id, _t);
            }
            ResLoad.clearByPath(path);
        }
		return dic;
	}
}

}
