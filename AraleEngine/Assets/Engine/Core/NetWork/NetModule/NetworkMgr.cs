#define SOCKET_TEST

using UnityEngine;
using System.Net;
using System.Collections;
using System.Collections.Generic;

namespace Arale.Engine
{
	public enum ErrorType
	{
		None          =0,
		TimeOut       =-1,
		RecvException =-2,
		SendException =-3,
		TCPServerError=-4,
		TCPDeviceError=-5,
		TCPVerify     =-6,
	}

    public enum ClientType
    {
        Http,
        Tcp,
    }

    public class NetworkMgr : MgrBase<NetworkMgr>
    {
        #region 引擎消息
        public const string EventWaiting     = "Net.Waiting";
        public const string EventDisconnect  = "Net.Disconnect";
        public const string EventConnect     = "Net.Connect";
        public const string EventSendError   = "Net.SendError";
        public const string EventRecvError   = "Net.RecvError";
        public const string EventHandlerError= "Net.HandlerError";
        #endregion
        
        public bool mSendMsgEnabled = true; //消息是否可以继续发送
        public bool mRecvMsgEnabled = true; //接收消息的开关
		public bool mForeground     = true; //应用处于前台
        public float mTimeout = 10.0f;      //连接超时时间

        #region 消息ID-Handler映射
        IDictionary<int, PacketHandler> mPacketHandlers = new Dictionary<int, PacketHandler>();
        public PacketHandler GetHandler(int msgID)
        {
            PacketHandler handler;
			if (!mPacketHandlers.TryGetValue(msgID, out handler))
            {
                handler = new PacketHandler();
				mPacketHandlers[msgID] = handler;
            }
            return handler;
        }
        #endregion

		public NetClient GetLink(int idx)
		{
			return mClients [idx];
		}

        // 游戏网络连接
        NetClient[] mClients = new NetClient[4];
        public override void Update()
        {
            for(int i=0;i<mClients.Length;i++)
            {
                NetClient c = mClients[i];
                if (c == null)continue;
                c.Update();
            }
        }

		public override void Deinit ()
		{
			for(int i=0;i<mClients.Length;i++)
			{
				NetClient c = mClients[i];
				if (c == null)continue;
				c.Close ();
				mClients[i] = null;
			}
		}

        public NetClient CreateClient(ClientType ct, string url)
        {
            NetClient c = null;
            for(int i=0;i<mClients.Length;++i)
            {
                c = mClients[i];
                if (c != null)continue;
                switch(ct)
                {
				case ClientType.Http:
					mClients[i] = c = new HttpClient (url);
					return c;
				case ClientType.Tcp:
					mClients[i] = c = new TcpClient ();
					return c;
                default:
                	return null;
                } 
            }
            return null;
        }

        // 重置网络，清空缓存中的网络消息
        public void ResetNetwork()
        {
            for (int i = 0; i < mClients.Length; ++i)
            {
                NetClient c = mClients[i];
                if (c == null)continue;
                c.Clear();
                c.Close();
                mClients[i] = null;
            }
        }
    }
}



