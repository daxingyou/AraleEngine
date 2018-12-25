using System.Collections;
using System;

namespace Arale.Engine
{
    
    public class NetClient 
    {
        protected int mCID;//连接ID
        // 多线程锁
        protected object mLock = new object();
        //接收队列
        protected ArrayList mRecvList = new ArrayList();
        //发送队列
        protected ArrayList mSendList = new ArrayList();
        // 是否允许立刻发送消息
        protected bool mReadySend;

        internal NetClient()
        {
        }

        /// <summary>
        /// 创建消息包数据结构，并压入等待发送的消息队列中等待发送
        /// </summary>
        /// <param name="msgID">消息包协议号</param>
        /// <param name="msgData">消息对象</param>
		public void send(int msgID, object msgData)
        {
            try
            {
                if (msgData == null)throw new Exception("msgData can not be null");
				mSendList.Add(Packet.createPacket(msgID, msgData));
            }
            catch (System.Exception ex)
            {
                Log.e(ex, Log.Tag.Net);
                EventMgr.single.SendEvent(NetworkMgr.EventSendError, this);
            }
        }

        public virtual void update()
        {
            //处理发送队列中的消息
            processSendData();
            // 处理接受队列中的消息
            processReceiveData();
        }

        protected virtual void processSendData(){}
        protected virtual void processReceiveData()
        {
            if (mRecvList.Count < 1)
                return;

            // 从缓冲队列中取出消息包，每一帧只处理一个包
            Packet packet = null;
            lock (mLock)
            {
                packet = mRecvList[0] as Packet;
                mRecvList.RemoveAt(0);
            }

            if (!packet.handle())
            {
                EventMgr.single.SendEvent(NetworkMgr.EventHandlerError, this);
            }
        }

        protected void AddPacketResult(byte[] bytes)
        {
			Packet packet = Packet.createPacket(bytes);
			if (packet == null || packet.mHandler==null)return;

            lock (mLock)
            {
                mRecvList.Add(packet);
            }

			// 接收到的数据比包的长度大，需要对数据进行拆分(http接收可能存在该情况)
            if (bytes.Length > packet.mLen + 4)
            {
                int newLength = bytes.Length - packet.mLen - 4;
                byte[] newBytes = new byte[newLength];
                System.Array.Copy(bytes, packet.mLen + 4, newBytes, 0, newLength);
                Log.d("Split a new packet. New Length = " + newLength, Log.Tag.Net);
                AddPacketResult(newBytes);
            }
        }

        public virtual void clear(){}
        public virtual void close(){}
    }

}
