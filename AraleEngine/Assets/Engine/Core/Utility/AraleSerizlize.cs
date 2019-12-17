using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Reflection;
using System.Text;


namespace Arale.Engine
{
    public abstract class AraleSerizlize
    {
        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field,AllowMultiple = false, Inherited = true)]
        public class Field:Attribute
        {
            public Field()
            {
            }
        }
        
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

        //将一个对象序列化到xml文件
        public static bool saveXml(object o, string path)
        {
            XmlDocument xml = new XmlDocument();
            string xmlStr = string.Format ("<Root></Root>");
            xml.LoadXml(xmlStr);
            XmlElement root = xml.DocumentElement; 
            XmlDeclaration c = xml.CreateXmlDeclaration ("1.0", "utf-8", null);
            xml.InsertBefore (c, root);
            writeXml(o,"Root",root);
            //save version.xml
            byte[] utf8 = Encoding.GetEncoding("UTF-8").GetBytes(formatXml(xml));
            FileStream fs = new FileStream(path, FileMode.Create);
            fs.Write(utf8, 0, utf8.Length);
            fs.Close();
            return true;
        }

        static void writeXml(object o, string name, XmlNode node, bool element=false)
        {
            Type type = o.GetType();
            if (type.IsPrimitive || type.IsEnum || type == typeof(string))
            {//基本类型或字符串
                if (element)
                {//集合元素
                    node.AppendChild(node.OwnerDocument.CreateElement(name)).InnerText=o.ToString();
                }
                else
                //if (isValueChanged(o, fi))
                {//如果是默认值就不记录
                    node.Attributes.Append(node.OwnerDocument.CreateAttribute(name)).Value=o.ToString();
                }
                return;
            }

            if(type.IsArray)
            {//数组
                Array a = o as Array;
                for(int j=0;j<a.Length;++j)writeXml(a.GetValue(j), name, node, true);
                return;
            }

            if (typeof(IEnumerable).IsAssignableFrom(type))
            {//List
                Type[] tType = type.GetGenericArguments();
                if (tType.Length == 1)
                {
                    object list = o;
                    int count = (int)type.GetProperty("Count").GetValue(list, null);
                    for (int j = 0; j < count; ++j)
                    {
                        object lo = type.GetProperty("Item").GetValue(list, new object[]{ j });
                        writeXml(lo, name, node, true);
                    }
                }
                return;
            }

            node = node.AppendChild(node.OwnerDocument.CreateElement(name));
            node.Attributes.Append(node.OwnerDocument.CreateAttribute("Type")).Value=type.Name;

            //必须BindingFlags.Instance,否则获取GetCustomAttributes为空
            MemberInfo[] mbs = type.GetMembers(BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.Instance);
            for (int i = 0; i < mbs.Length; ++i)
            {
                MemberInfo mb = mbs[i];
                if (mb.GetCustomAttributes(typeof(AraleSerizlize.Field), false).Length < 1)continue;

                object val;
                Type valType;
                string valName;
                switch (mb.MemberType)
                {
                    case MemberTypes.Field:
                        FieldInfo fi = mb as FieldInfo;
                        val = fi.GetValue(o);
                        valType = fi.FieldType;
                        valName = fi.Name;
                        break;
                    case MemberTypes.Property:
                        PropertyInfo pi = mb as PropertyInfo;
                        val = pi.GetValue(o, null);
                        valType = pi.PropertyType;
                        valName = pi.Name;
                        break;
                    default:
                        continue;
                }
                writeXml(val, valName, node);
            }
        }

        static string formatXml(XmlDocument xml)
        {
            StringBuilder sb = new StringBuilder ();
            StringWriter sw = new StringWriter (sb);
            XmlTextWriter xtw = new XmlTextWriter (sw);
            xtw.Formatting = Formatting.Indented;
            xtw.Indentation = 1;
            xtw.IndentChar = '\t';
            xml.WriteTo (xtw);
            xtw.Close ();
            return sb.ToString ();
        }

        static bool isValueChanged(object o, FieldInfo fi)
        {
            Type oType = o.GetType();
            object co = oType.Assembly.CreateInstance(oType.Name);
            if (fi.GetValue(co)==null)return true;
            return fi.GetValue(co).ToString() != fi.GetValue(o).ToString();
        }

        /*public static class ReaderExtend
        {
            public static Vector3 FromString(this Vector3 o)
            {
                string[] ss = s.Split(',');
                Vector3 v;
                v.x = float.Parse(ss[0]);
                v.y = float.Parse(ss[1]);
                v.z = float.Parse(ss[2]);
                return v;
            }
        }*/
    }
}
