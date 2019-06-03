using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization.Formatters.Binary;


namespace Arale.Engine
{
    public abstract class AraleSerizlize
    {
        public abstract void write(BinaryWriter w);
        public abstract void read(BinaryReader r);
        public static void write<T>(List<T> ls, BinaryWriter w) where T:AraleSerizlize
        {
            int n = ls.Count;
            w.Write(n);
            for (int i = 0; i < n; ++i)ls[i].write(w);
        }
        public static List<T> read<T>(BinaryReader r) where T:AraleSerizlize,new()
        {
            int n = r.ReadInt32();
            List<T> ls = new List<T>(n);
            for (int i = 0; i < n; ++i)
            {
                T t = new T();
                t.read(r);
                ls.Add(t);
            }
            return ls;
        }
        public static void write<T>(Dictionary<string,T> dic, BinaryWriter w) where T:AraleSerizlize
        {
            w.Write(dic.Count);
            IEnumerator e = dic.GetEnumerator();
            while (e.MoveNext())
            {
                KeyValuePair<string,T> kp = (KeyValuePair<string,T>)e.Current;
                w.Write(kp.Key);
                kp.Value.write(w);
            }
        }
        public static void read<T>(Dictionary<string,T> dic, BinaryReader r) where T:AraleSerizlize,new()
        {
            int n = r.ReadInt32();
            for (int i = 0; i < n; ++i)
            {
                string key = r.ReadString();
                T val = new T();
                val.read(r);
                if (dic.ContainsKey(key))throw new UnityException("has same key="+key);
                dic[key] = val;
            }
        }
    }
}
