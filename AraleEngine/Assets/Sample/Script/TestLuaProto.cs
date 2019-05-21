using UnityEngine;
using System.Collections;
using Arale.Engine;
using System.IO;
using ProtoBuf;
using ProtoBuf.Meta;

//勾选uselua
public class TestLuaProto : GRoot
{
    protected override void gameStart(){}
    protected override void gameExit(){}
    protected override void gameUpdate(){}

    void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 100, 50), "Test"))
        {//嵌入类，数组均支持
            Proto.test.TestProto msg = new Proto.test.TestProto();
            msg.a = "arale";
            msg.b = 1234;
            msg.c = 5678;
            msg.d.AddRange(new int[]{1,2,3});
            LuaObject luaPacket = testLuaProto<Proto.test.TestProto>(msg);
            if (null == luaPacket)return;
            //byte[] dt = luaPacket.value<byte[]>("otherinfo");
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
