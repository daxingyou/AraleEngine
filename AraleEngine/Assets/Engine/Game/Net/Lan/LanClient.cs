using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using Arale.Engine;
using System.Text;


public class LanClient : NetworkDiscovery//局域网发现,两端的端口设置要一致
{
    //==========
    public float netDelay{set;get;}
    NetworkConnection mConn;
    NetworkClient  mClient;//客户端
	Unit.Mgr       mUnitMgr;
	public Unit.Mgr unitMgr{get{return mUnitMgr;}}
    //==========
    void Awake ()
    {
        this.showGUI = false;
        this.broadcastPort = 4777;
        InvokeRepeating("checkHost", 5f, 5f);
        DontDestroyOnLoad(gameObject);
        Initialize ();
        StartAsClient ();

		mUnitMgr= new Unit.Mgr(false);
        mClient = new NetworkClient ();
        mClient.RegisterHandler (MsgType.Connect, onConnected);
        mClient.RegisterHandler (MsgType.Disconnect, onDisConnect);
        mClient.RegisterHandler ((short)MyMsgId.Time,  onTime);
        mClient.RegisterHandler ((short)MyMsgId.Login, onLogin);
        mClient.RegisterHandler ((short)MyMsgId.Create,onUnitCreate);
		mClient.RegisterHandler ((short)MyMsgId.CreateBullet, onCreateBullet);
		mClient.RegisterHandler ((short)MyMsgId.ReqPick, OnReqPick);

		//unit message begin
		mClient.RegisterHandler ((short)MyMsgId.State, onUnitMsg);
		mClient.RegisterHandler ((short)MyMsgId.Anim,  onUnitMsg);
		mClient.RegisterHandler ((short)MyMsgId.Attr,  onUnitMsg);
		mClient.RegisterHandler ((short)MyMsgId.Nav,   onUnitMsg);
		mClient.RegisterHandler ((short)MyMsgId.Move,  onUnitMsg);
		mClient.RegisterHandler ((short)MyMsgId.Buff,  onUnitMsg);
		mClient.RegisterHandler ((short)MyMsgId.Event, onUnitMsg);
		mClient.RegisterHandler ((short)MyMsgId.Skill, onUnitMsg);
		//unit message end
    }

    void OnDestroy()
    {
		mUnitMgr.dispose ();
        mClient.Shutdown();
    }

    void LateUpdate()//重写Update导致OnReceivedBroadcast无法调用
    {
        mUnitMgr.update();
    }

    public void connet(string ip, short port)
    {
        mClient.Connect(ip, port);
        Log.i("LanClient connet", Log.Tag.Net);
    }

    public void sendMsg(short msgid, MessageBase msg)
    {
        if (mConn == null)return;
        mConn.Send (msgid, msg);
		Log.i("LanClient send msg id="+(MyMsgId)msgid, Log.Tag.Net);
    }

    public void startPing()
    {
        StartCoroutine("ping"); 
    }

    public void stopPing()
    {
        StopCoroutine("ping");
    }

    IEnumerator ping()
    {
        WaitForSeconds w = new WaitForSeconds(1f);
        while (true)
        {
            Ping p = new Ping(mClient.connection.address);
            while (!p.isDone)
            {  
                yield return null;
            }
            netDelay = 0.001f*p.time;
            p.DestroyPing();
            yield return w;
        }
    }
 
    #region 监听局域网内的host
    public class HostInfo
    {
        public string ip;
        public short  port;
        public string name;
        public float  time;
    }
    List<HostInfo> mHosts = new List<HostInfo>();
    public override void OnReceivedBroadcast(string fromAddress, string data)
    {
        base.OnReceivedBroadcast (fromAddress, data);
        foreach(string key in broadcastsReceived.Keys)
        {
            NetworkBroadcastResult r = broadcastsReceived [key];
            string s = new UnicodeEncoding().GetString(r.broadcastData, 0, r.broadcastData.Length);
            Log.d(s, Log.Tag.Net);

            string ip = s.Substring(0,s.IndexOf(':'));
            HostInfo hi = mHosts.Find(delegate (HostInfo a)
                {
                    return a.ip == ip;
                });
            if (hi == null)
            {
                string[] ss = s.Split(':');
                hi = new HostInfo();
                hi.ip = ss[0];
                short.TryParse(ss[1], out hi.port);
                hi.name = ss[2];
                mHosts.Add(hi);
                EventMgr.single.PostEvent("Game.AddHost", hi);
            }
            hi.time = Time.realtimeSinceStartup;
        }
    }

