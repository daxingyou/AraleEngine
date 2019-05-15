#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Collections.Generic;
using System.Reflection;
using ProtoBuf;
using System.ComponentModel;
using ProtoBuf.Meta;
using UnityEditor;
namespace Arale.Engine
{


public class LuaProtoGen
{
    string outPath;
    HashSet<Type> genFilter = new HashSet<Type>();
    HashSet<Type> hasGen  = new HashSet<Type>();
    static HashSet<Type> ableGen  = new HashSet<Type>();
    static List<Type> genList    = new List<Type>();
    public LuaProtoGen(string outPath)
    {
        this.outPath = outPath;
    }

    public void addFilter(Type type)
    {
        genFilter.Add(type);
    }

    public void genLuaPB()
    {
        ableGen.Clear();
        hasGen.Clear();
        genList.Clear();
        DirectoryInfo di = new DirectoryInfo(outPath);
        if (!di.Exists)throw new Exception("out path not exists path="+outPath);
        foreach (FileInfo fi in di.GetFiles())fi.Delete();
        listPBTypes();
        for(int i=0;i<genList.Count;++i)genLuaPB(genList[i], outPath);
    }

    //获取所有设置了ProtoContractAttribute标签属性的类(PB类型)
    void listPBTypes()
    {
        Assembly[] abs = AppDomain.CurrentDomain.GetAssemblies();
        foreach (Assembly ab in abs)//遍历程序集
        {
            foreach (Type t in ab.GetExportedTypes())//遍历类型
            {
                ProtoContractAttribute attr = Attribute.GetCustomAttribute(t, typeof(ProtoContractAttribute)) as ProtoContractAttribute;
                if (attr == null)continue;
                ableGen.Add(t);
                if (genFilter.Count>0&&!genFilter.Contains(t))continue;
                genList.Add(t);
            }
        }
    }
        
    void genLuaPB(Type t, string saveFolder)
    {
        if (hasGen.Contains(t))return;
        hasGen.Add(t);
        //=======Gen Code==========
        PBType pbType = new PBType(t);
        string pbTypeCode = pbType.genLuaCode();
        Debug.LogError(pbTypeCode);
        string path = saveFolder+t.Namespace+".lua";
        if (!File.Exists(path))
        {
            File.WriteAllText(path, "--AutoGen Code,Don't Modify"+RET+RET);
        }
        string str = File.ReadAllText(path);
        str += pbTypeCode;
        File.WriteAllText(path, str);
        //=========================
    }

