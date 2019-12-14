#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using Arale.Engine;
using System.Net.Sockets;
using System.Net;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Proto.test;

public class TestNetWork : GRoot {

	// Use this for initialization
	Socket mServer;
	Dictionary<IntPtr, SocketAsyncEventArgs> mClients = new Dictionary<IntPtr, SocketAsyncEventArgs>();
	//SocketAsyncEventArgs mEvent;
	protected override void GameStart(){
		Log.mFilter = (int)Log.Tag.Net;
		Log.mDebugLevel = 3;
		NetworkMgr.single.Init ();
		NetworkMgr.single.GetHandler (1).RegHandler (onPacketCallback);
		EventMgr.single.AddListener (NetworkMgr.EventSendError, onSendError);
		EventMgr.single.AddListener (NetworkMgr.EventRecvError, onReciveError);
		EventMgr.single.AddListener (NetworkMgr.EventConnect, onNetConnect);
		EventMgr.single.AddListener (NetworkMgr.EventDisconnect, onNetDisconnet);
		mServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		mServer.Bind (new IPEndPoint (IPAddress.Parse("127.0.0.1"), 9527));
		mServer.Listen (5);
		doAccept ();
	}

	protected override void GameUpdate()
	{
		NetworkMgr.single.Update ();
	}

	protected override void GameExit(){
		NetworkMgr.single.GetHandler (1).UnregHandler (onPacketCallback);
		EventMgr.single.AddListener (NetworkMgr.EventSendError, onSendError);
		EventMgr.single.RemoveListener (NetworkMgr.EventRecvError, onReciveError);
		EventMgr.single.RemoveListener (NetworkMgr.EventConnect, onNetConnect);
		EventMgr.single.RemoveListener (NetworkMgr.EventDisconnect, onNetDisconnet);
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
			mClient = NetworkMgr.single.CreateClient (ClientType.Tcp, "") as Arale.Engine.TcpClient;
			mClient.Connect ("127.0.0.1", null, 9527);
		}
		if (GUI.Button (new Rect (0, 30, 100, 30), "断开连接"))
		{
			mClient.Close ();
		}
		if (GUI.Button (new Rect (0, 60, 100, 30), "发送数据"))
		{
            TestProto msg = new TestProto ();
            msg.a = "arale";
            msg.b = 1234;
            msg.c = 5678;
            msg.d.AddRange(new int[]{1,2,3});
			mClient.send(1, msg);
		}
		if (GUI.Button (new Rect (0, 90, 100, 30), "LuaProto编解码测试"))
		{//根据proto源码实现
            TestProto msg = new TestProto ();
            msg.a = "arale";
            msg.b = 1234;
            msg.c = 5678;
            msg.d.AddRange(new int[]{1,2,3});
			Packet pk = Packet.CreatePacket (1, msg);
			pk = Packet.CreatePacket (pk.mNetData);
            msg = pk.ToOBJ(typeof(TestProto)) as TestProto;
            Debug.LogError ("msg:"+msg.a+","+msg.b+","+msg.c+","+GHelper.toString<int>(msg.d.ToArray()));
		}
        if (GUI.Button(new Rect(0, 120, 100, 30), "LuaPB编解码测试"))
        {//网上通用方式
            LuaRoot.single.mL.DoString(@"
                local rapidjson = require 'rapidjson' 
                local t = rapidjson.decode('{""a"":123}')
                print(t.a)
                t.a = 456
                local s = rapidjson.encode(t)
                print('json', s)
                ------------------------------------
                local lpeg = require 'lpeg'
                print(lpeg.match(lpeg.R '09','123'))
                ------------------------------------
                local pb = require 'pb'
                local protoc = require 'common/protoc'

                assert(protoc:load [[
                message Phone {
                    optional string name        = 1;
                    optional int64  phonenumber = 2;
                }
                message Person {
                    optional string name     = 1;
                    optional int32  age      = 2;
                    optional string address  = 3;
                    repeated Phone  contacts = 4;
                } ]])

                local data = {
                name = 'ilse',
                age  = 18,
                    contacts = {
                        { name = 'alice', phonenumber = 12312341234 },
                        { name = 'bob',   phonenumber = 45645674567 }
                    }
                }

                local bytes = assert(pb.encode('Person', data))
                print(pb.tohex(bytes))
                local data2 = assert(pb.decode('Person', bytes))
                print(data2.name)
                print(data2.age)
                print(data2.address)
                print(data2.contacts[1].name)
                print(data2.contacts[1].phonenumber)
                print(data2.contacts[2].name)
                print(data2.contacts[2].phonenumber)
            ");
        }
	}

	void onPacketCallback(Packet packet)
	{
        TestProto msg = packet.ToOBJ(typeof(TestProto)) as TestProto;
        Debug.LogError ("cs msg:"+msg.a+","+msg.b+","+msg.c+","+GHelper.toString<int>(msg.d.ToArray()));
        packet.mMetaPacket = null;//清除缓存
        LuaObject lo = packet.ToLua("TestProto") as LuaObject;
        Debug.LogError("lua msg:" + lo.value<string>("a") + "," + lo.value<int>("b") + "," + lo.value<int>("c")+","+GHelper.toString<int>(lo.value<int[]>("d")));
	}

	void onSendError(Arale.Engine.EventMgr.EventData ed)
	{
		Debug.LogError ("onSendError");
		NetClient nc = ed.data as NetClient;
		nc.Close ();
	}

	void onReciveError(Arale.Engine.EventMgr.EventData ed)
	{
		Debug.LogError ("onReciveError");
		NetClient nc = ed.data as NetClient;
		nc.Close ();
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