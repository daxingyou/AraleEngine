using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using ProtoBuf;
using ProtoBuf.Meta;

namespace Arale.Engine
{
    public class PacketHandler
    {
        public delegate void OnPacketCallback(Packet packet); 
        public bool discardable;//是否可丢弃
        public bool lastone;    //是否只处理最后收到的包
        OnPacketCallback mOnPacketCallback;
        public void RegHandler(OnPacketCallback callback)
        {
            mOnPacketCallback += callback;
        }
        public void UnregHandler(OnPacketCallback callback)
        {//即使没注册再调用也不会有问题
            mOnPacketCallback -= callback;
        }

        public void Handle(Packet packet)
        {
            mOnPacketCallback(packet);
        }
    }
    
    public class Packet
    {
		Packet(){}
		#region 包头信息
		public const int HeaderSize = 20;
		public const int MaxSize    = 512 * 1024 * 1024;
		public const int SizeBytes  = 4;
        // 整包长度
		public int mLen {
			protected set{WriteInt32 (value, mNetData, 0);}
			get{return ReadInt32 (mNetData, 0);}
		}

		// 消息包ID
		public int mID {
			protected set{WriteInt32 (value, mNetData, 4);}
			get{return ReadInt32 (mNetData, 4);}
		}

		// 时间戳
		public long mStamp {
			protected set{WriteInt64 (value, mNetData, 8);}
			get{return ReadInt64 (mNetData, 8);}
		}

		// 标志位
		public int mFlag {
			protected set{WriteInt32 (value, mNetData, 16);}
			get{return ReadInt32 (mNetData, 16);}
		}
		#endregion
        
		//元包和元数据
		public object mMetaPacket;
        //网络包数据(包含PacketHeader)
		public byte[] mNetData;

		//该包对应的处理器
		public PacketHandler mHandler;
		public virtual object ToOBJ(Type t)
		{
			if (mMetaPacket != null)return mMetaPacket;
			using (MemoryStream ms = new MemoryStream(mNetData, HeaderSize, mNetData.Length - HeaderSize))
			{
				mMetaPacket = Serializer.Deserialize(ms, t);
			}
			return mMetaPacket;
		}

		public virtual object ToLua(string name)
		{
			if (mMetaPacket != null)return mMetaPacket;
			using (MemoryStream ms = new MemoryStream(mNetData, HeaderSize, mNetData.Length - HeaderSize))
			{
				ProtoReader pr = new ProtoReader (ms, RuntimeTypeModel.Default, null);
                LuaObject lp = LuaObject.newObject(name);
				lp.call ("Deserialize", pr);
				mMetaPacket = lp;
			}
			return mMetaPacket;
		}

        //将proto数据包(原始包msgData)封包,encrypt表示是否加密数据包
		protected virtual void EncodeMeta(MemoryStream ms)
        {
			LuaObject luaPacket = mMetaPacket as LuaObject;
			if (luaPacket == null)
			{
				Serializer.Serialize (ms, mMetaPacket);
			}
			else
			{
				ProtoWriter pw = new ProtoWriter (ms, RuntimeTypeModel.Default, null);
				luaPacket.call("Serializer", pw);
				pw.Close();
			}
        }

        //处理数据包
        public bool Handle()
        {
            try
            {
                Log.d("Execute Packet ID = " + mID + " Length = " + mLen, Log.Tag.Net);
                mHandler.Handle(this);
				return true;
            }
            catch (System.Exception e)
            {
                Log.e(e, Log.Tag.Net);
                return mHandler.discardable?true:false;
            }
        }
			

		public static Packet CreatePacket(int msgID, object msgData, bool encrypt = false)
		{
			Packet pk = new Packet ();
			pk.mMetaPacket = msgData;
			using (MemoryStream ms = new MemoryStream ())
			{
				ms.Seek (HeaderSize, SeekOrigin.Begin);
				pk.EncodeMeta (ms);
				pk.mNetData = ms.ToArray();
				pk.mLen  = pk.mNetData.Length;
				pk.mID   = msgID;
				pk.mStamp= 0;
				pk.mFlag = 0;
			}
			return pk;
		}

		public static Packet CreatePacket(byte[] data, bool encrypt = false)
		{
			Packet pk = new Packet ();
			pk.mNetData = data;
			pk.mHandler = NetworkMgr.single.GetHandler(pk.mID);
			return pk;
		}

		#region 简单数据网络读写
		//网络字节序为大端字节序(高位字节存低地址)
		public static int ReadInt32(byte[] buff, int offset)
		{
			return (int)(buff [offset] << 3) | (int)(buff [offset+1] << 2) | (int)(buff [offset+2] << 1) | (int)(buff [offset+3]);
		}

		public static void WriteInt32(int val, byte[] buff, int offset)
		{
			buff [offset]   = (byte)(val>>3);
			buff [offset+1] = (byte)(val>>2);
			buff [offset+2] = (byte)(val>>1);
			buff [offset+3] = (byte)(val);
		}

		public static long ReadInt64(byte[] buff, int offset)
		{
			return (long)(buff [offset] << 7) | (long)(buff [offset+1] << 6) | (long)(buff [offset+2] << 5) | (long)(buff [offset+3] << 4) | (long)(buff [offset+4] << 3) | (long)(buff [offset+5] << 2) | (long)(buff [offset+6] << 1) | (long)(buff [offset+7]);
		}

		public static void WriteInt64(long val, byte[] buff, int offset)
		{
			buff [offset]   = (byte)(val>>7);
			buff [offset+1] = (byte)(val>>6);
			buff [offset+2] = (byte)(val>>5);
			buff [offset+3] = (byte)(val>>4);
			buff [offset+4] = (byte)(val>>3);
			buff [offset+5] = (byte)(val>>2);
			buff [offset+6] = (byte)(val>>1);
			buff [offset+7] = (byte)(val);
		}
		#endregion
    }
}

