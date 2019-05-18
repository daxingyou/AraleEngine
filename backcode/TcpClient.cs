using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System;
using System.Net;
using Scripts.CoreScripts.Core;
using Scripts.CoreScripts.NetWrok.Utils;

public class TcpClient
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

	enum SocketState
	{
		Wait,
		Ok,
		Close,
		Error,
	}

	enum SocketAction
	{
		Connect,
		Read,
		Send,
		Close,
	}

	enum NetState
	{
		Disconnect,
		Connecting,
		Connected,
	}

	Socket      mSocket;
	SocketState mSocketState;
	NetState    mNetState;
	int         mConnectTryTimes;
	uint        mPort = 0;
	string      mHostName;

	void handleError(bool needClose, SocketAction action, string msg)
	{
		Log.D(string.Format("{0},{1},{2}",needClose,action,msg), Log.Tag.Net);
		if (needClose)closeSocket ();
		mSocketState = SocketState.Error;
	}

	void handlePacket(byte[] data)
	{
	}

	void handleConnect(Socket s)
	{
		mSocket = s;
		mSocketState = SocketState.Ok;
		readPackageHeader();//开始收包
	}
	#region 建立连接
	void connectHost(string hostname, uint port)
	{
		try
		{
			Log.I ("connectHost begin:"+hostname, Log.Tag.Net);
			mSocketState = SocketState.Wait;
			close();
			IPAddress ip;
			if (IPAddress.TryParse (hostname, out ip))
			{//如果给的是ip地址先使用ip地址连接
				Socket s = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
				s.BeginConnect(ip, (int)port, onHostConnect, s);
			}
			else
			{//如果给的是域名直接域名连接
				Dns.BeginGetHostEntry (hostname, new AsyncCallback (onHostEntry), null);
			}
			Log.I ("connectHost end", Log.Tag.Net);
		}
		catch(System.Exception)
		{
			handleError (true, SocketAction.Connect, "Exception");
		}
	}

	void onHostConnect(IAsyncResult result)
	{
		Log.I ("onHostConnect begin", Log.Tag.Net);
		try
		{
			Socket s = (Socket)result.AsyncState;
			s.EndConnect(result);
			if (!s.Connected)throw new Exception("connect failed");
			handleConnect(s);
			return;
		}
		catch(SocketException e)
		{
		}
		catch(Exception e)
		{
		}

		//连接失败尝试dns解析
		try
		{
			Dns.BeginGetHostEntry (mHostName, new AsyncCallback (onHostEntry), null);
		}
		catch(System.Exception e)
		{
			Log.E (e,Log.Tag.Net);
			handleError (false, SocketAction.Connect, "Exception");
		}
		Log.I ("onHostConnect end", Log.Tag.Net);
	}

	void onHostEntry(IAsyncResult result)
	{
		Log.I ("onHostEntry begin", Log.Tag.Net);
		try
		{
			IPAddress ipv6 = null;
			IPAddress ipv4 = null;
			IPHostEntry IpEntry = Dns.EndGetHostEntry (result);
			if (!result.IsCompleted)throw new Exception("dns get host entry is not completed");
			for (int i = 0; i < IpEntry.AddressList.Length; i++)
			{
				IPAddress ipa = IpEntry.AddressList [i];
				if (ipa.AddressFamily == AddressFamily.InterNetwork)  
				{
					ipv4 = ipa;
				}
				else if (ipa.AddressFamily == AddressFamily.InterNetworkV6)  
				{
					ipv6 = ipa;
					break;
				}  
			}

			Socket s = null;
			if (ipv6 != null)
			{
				Log.I ("ipv6", Log.Tag.Net);
				s = new Socket (AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
				s.BeginConnect(ipv6, (int)mPort, onConnect, s);
			}
			else
			{
				IPAddress ip;
				if (IPAddress.TryParse (mHostName, out ip))
				{
					ipv4 = ip;
				}

				if (ipv4 != null)
				{
					Log.I ("ipv4", Log.Tag.Net);
					s = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
					s.BeginConnect(ipv4, (int)mPort, onConnect, s);
				}
				else
				{
					throw new Exception("dns not find host entry");
				}
			}	
		}
		catch(Exception e)
		{
			Log.E (e, Log.Tag.Net);
			handleError (false, SocketAction.Connect, "Exception");
		}
		Log.I ("onHostEntry end", Log.Tag.Net);
	}

	private void onConnect(IAsyncResult result)
	{
		try
		{
			Socket s = (Socket)result.AsyncState;
			s.EndConnect(result);
			if(!s.Connected )throw new Exception("connect failed");
			handleConnect(s);
		}
		catch(SocketException e)
		{
			handleError (false, SocketAction.Connect, "SocketException");
		}
		catch(Exception e)
		{
			handleError (false, SocketAction.Connect, "Exception");
		}
	}
	#endregion

	#region 关闭连接
	void closeSocket()
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
					mSocketState = SocketState.Close;
				}
			}
		}
		catch (SocketException e)
		{
			handleError (false, SocketAction.Close, "SocketException");
		}
		catch( ObjectDisposedException e)
		{
			handleError (false, SocketAction.Close, "ObjectDisposedException");
		}
		finally
		{
			mSocket = null;
		}
	}
	#endregion

	#region 发送
	void readPackageHeader(){
		try
		{
			if( mSocket == null )return;
			ReceiveBuffer buffer = new ReceiveBuffer(2);
			mSocket.BeginReceive(buffer.buffer, buffer.offset, buffer.size, SocketFlags.None, onReadPackageHeader, buffer);
		}
		catch( SocketException e)
		{handleError (true, SocketAction.Read, "begin SocketException");}
		catch(ObjectDisposedException e)
		{handleError (true, SocketAction.Read, "begin ObjectDisposedException");}
		catch (Exception e)
		{handleError (true, SocketAction.Read, "begin Exception");}
	}

	void onReadPackageHeader(IAsyncResult result)
	{
		int step = 0;
		try
		{
			if( mSocket == null )return;
			ReceiveBuffer buffer = (ReceiveBuffer)result.AsyncState;
			int size = mSocket.EndReceive(result);
			if(size < 0 )return;

			if( size == 0 )
			{ //remote server is shutdown
				step=1;
				throw new Exception("remote server shutdown");
			}
			else if( size == buffer.size )
			{ //package's header is received
				step=2;
				ushort len = BufferReader.readUInt16(buffer.buffer, 0);
				ReceiveBuffer body = new ReceiveBuffer(len-2);
				mSocket.BeginReceive(body.buffer, body.offset, body.size, SocketFlags.None, onReadPackageBody, body);
			}
			else
			{
				step=3;
				buffer.offset = buffer.offset + size;
				mSocket.BeginReceive(buffer.buffer, buffer.offset, buffer.size, SocketFlags.None, onReadPackageHeader, buffer);
			}
		}
		catch( SocketException)
		{handleError (true, SocketAction.Read, "head SocketException,"+step);}
		catch(ObjectDisposedException)
		{handleError (true, SocketAction.Read, "head ObjectDisposedException,"+step);}
		catch (Exception)
		{handleError (true, SocketAction.Read, "head Exception,"+step);}
	}

	void onReadPackageBody(IAsyncResult result)
	{
		int step = 0;
		try
		{
			if( mSocket == null )return;
			ReceiveBuffer buffer = (ReceiveBuffer)result.AsyncState;
			int size = mSocket.EndReceive(result);
			if(size<0)return;

			if( size == 0 )
			{ //remote server is shutdown
				step=1;
				throw new Exception("remote server shutdown");
			}
			else if( size == buffer.size )
			{ //package's body is recevied
				step=2;
				handlePacket(buffer.buffer);
				ReceiveBuffer head = new ReceiveBuffer(2);
				mSocket.BeginReceive(head.buffer, head.offset, head.size, SocketFlags.None, onReadPackageHeader, head);
			}
			else
			{
				step=3;
				buffer.offset = buffer.offset+size;	
				mSocket.BeginReceive(buffer.buffer, buffer.offset, buffer.size, SocketFlags.None, onReadPackageBody, buffer);
			}
		}
		catch( SocketException)
		{handleError (true, SocketAction.Read, "body SocketException,"+step);}
		catch(ObjectDisposedException)
		{handleError (true, SocketAction.Read, "body ObjectDisposedException"+step);}
		catch (Exception)
		{handleError (true, SocketAction.Read, "body Exception"+step);}
	}
	#endregion

	#region 接收
	void send(byte[] data)
	{
		try
		{
			if( mSocket == null )return;
			mSocket.BeginSend(data, 0, data.Length, SocketFlags.None, onSend, data);
		}
		catch( SocketException)
		{handleError (true, SocketAction.Send, "begin SocketException");}
		catch(ObjectDisposedException)
		{handleError (true, SocketAction.Send, "begin ObjectDisposedException");}
		catch (Exception)
		{handleError (true, SocketAction.Send, "begin Exception");}
	}

	void onSend(IAsyncResult result)
	{
		try
		{
			if( mSocket == null )return;
			mSocket.EndSend(result);
		}
		catch( SocketException)
		{handleError (true, SocketAction.Send, "SocketException");}
		catch(ObjectDisposedException)
		{handleError (true, SocketAction.Send, "ObjectDisposedException");}
		catch (Exception)
		{handleError (true, SocketAction.Send, "Exception");}
	}
	#endregion

	void updateNetState()
	{
		switch(mNetState)
		{
		case NetState.Disconnect://网络断开
			break;
		case NetState.Connecting://正在连接
			switch (mSocketState)
			{
			case SocketState.Wait://正在连接
				break;
			case SocketState.Ok://连接成功
				mNetState = NetState.Connected;
				handleConnect(true);
				break;
			default://连接失败
				mNetState = NetState.Disconnect;
				handleConnect(false);
				break;
			}
			break;
		case NetState.Connected://连接完成
			if (mSocketState == SocketState.Ok)break;
			mNetState = NetState.Disconnect;
			handleDisconnect ();
			break;
		}
	}

	void handleDisconnect()
	{
		close ();
		if(mOnDisconnect!=null)mOnDisconnect ();
		mOnDisconnect = null;
	}

	void handleConnect(bool isOk)
	{
		if ((!isOk) && (--mConnectTryTimes != 0))
		{
			connectHost (mHostName, mPort);
			return;
		}
		if(mOnConnect!=null)mOnConnect(isOk);
		mOnConnect = null;
	}

	Action mOnDisconnect;
	public Action onDisconnect{get{return mOnDisconnect;}}
	Action<bool> mOnConnect;
	#region 公共接口
	public bool IsOk
	{
		get{ return mNetState == NetState.Connected; }
	}

	public void init(string host, int port)
	{
		mHostName = host;
		mPort = (uint)port;
	}

	public void connect(Action<bool> callback, int tryTimes=1)
	{
		mConnectTryTimes = tryTimes;
		mOnConnect = callback;
	}

	public void close()
	{
		closeSocket ();
	}

	public void sendMessage()
	{
	}

	public void update()
	{
		updateNetState ();
	}
	#endregion

	#region 心跳
	private DateTime _localTime;
	private DateTime _localTimeTmp;
	private ulong    _serverTime;
	private bool _isContinueTimePing;
	Scripts.CoreScripts.NetWrok.Utils.Timer _pingTimer = null;
	public void StartTimePing()
	{
		_isContinueTimePing = true;
		_localTimeTmp = DateTime.Now;
		_localTime = DateTime.Now;
		//sendMessage (MSGID.REQUEST_PING);
		_pingTimer.start(8000);
	}

	public void StopTimePing()
	{
		_isContinueTimePing = false;
		_pingTimer.stop();
	}

	private void OnTimePing()
	{
		if (_isContinueTimePing)
		{
			_localTimeTmp = DateTime.Now;
			//sendMessage (MSGID.REQUEST_PING);
		}
	}

	private void setServerTime(ulong t)
	{
		ulong dt1 = 0;
		if (t > getSerTime())
		{
			dt1=t - getSerTime();
		} 
		else
		{
			dt1= getSerTime()-t;
		}

		_serverTime = t;
		ulong dt = (ulong)((DateTime.Now - _localTimeTmp).TotalMilliseconds / 2);

		_serverTime = dt + _serverTime;
		_localTime = DateTime.Now;
	}

	public ulong getSerTime()
	{
		return _serverTime + (ulong)(DateTime.Now - _localTime).TotalMilliseconds;
	}

	private void onSysPingTimeOut()
	{
		/*ICollection ic = _gamePackets;
		lock(ic.SyncRoot)
		{
			_gamePackets.Clear ();
			if (_gameSocket != null)
			{
				_gameSocket.close ();
			}
			_gameSocket = null;
		}*/
	}
	#endregion
}
