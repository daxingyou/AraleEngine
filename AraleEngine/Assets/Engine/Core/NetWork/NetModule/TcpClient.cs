#define SYNC_SEND_MESSAGE

using UnityEngine;
using System;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;


namespace Arale.Engine
{
    public class ReceiveBuffer
    {
        private int _offset;
        private byte[] _buffer;

        public ReceiveBuffer(int size){
            _offset = 0;
            _buffer = new byte[size];
        }

        public int offset{
            get{ return _offset; }
            set { _offset = value; }
        }

        public int size{
            get { return buffer.Length - _offset; }
        }

        public byte[] buffer{
            get { return _buffer; }
        }
    }

    public class TcpClient : NetClient
	{
        // Socket客户端对象
		protected Socket mSocket = null;
		// 服务器地址
        string mHostName;
		string mIp;
		int    mPort;
		//连接是否正常
		protected enum State
        {
			Disconnect,
			Reset,
			Resetting,
			Ok,
		}
		protected State netState;
		
		// 构造函数
        internal TcpClient()
		{
			netState   = State.Disconnect;
		}


        #region  建立连接
		public void Connect(string ip = null, string hostname = null, int port = 0)
		{
            if (!string.IsNullOrEmpty(ip))
            {
                mIp = ip;
            }
            if (!string.IsNullOrEmpty(hostname))
            {
                mHostName = hostname;
            }
            if (0 != port)
            {
                mPort = port;
            }
			netState = State.Disconnect;

            try
            {
                if (false == string.IsNullOrEmpty(mIp))
                {                  
                    Log.d("Socket Connect With Ip Address. " + mIp + ":" + mPort, Log.Tag.Net);
                    IPEndPoint ipEndpoint = new IPEndPoint(IPAddress.Parse(mIp), mPort);
                    mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    mSocket.SendTimeout = (int)(1000 * NetworkMgr.single.mTimeout);
                    mSocket.SendBufferSize = 1 << 20;
                    mSocket.NoDelay = true;
                    mSocket.BeginConnect(ipEndpoint, new AsyncCallback(ConnectCallback), mSocket);
                }
                else if (false == string.IsNullOrEmpty(mHostName))
                {
                    IPAddress[] ips = Dns.GetHostAddresses(mHostName);
                    Log.d("Socket Connect With Hostname. " + mHostName + ":" + mPort, Log.Tag.Net);
                    mSocket = new Socket(ips.Length > 0 ? ips[0].AddressFamily : AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                    mSocket.SendTimeout = (int)(1000 * NetworkMgr.single.mTimeout);
                    mSocket.SendBufferSize = 1 << 20;
                    mSocket.NoDelay = true;
                    mSocket.BeginConnect(mHostName, mPort, new AsyncCallback(ConnectCallback), mSocket);
                }
                else
                {
                    Log.d("Socket Connect Without any ip address of hostname.", Log.Tag.Net);
                }
            }
            catch (Exception ex)
            {
                Log.e(ex, Log.Tag.Net);
                EventMgr.single.SendEvent(NetworkMgr.EventDisconnect, this);
            }
		}
		
		void ConnectCallback(IAsyncResult asyncConnect)
		{
			Socket client = (Socket)asyncConnect.AsyncState;
			try
			{
				client.EndConnect(asyncConnect);
                if(!client.Connected )throw new Exception("connect failed");
                //开始收包
                Log.d("Socket Connect End. Begin Read", Log.Tag.Net);
				ReceiveBuffer buffer = new ReceiveBuffer(Packet.SizeBytes);
                mSocket.BeginReceive(buffer.buffer, buffer.offset, buffer.size, SocketFlags.None, OnReadPacketHeader, buffer);
                //========
				mReadySend=true;
				netState=State.Ok;
                EventMgr.single.SendEvent(NetworkMgr.EventConnect, this);
			}
			catch(SocketException e)
			{
                Log.e(e, Log.Tag.Net);
				netState=State.Reset;
                EventMgr.single.SendEvent(NetworkMgr.EventDisconnect, this);
			}
            catch(ObjectDisposedException e)
            {
                Log.e(e, Log.Tag.Net);
                netState=State.Reset;
				EventMgr.single.SendEvent(NetworkMgr.EventDisconnect, this);
            }
			catch (Exception e)
			{
                Log.e(e, Log.Tag.Net);
				netState=State.Reset;
				EventMgr.single.SendEvent(NetworkMgr.EventDisconnect, this);
			}
		}
        #endregion


        #region 收包
        void OnReadPacketHeader(IAsyncResult result)
        {
            try
            {
                if( mSocket == null )return;
                ReceiveBuffer buffer = (ReceiveBuffer)result.AsyncState;
                int size = mSocket.EndReceive(result);
                if(size < 0 )return;

                if( size == 0 )
                { //remote server is shutdown
                    throw new Exception("remote server shutdown");
                }
                else if( size == buffer.size )
                { //package's header is received
					int len = Packet.ReadInt32(buffer.buffer, 0);
					if(len > Packet.MaxSize)throw new Exception("packet size error");
                    ReceiveBuffer body = new ReceiveBuffer(len);
					Packet.WriteInt32(len, body.buffer, 0);
					mSocket.BeginReceive(body.buffer, Packet.SizeBytes, body.size-Packet.SizeBytes, SocketFlags.None, OnReadPacketBody, body);
                }
                else
                {
                    buffer.offset = buffer.offset + size;
                    mSocket.BeginReceive(buffer.buffer, buffer.offset, buffer.size, SocketFlags.None, OnReadPacketHeader, buffer);
                }
            }
            catch( SocketException e)
            {
				Log.e(e.Message, Log.Tag.Net);
				EventMgr.single.SendEvent(NetworkMgr.EventRecvError, this);
            }
            catch(ObjectDisposedException e)
            {
				Log.e(e.Message, Log.Tag.Net);
				EventMgr.single.SendEvent(NetworkMgr.EventRecvError, this);
            }
            catch (Exception e)
            {
				Log.e(e.Message, Log.Tag.Net);
				EventMgr.single.SendEvent(NetworkMgr.EventRecvError, this);
            }
        }

        void OnReadPacketBody(IAsyncResult result)
        {
            try
            {
                if( mSocket == null )return;
                ReceiveBuffer buffer = (ReceiveBuffer)result.AsyncState;
                int size = mSocket.EndReceive(result);
                if(size<0)return;

                if( size == 0 )
                { //remote server is shutdown
                    throw new Exception("remote server shutdown");
                }
				else if( size == buffer.size-Packet.SizeBytes )
                { //package's body is recevied
                    //BufferReader msg = new BufferReader(buffer.buffer, 0);
                    AddPacketResult(buffer.buffer);
					ReceiveBuffer head = new ReceiveBuffer(Packet.SizeBytes);
                    mSocket.BeginReceive(head.buffer, head.offset, head.size, SocketFlags.None, OnReadPacketHeader, head);
                }
                else
                {
                    buffer.offset = buffer.offset+size; 
                    mSocket.BeginReceive(buffer.buffer, buffer.offset, buffer.size, SocketFlags.None, OnReadPacketBody, buffer);
                }
            }
            catch( SocketException)
            {
				EventMgr.single.SendEvent(NetworkMgr.EventRecvError, this);
            }
            catch(ObjectDisposedException)
            {
				EventMgr.single.SendEvent(NetworkMgr.EventRecvError, this);
            }
            catch (Exception)
            {
				EventMgr.single.SendEvent(NetworkMgr.EventRecvError, this);
            }
        }
        #endregion


        #region 发包
        protected override void ProcessSendData()
        {
            if (!mReadySend || mSendList.Count < 1)
                return;
            Packet pack = mSendList[0] as Packet;
            mSendList.RemoveAt(0);
            WritePacket(pack);
        }

        void WritePacket(Packet packet)
        {
            try
            {
                if( mSocket == null )return;
                byte[] dat = packet.mNetData;
                mSocket.BeginSend(dat, 0, dat.Length, SocketFlags.None, OnWritePacket, dat);
            }
            catch( SocketException e)
            {
				Log.e(e.Message, Log.Tag.Net);
				EventMgr.single.SendEvent(NetworkMgr.EventSendError, this);
            }
            catch(ObjectDisposedException e)
            {
				Log.e(e.Message, Log.Tag.Net);
				EventMgr.single.SendEvent(NetworkMgr.EventSendError, this);
            }
            catch (Exception e)
            {
				Log.e(e.Message, Log.Tag.Net);
				EventMgr.single.SendEvent(NetworkMgr.EventSendError, this);
            }
        }

        void OnWritePacket(IAsyncResult result)
        {
            try
            {
                if( mSocket == null )return;
                mSocket.EndSend(result);
                mReadySend=true;
            }
            catch( SocketException e)
            {
				Log.e(e.Message, Log.Tag.Net);
				EventMgr.single.SendEvent(NetworkMgr.EventSendError, this);
            }
            catch(ObjectDisposedException e)
            {
				Log.e(e.Message, Log.Tag.Net);
				EventMgr.single.SendEvent(NetworkMgr.EventSendError, this);
            }
            catch (Exception e)
            {
				Log.e(e.Message, Log.Tag.Net);
				EventMgr.single.SendEvent(NetworkMgr.EventSendError, this);
            }
        }
        #endregion

        #region
        public override void Close()
        {
            if( mSocket == null )return;
            try
            {
                lock(mSocket)
                {
                    if( mSocket.Connected )
                    {
                        mSocket.Shutdown(SocketShutdown.Both);
                        mSocket.Close();
                        netState = State.Disconnect;
                    }
                }
            }
            catch (SocketException e)
            {
				EventMgr.single.SendEvent(NetworkMgr.EventDisconnect, this);
            }
            catch( ObjectDisposedException e)
            {
				EventMgr.single.SendEvent(NetworkMgr.EventDisconnect, this);
            }
            finally
            {
				EventMgr.single.SendEvent(NetworkMgr.EventDisconnect, this);
                mSocket = null;
            }
        }
        #endregion
	}
}