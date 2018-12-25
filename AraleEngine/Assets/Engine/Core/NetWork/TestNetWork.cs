#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using Arale.Engine;
using System.Net.Sockets;
using System.Net;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Proto.Message;

public class TestNetWork : GRoot {

	// Use this for initialization
	Socket mServer;
	Dictionary<IntPtr, SocketAsyncEventArgs> mClients = new Dictionary<IntPtr, SocketAsyncEventArgs>();
	//SocketAsyncEventArgs mEvent;
	protected override void gameStart(){
		gameObject.AddComponent<LuaRoot> ();
		Log.mFilter = (int)Log.Tag.Net;
		Log.mDebugLevel = 3;
		NetworkMgr.single.Init ();
		NetworkMgr.single.getHandler (1).regHandler (onPacketCallback);
		EventMgr.single.AddListener (NetworkMgr.EventSendError, onSendError);
		EventMgr.single.AddListener (NetworkMgr.EventRecvError, onReciveError);
		EventMgr.single.AddListener (NetworkMgr.EventConnect, onNetConnect);
		EventMgr.single.AddListener (NetworkMgr.EventDisconnect, onNetDisconnet);
		mServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		mServer.Bind (new IPEndPoint (IPAddress.Parse("127.0.0.1"), 9527));
		mServer.Listen (5);
		doAccept ();
		GameObject go = new GameObject ("LuaProtoTest");
        go.AddComponent<LuaMono> ().bindLua("LTestNetWork");
	}

	protected override void gameUpdate()
	{
		NetworkMgr.single.Update ();
	}

	protected override void gameExit(){
		NetworkMgr.single.getHandler (1).unregHandler (onPacketCallback);
		EventMgr.single.AddListener (NetworkMgr.EventSendError, onSendError);
		EventMgr.single.UnAddListener (NetworkMgr.EventRecvError, onReciveError);
		EventMgr.single.UnAddListener (NetworkMgr.EventConnect, onNetConnect);
		EventMgr.single.UnAddListener (NetworkMgr.EventDisconnect, onNetDisconnet);
		NetworkMgr.single.Deinit ();
	}

	#region 服务器逻辑
	void doAccept()
	{
		SocketAsyncEventArgs e = new SocketAsyncEventArgs ();
		e.Completed += new System.EventHandler<SocketAsyncEventArgs> (onIOComplete);
		//如果异步请求已完成则返回false，并且不会触发异步事件回调
		if (!mServer.AcceptAsync (e))accept (e);
	}

	void onIOComplete(object sender, SocketAsyncEventArgs e)
	{
		switch (e.LastOperation)
		{
		case SocketAsyncOperation.Accept:
			accept (e);
			break;
		case SocketAsyncOperation.Receive:
			receive (e);
			break;
		case SocketAsyncOperation.Send:
			send (e);
			break;
		default:
			break;
		}
	}
		
	void accept(SocketAsyncEventArgs e)
	{
		if (e.SocketError !=SocketError.Success)
			return;
		Socket s = e.AcceptSocket;
		Debug.LogError ("accept=" + s.Handle+","+s.RemoteEndPoint.ToString());
		mClients [s.Handle] = e;//必须，否则因为e没被引用而释放导致连接自动关闭
		doAccept();


		SocketAsyncEventArgs re = new SocketAsyncEventArgs ();
		re.UserToken = s;
		re.SetBuffer (new byte[1024], 0, 1024);//不设置直接崩溃
		re.Completed += new System.EventHandler<SocketAsyncEventArgs> (onIOComplete);
		if (!s.ReceiveAsync (re))receive (re);
	}

	void receive(SocketAsyncEventArgs e)
	{
		Socket s = (Socket)e.UserToken;
		if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
		{//直接把接收数据返还给客户端
			e.SetBuffer (e.Offset, e.BytesTransferred);
			if(!s.SendAsync(e))send(e);
		}
		else
		{
			close (e);
		}
	}

	void send(SocketAsyncEventArgs e)
	{
		Socket s = (Socket)e.UserToken;
		if (e.SocketError == SocketError.Success)
		{
			if (!s.ReceiveAsync (e))receive (e);
		}
		else
		{
			close (e);
		}
	}

	void close(SocketAsyncEventArgs e)
	{
		Socket s = (Socket)e.UserToken;
		try
		{
			s.Shutdown (SocketShutdown.Both);
		}
		catch(Exception ex)
		{
			Debug.LogError(ex);
		}
		s.Close ();
	}
	#endregion


	#region 客户端逻辑 
	Arale.Engine.TcpClient mClient;
	void OnGUI()
	{
		if (GUI.Button (new Rect (0, 0, 100, 30), "建立连接"))
		{
			mClient = NetworkMgr.single.createClient (ClientType.Tcp, "") as Arale.Engine.TcpClient;
			mClient.connect ("127.0.0.1", null, 9527);
		}
		if (GUI.Button (new Rect (0, 30, 100, 30), "断开连接"))
		{
			mClient.close ();
		}
		if (GUI.Button (new Rect (0, 60, 100, 30), "发送数据"))
		{
			TestAdd msg = new TestAdd ();
			msg.n1 = 1234;
			msg.n2 = 5678;
			mClient.send(1, msg);
		}
		if (GUI.Button (new Rect (0, 90, 100, 30), "数据包编解码"))
		{
			TestAdd msg = new TestAdd ();
			msg.n1 = 1234;
			msg.n2 = 5678;
			Packet pk = Packet.createPacket (1, msg);
			pk = Packet.createPacket (pk.mNetData);
			msg = pk.toOBJ(typeof(TestAdd)) as TestAdd;
			Debug.LogError ("msg:"+msg.n1+","+msg.n2);
		}
	}

	void onPacketCallback(Packet packet)
	{
		TestAdd msg = packet.toOBJ(typeof(TestAdd)) as TestAdd;
		Debug.LogError ("msg:"+msg.n1+","+msg.n2);
	}

	void onSendError(Arale.Engine.EventMgr.EventData ed)
	{
		Debug.LogError ("onSendError");
		NetClient nc = ed.data as NetClient;
		nc.close ();
	}

	void onReciveError(Arale.Engine.EventMgr.EventData ed)
	{
		Debug.LogError ("onReciveError");
		NetClient nc = ed.data as NetClient;
		nc.close ();
	}

	void onNetConnect(Arale.Engine.EventMgr.EventData ed)
	{
		Debug.LogError ("onNetConnect");
	}

	void onNetDisconnet(Arale.Engine.EventMgr.EventData ed)
	{
		Debug.LogError ("onNetDisconnet");
	}
	#endregion
}
#endif