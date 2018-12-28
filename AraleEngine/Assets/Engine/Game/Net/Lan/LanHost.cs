using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using Arale.Engine;


public class LanHost : NetworkDiscovery//局域网发现,两端的端口设置要一致
{
    //==========
    public string  gameName{set;get;}
    NetworkServer  mServer;//服务端
	Unit.Mgr       mUnitMgr;
	public Unit.Mgr unitMgr{get{return mUnitMgr;}}
    Dictionary<int,Client> mClients = new Dictionary<int,Client> ();
    //==========
    void Awake ()
    {
        this.showGUI = false;
        this.broadcastPort = 4777;
        DontDestroyOnLoad(gameObject);
        Initialize();
        
		mUnitMgr = new Unit.Mgr(true);
        NetworkServer.Listen (5003);
        NetworkServer.RegisterHandler (MsgType.Connect, onConnected);
        NetworkServer.RegisterHandler (MsgType.Disconnect, onDisconnected);
        NetworkServer.RegisterHandler ((short)MyMsgId.Time, onTime);
		NetworkServer.RegisterHandler ((short)MyMsgId.ReqUnit, onReqUnit);
        NetworkServer.RegisterHandler ((short)MyMsgId.ReqEnterBattle, OnReqEnterBattle);
		NetworkServer.RegisterHandler ((short)MyMsgId.ReqCreateHero, OnReqCreateHero);

		//unit message begin
		NetworkServer.RegisterHandler ((short)MyMsgId.State,    onUnitMsg);
		NetworkServer.RegisterHandler ((short)MyMsgId.Anim,     onUnitMsg);
		NetworkServer.RegisterHandler ((short)MyMsgId.Nav,      onUnitMsg);
		NetworkServer.RegisterHandler ((short)MyMsgId.Move,     onUnitMsg);
		NetworkServer.RegisterHandler ((short)MyMsgId.Skill,    onUnitMsg);
		//unit message end
    }

    void OnDestroy()
    {
        this.StopBroadcast();
        NetworkServer.Shutdown();
    }

    void Update()
    {
        mUnitMgr.update();
    }

    public void startHost()
    {
        if(running)StopBroadcast();
        this.broadcastData = string.Format("{0}:{1}:{2}", Network.player.ipAddress, 5003, gameName);
        StartAsServer();
    }

    public void sendToAll(short msgid, MessageBase msg)
    {
        NetworkServer.SendToAll(msgid, msg);
		Log.i("LanHost sendToAll msg id="+(MyMsgId)msgid, Log.Tag.Net);
    }

    public void sendTo(int connectionId, short msgid, MessageBase msg)
    {
        NetworkServer.SendToClient(connectionId, msgid, msg);
		Log.i("LanHost sendTo "+connectionId+" msg id="+(MyMsgId)msgid, Log.Tag.Net);
    }

	Client getFreerClient()
    {
        Dictionary<int,Client>.ValueCollection vs = mClients.Values;
        foreach (Client c in vs)
        {
            return c;
        }
        return null;
    }

	Client getClient(NetworkConnection con, uint accountId = 0)
	{
		Client c = null;
		if (mClients.TryGetValue (con.connectionId, out c))return c;
		if (accountId!=0)
		{
			c = new Client ();
			c.conn = con;
			c.accoundId = accountId;
			mClients [con.connectionId] = c;
		}
		return c;
	}

    #region server
    static uint ACCOUNTID = 10000;
    void onConnected(NetworkMessage msg)
    {
        msg.conn.SetMaxDelay(0.01f);
        //加载对应的账号数据
		Client nc = getClient(msg.conn, ACCOUNTID++);
        Log.i("LanHost onConnected accountId="+nc.accoundId+",connecttionId="+msg.conn.connectionId, Log.Tag.Net);
		//通知客户端登陆成功并返回账号ID
        SCLogin m = new SCLogin();
        m.accountId = nc.accoundId;
        sendTo(msg.conn.connectionId, (short)MyMsgId.Login, m);
    }

    void onDisconnected(NetworkMessage msg)
    {
		Client c = getClient(msg.conn);
		if (c==null)return;
        Log.i("LanHost onDisconnected accountId="+c.accoundId+",connecttionId="+msg.conn.connectionId, Log.Tag.Net);
		Unit u = mUnitMgr.getUnit (c.playerGUID);
		if (u != null)u.decState(UnitState.Exist);
        mClients.Remove (msg.conn.connectionId);
        c.deinit ();
    }

