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
            public delegate object FromString();
            public delegate string ToString();
        }

        #region 2进制序列化
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
        #endregion


        #region Xml对象序列化
        public static bool saveXml(object o, string path)
        {
            XmlDocument xml = new XmlDocument();
            string xmlStr = string.Format ("<Root></Root>");
            xml.LoadXml(xmlStr);
            XmlElement root = xml.DocumentElement; 
            XmlDeclaration c = xml.CreateXmlDeclaration ("1.0", "utf-8", null);
            xml.InsertBefore (c, root);
            writeXml(o,null,root);
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
                {
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
            {//list 不支持字典集合，该集合可通过List重建
                IList list = o as IList;
                if (list == null)return;
                for(int i=0;i<list.Count;++i)writeXml(list[i], name, node, true);
                return;
            }

            switch (type.Name)
            {//存储为json属性
                case "Vector2":
                    if(o.Equals(default(Vector2)))return;
                    node.Attributes.Append(node.OwnerDocument.CreateAttribute(name)).Value = JsonUtility.ToJson(o).Replace('\"','\'');
                    return;
                case "Vector3":
                    if(o.Equals(default(Vector3)))return;
                    node.Attributes.Append(node.OwnerDocument.CreateAttribute(name)).Value = JsonUtility.ToJson(o).Replace('\"','\'');
                    return;
            }


            if(name!=null)node = node.AppendChild(node.OwnerDocument.CreateElement(name));
            node.Attributes.Append(node.OwnerDocument.CreateAttribute("Type")).Value=type.FullName;
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
                        valType = fi.FieldType;
                        if ((valType.IsPrimitive || valType.IsEnum || valType == typeof(string)) && isDefault(o, fi))
                        {//默认值未改过
                            continue;
                        }
                        valName = fi.Name;
                        val = fi.GetValue(o);
                        break;
                    case MemberTypes.Property:
                        PropertyInfo pi = mb as PropertyInfo;
                        valType = pi.PropertyType;
                        if ((valType.IsPrimitive || valType.IsEnum || valType == typeof(string)) && isDefault(o, pi))
                        {//默认值未改过
                            continue;
                        }
                        valName = pi.Name;
                        val = pi.GetValue(o, null);
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

        static bool isDefault(object o, FieldInfo fi)
        {
            Type oType = o.GetType();
            object co = oType.Assembly.CreateInstance(oType.Name);
            if (fi.GetValue(o) == null)return true;
            if (fi.GetValue(co)==null)return false;
            return fi.GetValue(co).ToString() == fi.GetValue(o).ToString();
        }

        static bool isDefault(object o, PropertyInfo pi)
        {
            Type oType = o.GetType();
            object co = oType.Assembly.CreateInstance(oType.Name);
            if (pi.GetValue(o,null) == null)return true;
            if (pi.GetValue(co,null)==null)return false;
            return pi.GetValue(co,null).ToString() == pi.GetValue(o,null).ToString();
        }

        public static object fromXml(Type type, string strXml)
        {
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(strXml);
            XmlElement root = xml.DocumentElement;
            bool isDefault = false;
            return readXml(type, null, root, ref isDefault);
        }

        static object readXml(Type type, string name, XmlNode node, ref bool defaultValue, bool element=false)
        {
            XmlAttribute attr = null;
            if (type.IsPrimitive || type.IsEnum || type == typeof(string))
            {//基本类型或字符串
                if (element)
                {//集合元素
                    return typeValue(type, node.InnerText);
                }
                else
                {
                    attr = node.Attributes[name];
                    if (attr == null)
                    {
                        defaultValue = true;
                        return null;
                    }
                    return typeValue(type, attr.Value);
                }
            }

            if(type.IsArray)
            {//数组
                XmlNodeList sn = node.SelectNodes(name);
                Array array = Array.CreateInstance(type.GetElementType(),sn.Count);
                bool isDefault = false;
                for (int j = 0; j < sn.Count; ++j)array.SetValue(readXml(type.GetElementType(), null, sn.Item(j), ref isDefault, true),j);
                return array;
            }

            if (typeof(IList).IsAssignableFrom(type))
            {//list 不支持字典集合，该集合可通过List重建
                XmlNodeList sn = node.SelectNodes(name);
                IList list = System.Activator.CreateInstance(type) as IList;
                bool isDefault = false;
                for(int j=0;j<sn.Count;++j)list.Add(readXml(type.GetGenericArguments()[0], null, sn.Item(j), ref isDefault, true));
                return list;
            }

            switch (type.Name)
            {//存储为json属性
                case "Vector2":
                    attr = node.Attributes[name];
                    if (attr == null)return default(Vector2);
                    return JsonUtility.FromJson<Vector2>(attr.Value);
                case "Vector3":
                    attr = node.Attributes[name];
                    if (attr == null)return default(Vector3);
                    return JsonUtility.FromJson<Vector3>(attr.Value);
            }

            if (name != null)node = node.SelectSingleNode(name);
            attr = node.Attributes["Type"];
            if (attr != null)type = Type.GetType(attr.Value, true);
            object o = System.Activator.CreateInstance(type);
            MemberInfo[] mbs = type.GetMembers(BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.Instance);
            for (int i = 0; i < mbs.Length; ++i)
            {
                MemberInfo mb = mbs[i];
                if (mb.GetCustomAttributes(typeof(AraleSerizlize.Field), false).Length < 1)continue;

                bool isDefault = false;
                switch (mb.MemberType)
                {
                    case MemberTypes.Field:
                        FieldInfo fi = mb as FieldInfo;
                        object val = readXml(fi.FieldType, fi.Name, node, ref isDefault);
                        if (isDefault)break;
                        fi.SetValue(o, val);
                        break;
                    case MemberTypes.Property:
                        PropertyInfo pi = mb as PropertyInfo;
                        object val2 = readXml(pi.PropertyType, pi.Name, node, ref isDefault);
                        if (isDefault)break;
                        pi.SetValue(o, val2, null);
                        break;
                    default:
                        continue;
                }
            }
            return o;
        }

        static object typeValue(Type type, string val)
        {
            if (type.IsEnum)return Enum.Parse(type, val);
            if (type == typeof(System.Type)) return Type.GetType(val);
            TypeCode code = System.Type.GetTypeCode(type);
            switch (code)
            {
                case TypeCode.Boolean:
                    return Boolean.Parse(val);
                case TypeCode.Char:
                    return Char.Parse(val);
                case TypeCode.SByte:
                    return SByte.Parse(val);
                case TypeCode.Byte:
                    return Byte.Parse(val);
                case TypeCode.Int16:
                    return Int16.Parse(val);
                case TypeCode.UInt16:
                    return UInt16.Parse(val);
                case TypeCode.Int32:
                    return Int32.Parse(val);
                case TypeCode.UInt32:
                    return UInt32.Parse(val);
                case TypeCode.Int64:
                    return Int64.Parse(val);
                case TypeCode.UInt64:
                    return UInt64.Parse(val);
                case TypeCode.Single:
                    return Single.Parse(val);
                case TypeCode.Double:
                    return Double.Parse(val);
                case TypeCode.String:
                    return val;
            }
            Debug.LogError("not support type="+type);
            return null;
        }
        #endregion
    }
}