    #region 辅助
    //参考ValueMember.cs TryGetCoreSerializer, TypeModel.cs TrySerializeAuxiliaryType
    static string getSerializer(DataFormat format, Type valueType, bool isWrite, out WireType wireType)
    {
        ProtoTypeCode typecode = Helpers.GetTypeCode(valueType);
        if (Helpers.IsEnum(valueType))
        {//自定义枚举类型
            wireType = WireType.Variant;
            return isWrite?"WriteInt32":"ReadInt32";
        }

        ProtoTypeCode code = Helpers.GetTypeCode(valueType);
        switch (code)
        {
            case ProtoTypeCode.Byte: wireType = GetIntWireType(format, 32); return isWrite?"WriteByte":"ReadByte";
            case ProtoTypeCode.SByte: wireType = GetIntWireType(format, 32); return isWrite?"WriteSByte":"ReadSByte";
            case ProtoTypeCode.Int16: wireType = GetIntWireType(format, 32); return isWrite?"WriteInt16":"ReadInt16";
            case ProtoTypeCode.UInt16: wireType = GetIntWireType(format, 32); return isWrite?"WriteUInt16":"ReadUInt16";
            case ProtoTypeCode.Int32: wireType = GetIntWireType(format, 32); return isWrite?"WriteInt32":"ReadInt32";
            case ProtoTypeCode.UInt32: wireType = GetIntWireType(format, 32); return isWrite?"WriteUInt32":"ReadUInt32";
            case ProtoTypeCode.Int64: wireType = GetIntWireType(format, 64); return isWrite?"WriteInt64":"ReadInt64";
            case ProtoTypeCode.UInt64: wireType = GetIntWireType(format, 64); return isWrite?"WriteUInt64":"ReadUInt64";
            case ProtoTypeCode.Char: wireType = WireType.Variant; return isWrite?"WriteUInt16":"ReadUInt16";
            case ProtoTypeCode.String: wireType = WireType.String; return isWrite?"WriteString":"ReadString";
            case ProtoTypeCode.Single: wireType = WireType.Fixed32; return isWrite?"WriteSingle":"ReadSingle";
            case ProtoTypeCode.Double: wireType = WireType.Fixed64; return isWrite?"WriteDouble":"ReadDouble";
            case ProtoTypeCode.Boolean: wireType = WireType.Variant; return isWrite?"WriteBoolean":"ReadBoolean";
            case ProtoTypeCode.ByteArray: wireType = WireType.String; return isWrite?"WriteBytes":"ReadBytes";
                //==扩展类型
            case ProtoTypeCode.DateTime: wireType = GetDateTimeWireType(format); return isWrite?"WriteDateTime":"ReadDateTime";
            case ProtoTypeCode.Decimal: wireType = WireType.String; return isWrite?"WriteDecimal":"ReadDecimal";
            case ProtoTypeCode.TimeSpan: wireType = GetDateTimeWireType(format); return isWrite?"WriteTimeSpan":"ReadTimeSpan";
            case ProtoTypeCode.Guid: wireType = WireType.String; return isWrite?"WriteGuid":"ReadGuid";
            case ProtoTypeCode.Uri: wireType = WireType.String; return isWrite?"WriteString":"ReadString"; // treat as string; wrapped in decorator later
            case ProtoTypeCode.Type: wireType = WireType.String; return isWrite?"WriteType":"ReadType";
        }

        //自定义PB类型
        wireType = WireType.String;
        return isWrite?"WriteObject":"ReadObject";;
    }

    static WireType GetIntWireType(DataFormat format, int width)
    {
        switch(format)
        {
            case DataFormat.ZigZag: return WireType.SignedVariant;
            case DataFormat.FixedSize: return width == 32 ? WireType.Fixed32 : WireType.Fixed64;
            case DataFormat.TwosComplement:
            case DataFormat.Default: return WireType.Variant;
            default: throw new InvalidOperationException();
        }
    }

    static WireType GetDateTimeWireType(DataFormat format)
    {
        switch (format)
        {
            case DataFormat.Group: return WireType.StartGroup;
            case DataFormat.FixedSize: return WireType.Fixed64;
            case DataFormat.Default: return WireType.String;
            default: throw new InvalidOperationException();
        }
    }

    const  string RET = "\r\n";
    static string TAB(int tab)
    {
        string s = "";
        while(tab-->0)s+="\t";
        return s;
    } 
    static string TypeName(Type t)
    {
        int n = t.Namespace.Length;
        return t.FullName.Replace('+', '.').Replace('.','_').Substring(n>0?n+1:0);
    }
    #endregion

    #region 生成luaPB
    class PBType
    {
        protected int tab;
        protected List<PBMember> members = new List<PBMember>();
        protected Type   type;
        protected string typeName;
        public PBType(Type t)
        {
            Debug.LogError("Type="+t.FullName);
            this.type = t;
            this.typeName = TypeName(t);
            parse();
        }

        void addMember(PBMember mb)
        {
            Debug.LogError("Add Member:" + mb.name + ",type=" + mb.valueType);
            mb.tab = tab + 1;
            mb.isEmum = type.IsEnum;
            members.Add(mb);
            //添加关联类型
            Type associateType = mb.GetAssociateType();
            if (ableGen.Contains(associateType))genList.Add(associateType);
        }

