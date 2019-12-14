using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System;
using System.Reflection;

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
                //文件以.txt结尾
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
                ResLoad.clearByPath(path);
            }
            else
            {
                //文件以.bytes结尾，否则字符串处理可能有错
                string path = string.Format("Table/{0}", recoderType.Name);
                TextAsset ta = ResLoad.get(path).asset<TextAsset>();
                byte[] data = ta.bytes;
                using (MemoryStream ms = new MemoryStream(ta.bytes))
                {
                    BinaryReader r = new BinaryReader(ms);
                    int count = r.ReadInt32();   //记录数
                    string head = r.ReadString();//表头
                    int[] idx = TypeReader(head);
                    for (int i = 0; i < count; ++i)
                    {
                        TableBase record = Activator.CreateInstance(recoderType) as TableBase;
                        record.read(r, idx);
                        dic.Add(record.id, record);
                    }
                }
                ResLoad.clearByPath(path);
            }
    		return dic;
    	}

        int[] TypeReader(string head)
        {
            string[] s = head.Split(',');
            FieldInfo[] fi = recoderType.GetFields();
            int[] idx = new int[s.Length];
            for (int i = 0; i < s.Length; ++i)
            {
                idx[i] = -1;
                for (int j = 0, max = fi.Length; j < max; ++j)
                {
                    if (s[i]==fi[j].Name)
                    {
                        idx[i]=j;
                        break;
                    }
                }
            }
            return idx;
        }
    }

}