    void onTime(NetworkMessage msg)
    {
        Log.i("LanHost onTime", Log.Tag.Net);
        MsgTime m = msg.ReadMessage<MsgTime> ();
        m.serverUtcNs = System.DateTime.UtcNow.Ticks;
        m.serverLocalNs = System.DateTime.Now.Ticks;
        sendTo(msg.conn.connectionId, (short)MyMsgId.Time, m);
    }
		

    void onUnitMsg(NetworkMessage msg)
    {
		Log.i("LanHost onUnitMsg:"+msg.msgType, Log.Tag.Net);
		MsgUnit m = msg.ReadMessage<MsgUnit> ();
		msg.reader.SeekZero ();
		Log.i("LanHost guid:"+m.guid, Log.Tag.Net);
		Unit u = mUnitMgr.getUnit (m.guid);
		if (u != null)u.onSync (msg);
    }

    void onReqUnit(NetworkMessage msg)
    {
        Log.i("LanHost onReqUnit", Log.Tag.Net);
        MsgReqUnit m = msg.ReadMessage<MsgReqUnit> ();
        Unit u    = mUnitMgr.getUnit (m.guid);
        
		MsgCreate reply = new MsgCreate();
        reply.unitType = u.type;
        reply.guid  = u.guid;
		//reply.agentId = mClients[msg.conn.connectionId].accoundId;
		reply.pos   = u.pos;
		reply.dir   = u.dir;
		reply.state = u.state;
		reply.tid   = u.tid;
        sendTo (msg.conn.connectionId, (short)MyMsgId.Create, reply);
    }
		

    void OnReqEnterBattle(NetworkMessage msg)
    {
        Log.i("LanHost OnReqEnterBattle", Log.Tag.Net);
		//同步其他玩家信息
    }

	void OnReqCreateHero(NetworkMessage msg)
	{
		Log.i("LanHost OnReqCreateHero", Log.Tag.Net);
		Client client = getClient (msg.conn);
		if (client.playerGUID != 0)
		{//删除上个角色
			Unit lu = mUnitMgr.getUnit(client.playerGUID);
			if (lu != null)lu.decState (UnitState.Exist,true);
		}
		MsgReqCreateHero m = msg.ReadMessage<MsgReqCreateHero> ();
		Player u = createPlayer (m.heroID, Vector3.zero, Vector3.forward, mClients [msg.conn.connectionId].accoundId);
		client.playerGUID = u.guid;

		//同步其周围玩家信息
		List<Unit> units = mUnitMgr.getUnitInSphere(1, u.pos, 1000);
		for (int i = 0; i < units.Count; ++i)
		{
			Unit o = units[i];
			if (o.guid == u.guid || !o.isState(UnitState.Exist))continue;
			MsgCreate reply = new MsgCreate();
			reply.agentId = o.agentId;
			reply.guid    = o.guid;
			reply.pos     = o.pos;
			reply.dir     = o.dir;
			reply.state   = o.state;
			reply.tid     = o.tid;
			reply.unitType = o.type;
			sendTo(msg.conn.connectionId, (short)MyMsgId.Create, reply);
		}
	}

	public Monster createMonster(int tid, Vector3 dir, Vector3 pos, uint agentId=0)
    {
        Monster u = mUnitMgr.getUnit(0, 2, tid) as Monster;
		if (u == null)return null;
		u.agentId = agentId;
		u.setParam(pos, dir);
		u.ai.setPatrolPoint (new List<Vector3>{new Vector3(23,-1,23), new Vector3(-23,-1,23), new Vector3(-23,-1,-23), new Vector3(23,-1,-23)});
		u.ai.setPatrolArea (Vector3.zero, 6);

        MsgCreate reply = new MsgCreate();
        reply.agentId = u.agentId;
        reply.guid  = u.guid;
        reply.pos   = u.pos;
        reply.dir   = u.dir;
        reply.state = u.state;
		reply.tid   = u.tid;
        reply.unitType = u.type;
        sendToAll((short)MyMsgId.Create, reply);
        return u;
    }

	public Player createPlayer(int tid, Vector3 pos, Vector3 dir, uint agentId=0)
	{
		Player u = mUnitMgr.getUnit(0, 1, tid) as Player;
		u.agentId = agentId;
		u.setParam(pos, dir);

		MsgCreate reply = new MsgCreate();
		reply.agentId = u.agentId;
		reply.guid  = u.guid;
		reply.pos   = u.dir;
		reply.dir   = u.pos;
		reply.state = u.type;
		reply.tid   = u.tid;
		reply.unitType = u.type;
		sendToAll((short)MyMsgId.Create, reply);
		return u;
	}
    #endregion
}