        void parse()
        {
            MemberInfo[] mbs = type.GetMembers();
            foreach (MemberInfo mb in mbs)
            {
                PBMember pbm = new PBMember();
                //获取所有ProtoMemberAttribute属性(PB字段)
                ProtoMemberAttribute attr= Attribute.GetCustomAttribute(mb, typeof(ProtoMemberAttribute)) as ProtoMemberAttribute;
                if (attr != null)
                {
                    pbm.name = attr.Name;
                    pbm.tag  = attr.Tag;
                    pbm.datafmt = attr.DataFormat;
                    DefaultValueAttribute dva = Attribute.GetCustomAttribute(mb, typeof(DefaultValueAttribute)) as DefaultValueAttribute;
                    if (dva != null)
                    {
                        pbm.defaultVaule = dva.Value;
                    }
                    pbm.isRequire = attr.IsRequired;
                    pbm.isPacked = attr.IsPacked;
                    pbm.isOverrideList = attr.OverwriteList;
                }

                ProtoEnumAttribute pea = Attribute.GetCustomAttribute(mb, typeof(ProtoEnumAttribute)) as ProtoEnumAttribute;
                if(pea != null)
                {
                    pbm.name = pea.Name;
                    pbm.defaultVaule = pea.Value;
                }

                if (attr == null && pea == null)
                    continue;

                Type memberType = null;
                switch (mb.MemberType)
                {
                    case MemberTypes.Field:
                        memberType = ((FieldInfo)mb).FieldType; break;
                    case MemberTypes.Property:
                        memberType = ((PropertyInfo)mb).PropertyType; break;
                    default:
                        throw new NotSupportedException(mb.MemberType.ToString());
                }
                pbm.valueType = memberType;
                addMember(pbm);
            }
        }

        protected string member()
        {
            string s = "";
            foreach (PBMember mb in members)
            {
                s += mb.member()+";"+RET;
            }
            return string.IsNullOrEmpty(s)?s:s+RET;
        }

        protected virtual string serializer()
        {
            string s = TAB(tab+1) + "Serializer = function(self,dst)"+RET;
            foreach (PBMember mb in members)
            {
                string fs = mb.serializer();
                if (string.IsNullOrEmpty(fs))continue;
                s += fs;
            }
            s += TAB(tab+1) + "end;"+RET;
            return s+RET;
        }

        protected virtual string deserialize()
        {
            string s = TAB(tab+1) + "Deserialize = function(self,src)"+RET;
            s += TAB(tab+2)+"local tag;"+RET;
            foreach (PBMember mb in members)
            {
                string fs = mb.deserialize();
                if (string.IsNullOrEmpty(fs))continue;
                s += fs;
            }
            s += TAB(tab+1) + "end;"+RET;
            return s+RET;
        }

        public virtual string genLuaCode()
        {
            if (type.IsEnum)
            {
                string defBegin = string.Format("if not {0} then", typeName)+RET;
                string typeBegin = string.Format("{0} =", typeName)+RET+"{"+RET;
                string typeEnd = "}"+RET+RET;
                string defEnd   = "end"+RET+RET;
                return defBegin + typeBegin + member() + typeEnd + defEnd;
            }
            else
            {
                string defBegin = string.Format("if not {0} then", typeName) + RET;
                string typeBegin = string.Format("{0} =", typeName) + RET + "{" + RET;
                string typeEnd = "}" + RET + RET;
                string createClass = string.Format("createClass(\"{0}\",{1},{2})", typeName, typeName, "LProtoWrite") + RET;
                string defEnd = "end" + RET + RET;
                return defBegin + typeBegin + member() + serializer() + deserialize() + typeEnd + createClass + defEnd;
            }
        }
    }

    class PBMember
    {
        public int        tab;
        public string     name;
        public int        tag;
        public DataFormat datafmt;
        public Type       valueType;
        public object     defaultVaule;
        public bool       isRequire;
        public bool       isPacked;
        public bool       isOverrideList;
        public bool       isEmum;

