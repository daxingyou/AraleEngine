using UnityEngine;
using System.Collections;
using Arale.Engine;
using System.Net.Sockets;
using System.Net;
using System;
using System.Runtime.InteropServices;

public class TestNetwork : GRoot
{
    Socket mServer;
    protected override void gameStart()
    {
        EventMgr.single.AddListener(NetworkMgr.EventConnect, onEventConnect);
        initServer();
        NetworkMgr.single.Init ();
    }

    protected override void gameExit()
    {
        NetworkMgr.single.Deinit ();
        EventMgr.single.UnAddListener(NetworkMgr.EventConnect, onEventConnect);
    }

    protected override void gameUpdate()
    {
        
    }

    void OnGUI()
    {
        if (GUI.Button (new Rect (0, 0, 100, 30), "连接服务器"))
        {
            Debug.LogError("connect");
            Arale.Engine.TcpClient client = NetworkMgr.single.createClient (ClientType.Tcp, "") as Arale.Engine.TcpClient;
            client.connect ("127.0.0.1", null, 9527);
        }
    }

    void initServer()
    {
        mServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Debug.LogError(getMemory(mServer));
        mServer.Bind (new IPEndPoint (IPAddress.Parse("127.0.0.1"), 9527));
        mServer.Listen (5);
        SocketAsyncEventArgs e = new SocketAsyncEventArgs ();
        e.Completed += new System.EventHandler<SocketAsyncEventArgs> (onIOComplete);
        if (!mServer.AcceptAsync (e))
        {//如果异步请求已完成则返回false，并且不会触发异步事件回调
            accept (e);
        }
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
        Debug.LogError ("accept");
        Socket s = e.AcceptSocket;
        Debug.LogError(getMemory(s));
        SocketAsyncEventArgs re = new SocketAsyncEventArgs ();
        re.UserToken = s;
        re.SetBuffer (new byte[1024], 0, 1024);//不设置直接崩溃
        re.Completed += new System.EventHandler<SocketAsyncEventArgs> (onIOComplete);
        if (!s.ReceiveAsync (re)) {
            receive (re);
        }
    }

    void receive(SocketAsyncEventArgs e)
    {
        Socket s = (Socket)e.UserToken;
        if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
        {
            e.SetBuffer (e.Offset, e.BytesTransferred);
            if(!s.SendAsync(e)){
                send(e);
            }
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
            if (!s.ReceiveAsync (e)) {
                receive (e);
            }
        }
        else
        {
            close (e);
        }
    }

    void close(SocketAsyncEventArgs e)
    {
        Debug.LogError("close");
        Socket s = (Socket)e.UserToken;
        Debug.LogError(getMemory(s));
        try
        {
            s.Shutdown (SocketShutdown.Receive);
        }
        catch(Exception ex)
        {
            Debug.LogError(ex);
        }
        s.Close ();
    }

    void onEventConnect(EventMgr.EventData ed)
    {
    }

    public static string getMemory(object o)
    {
        GCHandle h = GCHandle.Alloc(0, GCHandleType.Pinned);
        IntPtr addr = h.AddrOfPinnedObject();
        return "0x" + addr.ToString("X");
    }
}