    void checkHost()
    {
        float t = Time.realtimeSinceStartup;
        for (int i = mHosts.Count - 1; i >= 0; --i)
        {
            HostInfo hi = mHosts[i];
            if (t - hi.time > 10)
            {
                mHosts.RemoveAt(i);
                EventMgr.single.PostEvent("Game.DelHost", hi);
            }
        }
    }
    #endregion


    #region 消息处理
    void onConnected(NetworkMessage msg)
    {
        mConn = msg.conn;
        mConn.SetMaxDelay(0.01f);
        startPing();

        //请求同步时间
        MsgTime m = new MsgTime();
        m.clientUtcNs = System.DateTime.UtcNow.Ticks;
        sendMsg((short)MyMsgId.Time, m);
    }
    
    void onDisConnect(NetworkMessage msg)
    {
        stopPing();
    }

    void onTime(NetworkMessage msg)
    {
        Log.i("LanClient onTime", Log.Tag.Net);
        MsgTime m = msg.ReadMessage<MsgTime> ();
        long delay = (System.DateTime.UtcNow.Ticks - m.clientUtcNs) / 2;
        RTime.R.syncServerTime(m.serverUtcNs + delay, m.serverLocalNs + delay, false);
    }

    void onLogin(NetworkMessage msg)
    {
        Log.i("LanClient onLogin", Log.Tag.Net);
        SCLogin m  = msg.ReadMessage<SCLogin> ();
        unitMgr.accountId = m.accountId;
        EventMgr.single.SendEvent("Game.Login");
    }

    void onUnitCreate(NetworkMessage msg)
    {
        Log.i("LanClient onUnitCreate", Log.Tag.Net);
        MsgCreate m  = msg.ReadMessage<MsgCreate> ();
        switch (m.unitType)
        {
		case UnitType.Player:
            createPlayer(m);
            break;
		case UnitType.Monster:
            createMonster(m);
            break;
		case UnitType.Drop:
			createDropItems(m);
			break;
        }

    }

    void createPlayer(MsgCreate m)
    {
		Player u = mUnitMgr.getUnit(m.guid, UnitType.Player, m.tid) as Player;
		u.setParam(m.pos,m.dir);
        u.agentId = m.agentId;
		if(u.isAgent)EventMgr.single.SendEvent("Game.Player", u);
    }

    void createMonster(MsgCreate m)
    {
		Monster u = mUnitMgr.getUnit(m.guid, UnitType.Monster, m.tid) as Monster;
		u.setParam(m.pos,m.dir);
        u.agentId = m.agentId;
    }

	void createBullet(MsgCreateBullet m)
	{
		Bullet u = mUnitMgr.getUnit (m.guid, UnitType.Bullet, m.tid) as Bullet;
		u.setParam (m.pos, m.dir);
		u.play (m.vTarget,m.uTarget);
	}

	void createDropItems(MsgCreate m)
	{
		DropItems u = mUnitMgr.getUnit(m.guid, UnitType.Drop, m.tid) as DropItems;
		u.setParam(m.pos, m.dir);
	}

	void onUnitMsg(NetworkMessage msg)
	{
		Log.i("LanClient onUnitMsg:"+msg.msgType, Log.Tag.Net);
		MsgUnit m = msg.ReadMessage<MsgUnit> ();
		msg.reader.SeekZero ();
		Unit u = mUnitMgr.getUnit (m.guid);
		if (u == null)
		{
			reqUnit (m.guid);
			return;
		}
		u.onSync (msg);
	}

	void onCreateBullet(NetworkMessage msg)
	{
		Log.i("LanClient onCreateBullet", Log.Tag.Net);
		MsgCreateBullet m = msg.ReadMessage<MsgCreateBullet>();
		createBullet (m);
	}

    void reqUnit(uint guid)
    {
        MsgReqUnit msg = new MsgReqUnit();
        msg.guid  = guid;
        mConn.Send ((short)MyMsgId.ReqUnit, msg);
    }

	void OnReqPick(NetworkMessage msg)
	{
		Log.i("LanHost OnReqPick", Log.Tag.Net);
		MsgPick m = msg.ReadMessage<MsgPick> ();
		DropItems u = mUnitMgr.getUnit (m.dropGuid) as DropItems;
		u.decState (UnitState.Alive | UnitState.Exist);
		WindowMgr.SendWindowMessage ("MainWindow", "ShowDrop", u.tid);
	}
    #endregion
}