        bool isByteArray
        {
            get{return valueType == typeof(byte[]);}
        }
        bool isArray
        {
            get{return !isByteArray && valueType.IsArray;}
        }
        bool isList
        {
            get{return !isByteArray && valueType!=typeof(string) && typeof(IEnumerable).IsAssignableFrom(valueType);}
        }

        public Type GetAssociateType()
        {
            Type type = valueType;
            if (isArray || isList)
            {
                Type[] tType = valueType.GetGenericArguments();//获取泛类型列表
                type = tType.Length>0?tType[0]:valueType.GetElementType();
            }
            return type;
        }

        public string member()
        {
            if(isEmum)return TAB(tab) + name + "=" + (int)defaultVaule;

            if (valueType.IsEnum)
            {
                return TAB(tab) + name + "=" + TypeName(valueType)+"."+Enum.GetNames(valueType).GetValue(0);
            }
            else
            {
                return TAB(tab) + ((isArray || isList) ? name + "={}" : name);
            }
        }

        static bool needHint(WireType wireType)
        {
            return ((int)wireType & ~7) != 0;
        }

        string readValue(int tab, bool arraylistItem=false)
        {
            WireType wireType;
            string reader = getSerializer(datafmt, valueType, false, out wireType);
            string hint = needHint(wireType) ? string.Format("src:Hint(WireType.{0}); ", wireType) : "";
            if (arraylistItem)
            {
                Type[] tType = valueType.GetGenericArguments();//获取泛类型列表
                string elementType = tType.Length>0?TypeName(tType[0]):TypeName(valueType.GetElementType());
                return TAB(tab)+string.Format("{3}self.{1}[i] = self.{0}(\"{2}\", src);",reader, name, elementType, hint)+RET;
            }
            else
            {
                string readHeader = TAB(tab) + "tag = src:ReadFieldHeader();" + RET;
                string readValue  = TAB(tab) + string.Format("if tag=={0} then {3}self.{1} = self.{2}(self.{1}, src) end", tag, name, reader, hint) + RET;
                return readHeader + readValue;
            }
        }

        string writeValue(int tab, bool arrayListItem=false)
        {
            WireType wireType;
            string writer = getSerializer(datafmt, valueType, true, out wireType);
            string fmtstr = arrayListItem?"self.{0}(self.{1}[i], dst);":"self.{0}(self.{1}, dst);";
            string writeHeader = TAB(tab) + string.Format("ProtoWriter.WriteFieldHeader({0},WireType.{1},dst);", tag, wireType) + RET;
            string writeValue = TAB(tab)  + string.Format(fmtstr, writer, name) + RET;
            return writeHeader + writeValue;
        }

        string readArrayList(int tab)
        {
            string readTag   = TAB(tab)+"tag = src:ReadFieldHeader();" + RET;
            string ifBegin   = TAB(tab)+string.Format("if tag=={0} then", tag) + RET;
            string i1        = TAB(tab+1)+"local i = 1;" + RET;
            string loopBegin = TAB(tab+1)+"repeat" + RET;
            string value     = readValue(tab+2, true);
            string i2        = TAB(tab+2)+"i=i+1;" + RET;
            string loopEnd   = TAB(tab+1)+"until src:TryReadFieldHeader(tag)~=true" + RET;
            string ifEnd     = TAB(tab)+"end" + RET;
            return readTag + ifBegin + i1 + loopBegin + value + i2 + loopEnd + ifEnd;
        }

        string writeArrayList(int tab)
        {
            string loopBegin = TAB(tab) + string.Format("for i=1, #(self.{0}) do", name) + RET;
            string value     = writeValue(tab+1, true);
            string loopEnd   = TAB(tab) + "end" + RET;
            return loopBegin + value + loopEnd;
        }

        public string serializer()
        {
            return (isArray||isList) ? writeArrayList(this.tab+1) : writeValue(this.tab+1);
        }

