using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;


namespace Arale.Engine
{
    public abstract class AraleSerizlize
    {
        public abstract void write(BinaryWriter w);
        public abstract void read(BinaryReader r);
        public virtual void read(XmlNode node){}
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
        public static List<T> read<T>(XmlNode parent) where T:AraleSerizlize,new()
        {
            int n = parent.ChildNodes.Count;
            List<T> ls = new List<T>(n);
            for (int i = 0; i < n; ++i)
            {
                XmlNode node = parent.ChildNodes[i];
                T t = new T();
                t.read(node);
                ls.Add(t);
            }
            return ls;
        }
        public static void write<VType>(Dictionary<int,VType> dic, BinaryWriter w) where VType:AraleSerizlize
        {
            w.Write(dic.Count);
            IEnumerator e = dic.GetEnumerator();
            while (e.MoveNext())
            {
                KeyValuePair<int,VType> kp = (KeyValuePair<int,VType>)e.Current;
                w.Write(kp.Key);
                kp.Value.write(w);
            }
        }
        public static void read<VType>(Dictionary<int,VType> dic, BinaryReader r) where VType:AraleSerizlize,new()
        {
            int n = r.ReadInt32();
            for (int i = 0; i < n; ++i)
            {
                int key = r.ReadInt32();
                VType val = new VType();
                val.read(r);
                if (dic.ContainsKey(key))throw new UnityException("has same key="+key);
                dic[key] = val;
            }
        }

        public static void read<VType>(Dictionary<int,VType> dic, XmlNode parent) where VType:AraleSerizlize,new()
        {
            for (int i = 0,max=parent.ChildNodes.Count; i < max; ++i)
            {
                XmlNode n = parent.ChildNodes[i];
                int key = int.Parse(n.Attributes["id"].Value);
                VType val = new VType();
                val.read(n);
                if (dic.ContainsKey(key))throw new UnityException("has same key="+key);
                dic[key] = val;
            }
        }

        public static bool writeXml(object o)
        {
            return true;
        }
    }
}