        public string deserialize()
        {
            return (isArray||isList) ? readArrayList(this.tab+1) : readValue(this.tab+1);
        }
    }
    #endregion
}


public class LuaProto : MonoBehaviour
{
    // Use this for initialization
    void Start ()
    {
        ResLoad.init(this);
        gameObject.AddComponent<LuaRoot>();
    }

    [MenuItem("DevelopTools/Proto/Gen Lua")]
    public static void genLuaPB()
    {
        string path = Application.dataPath + "/StreamingAssets/Lua/PB/";
        LuaProtoGen lpb = new LuaProtoGen(path);
        //lpb.addFilter(typeof(gamedbproto.login_reply));
        //lpb.addFilter(typeof(gameproto.resp_hall_info));
        //lpb.addFilter(typeof(itemproto.MailLogItem));
        lpb.genLuaPB();
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 100, 50), "Test"))
        {
            /*gamedbproto.login_reply packet = new gamedbproto.login_reply();
            packet.seriallogin = 9527;
            packet.userid = 1000001;
            packet.today_first_login = 123456789;
            packet.first_login = 1;
            packet.otherinfo = new byte[8]{1,2,3,254,5,6,7,8};
            gameproto.byPropInfo prop = new gameproto.byPropInfo();
            prop.id = 997;
            prop.value = 998;
            packet.props.Add(prop);
            prop = new gameproto.byPropInfo();
            prop.id = 999;
            prop.value = 1000;
            packet.props.Add(prop);

            LuaObject luaPacket = testLuaProto<gamedbproto.login_reply>(packet);
            if (null == luaPacket)return;
            byte[] dt = luaPacket.value<byte[]>("otherinfo");*/
        }
    }

    static LuaObject testLuaProto<T>(T csPacket) where T : class
    {
        LuaObject luaPacket = null;
        using (MemoryStream ms = new MemoryStream()){
            //RuntimeTypeModel.debug = true;
            UnityEngine.Debug.LogError("========================CS Write");
            Serializer.Serialize<T>(ms, csPacket);
            UnityEngine.Debug.LogError("========================CS Read");
            using (MemoryStream mrs = new MemoryStream(ms.ToArray()))
            {
                Serializer.Deserialize<T>(mrs);
            }
            //RuntimeTypeModel.debug = false;
            byte[] data = ms.ToArray ();
            luaPacket = testLuaProto<T>(csPacket as T, data);
            //RuntimeTypeModel.debug = false;
        }
        return luaPacket;
    }

    static LuaObject testLuaProto<T>(T t, byte[] csdata) where T : class
    {
        if (null == t)return null;
        LuaObject luaPacket;
        byte[]    luadata;
        //读取
        UnityEngine.Debug.LogError("========================Lua Read");
        using (MemoryStream ms = new MemoryStream(csdata))
        {
            using (ProtoReader pr = new ProtoReader(ms, RuntimeTypeModel.Default, null))
            {
                luaPacket = LuaObject.newObject(t.GetType().Name);
                if (luaPacket == null)return null;
                luaPacket.call("Deserialize", pr);
            }
        }
        //写入
        UnityEngine.Debug.LogError("========================Lua Write");
        using (MemoryStream ms = new MemoryStream())
        {
            using (ProtoWriter pw = new ProtoWriter(ms, RuntimeTypeModel.Default, null))
            {
                luaPacket.call("Serializer", pw);
                pw.Close();
                luadata = ms.ToArray ();
            }
        }
        //校验
        if (csdata.Length != luadata.Length)
        {
            UnityEngine.Debug.LogError("==========data error= "+csdata.Length+","+luadata.Length);
            return null;
        }
        for (int i = 0; i < csdata.Length; ++i)
        {
            if (csdata[i] == luadata[i])continue;
            UnityEngine.Debug.LogError("==========data error");
            return null;
        }
        UnityEngine.Debug.LogError("==========data ok");
        return luaPacket;
    }
}

}

#endif