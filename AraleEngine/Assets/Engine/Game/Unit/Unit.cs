using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//需要同步的对象 玩家,怪物,npc,道具,buff,子弹
//通过该管理器可以找到任何一个网络对象,通过继承unit实现自己的网络对象
using UnityEngine.Networking;
using Arale.Engine;
using System;


#region Unit事件
//自定义事件使用Max+
public enum UnitEvent
{
    StateChanged=100,
    BeHit,
	NavBegin,
	NavEnd,
    MoveBegin,
    MoveEnd,
	SkillBegin,
	SkillEnd,
	BuffAdd,
	BuffDec,
	BuffClear,
	AIStart,
	AIStop,
    AIEnemyFound,//发现敌人
    AIEnemyDied, //敌人死亡
    AIWakeUp,    //AI唤醒
    AttrChanged, //属性改变
	HeadInfoInit,
	HeadInfoDeinit,
	Max,
}
#endregion

#region Unit状态
public class UnitState
{
    public const int ALL  =0xFFFF;
    public const int STOK =0x8000;
    public const int Init =0xFFFD;//初始状态
	public const int Exist=0x0001;//存
	public const int Alive=0x0002;//生
	public const int Move =0x0004;//移
	public const int Anim =0x0008;//动
	public const int Skill=0x0010;//技
	public const int Show =0x0020;//显
	public const int Harm =0x0040;//伤
    public const int Break=0x0080;//断
}
#endregion

#region Unit类型
public class UnitType
{
	public const int Player =0x0001;//玩家
	public const int Monster=0x0002;//怪物
	public const int NPC    =0x0004;//NPC
	public const int Bullet =0x0008;//子弹
	public const int Drop   =0x0010;//掉落
}
#endregion

#region Unit关系
public enum UnitRelation
{
    Self=0x1,//自己
    Emney=0x2,//敌人
    Friend=0x4,//朋友
    Neutral=0x8,//中立
}
#endregion

public abstract class Unit : LuaMono
{
	public uint  guid{ get; private set;}//单位编号
	public int   type{ get; private set;}//单位类型
	public int   tid { get; private set;}//单位配表id
    #region 状态管理
    int mState;//掩码最后位勿用，用来标识加状态还是减状态
    public int   state{get {return buff==null||(mState&UnitState.STOK)!=0?mState:mState=buff.unitState;}}
	public void addState(int mask, bool sync=false)
	{//请使用buff设置状态
        int oldState = state;
        mState |= mask;
        onUnitState(state, oldState);
        if (sync)syncState ();
	}

	public void decState(int mask, bool sync=false)
    {//请使用buff设置状态
        int oldState = state;
        mState &= (~mask);
        onUnitState(state, oldState);
        if (sync)syncState ();
	}

	public bool isState(int mask)
	{
		return (state & mask)!=0;
	}

	public void onSyncState(MsgState msg)
	{
        int oldState = state;
		pos = msg.pos;
		dir = msg.dir;
		mState = msg.state;
        onUnitState(state, oldState);
	}

	public void syncState()
	{
		MsgState msg = new MsgState();
		msg.guid  = guid;
		msg.pos   = pos;
		msg.dir   = dir;
		msg.state = state;
		sendMsg((short)MyMsgId.State, msg);
	}
    #endregion

	uint mAgentID;
	public uint  agentId
	{
		get{return mAgentID;}
		set
		{
			mAgentID = value;
			if (isAgent)
				name = name.Replace (' ', '*');
			else
				name = name.Replace ('*', ' ');
		}
	}//代理的id(accountId)由对应的客户端管理达到分布式处理,0由服务端管理
	public bool  isAgent{get{return  mAgentID==mgr.accountId;}}
	public bool  isServer{ set; get;}

    Transform mTran;
    Vector3  mPos;
    public Vector3  pos
    { 
        set
        {
            mPos = value;
            if (mTran != null)mTran.position = mPos;
        }
        get
        {
            if (mTran != null)mPos = mTran.position;
            return mPos;
        }
    }

    Vector3  mDir;//模型必须朝向z正轴
    public Vector3  dir
    {
        set
        {
            mDir = value;
            if (mTran != null)mTran.forward = mDir;
        }
        get
        {
            if (mTran != null)mDir = mTran.forward;
            return mDir;
        }
    }

    public Vector3 hitPos
    {
        get
        {
            if (mTran == null)return mPos;
            return mTran.GetChild(0).position;
        }
    }

	public void setDir(Vector3 dir)
	{
		if (dir == Vector3.zero)return;
		this.dir = Vector3.ProjectOnPlane(dir, Vector3.up).normalized;
	}

	public void forward(Vector3 tPos)
	{
		tPos.y = pos.y;
		dir = (tPos - pos).normalized;
	}

	public void backward(Vector3 tPos)
	{
		tPos.y = pos.y;
		dir = (pos - tPos).normalized;
	}

	Bounds mBound;
	public Bounds bound
	{
		get{
			if(mBound==null)mBound = new Bounds(pos, Vector3.one);
			mBound.center = pos;
			return mBound;
		}
	}

    public float scale=1;

    public void sendMsg(short msgid, MessageBase msg)
    {
        if (isServer)
            NetMgr.server.sendToAll(msgid, msg);
        else
            NetMgr.client.sendMsg(msgid, msg);
    }

	public Mgr mgr
    {
        get
        {
            if (isServer)
                return NetMgr.server.unitMgr;
            else
                return NetMgr.client.unitMgr;
        }
    }



	public void setParam(Vector3 pos, Vector3 dir, object param=null)
	{
		if (type == 0)throw new Exception ("can't new object by yourself");
        if (isState(UnitState.Alive))return;
        addState(UnitState.Alive);
		this.pos=pos;this.dir=dir;
		onUnitParam (param);
	}

    public delegate void OnUnitEventListener(int evt, object param, object sender);
    OnUnitEventListener onUnitEventListener;
    public void addListener(OnUnitEventListener listener){onUnitEventListener += listener;}
    public void removeListener(OnUnitEventListener listener){onUnitEventListener -= listener;}
    protected override bool onEvent(int evt, object param)
    {
        base.onEvent(evt, param);
        if(anim!=null)anim.onEvent(evt, param);
        if(buff!=null)buff.onEvent(evt, param);
        if(move!=null)move.onEvent(evt, param);
        if(effect!=null)effect.onEvent(evt, param);
        if(skill!= null)skill.onEvent(evt, param);
        if(ai!= null)ai.onEvent(evt, param);
        if(onUnitEventListener != null)onUnitEventListener(evt, param, this);
        return false;
    }

	public void sendUnitEvent(int evt, object param, bool sync=false)
	{
        onEvent (evt, param);
		if (!sync)return;
		MsgEvent msg = new MsgEvent();
		msg.guid = guid;
		msg.evt = evt;
		msg.param = param==null?"":param.ToString ();
		msg.sender = guid;
		sendMsg((short)MyMsgId.Event, msg);
	}
		
	public virtual void onSync(NetworkMessage msg)
	{
		switch (msg.msgType)
		{
		case (short)MyMsgId.State:
			{
				MsgState m = msg.ReadMessage<MsgState> ();
				this.onSyncState (m);
				if (isServer)sendMsg (msg.msgType, m);
				break;
			}
		case (short)MyMsgId.Nav:
			{
				MsgNav m = msg.ReadMessage<MsgNav> ();
                move.onSync(m);
				if (isServer)sendMsg (msg.msgType, m);
				break;
			}
		case (short)MyMsgId.Anim:
			{
				MsgAnim m = msg.ReadMessage<MsgAnim> ();
				anim.onSync (m);
				if (isServer)sendMsg (msg.msgType, m);
				break;
			}
		case (short)MyMsgId.Skill:
			{
				MsgSkill m = msg.ReadMessage<MsgSkill> ();
				if (isServer)skill.onSync (m);
				if (isServer)sendMsg (msg.msgType, m);
				break;
			}
		case (short)MyMsgId.Attr:
			{
				MsgAttr m = msg.ReadMessage<MsgAttr> ();
				attr.onSync (m);
				if (isServer)sendMsg (msg.msgType, m);
				break;
			}
		case (short)MyMsgId.Buff:
			{
				MsgBuff m = msg.ReadMessage<MsgBuff> ();
				buff.onSync (m);
				if (isServer)sendMsg (msg.msgType, m);
				break;
			}
		case (short)MyMsgId.Move:
			{
				MsgMove m = msg.ReadMessage<MsgMove> ();
                move.onSyncMove(m);
				if (isServer)sendMsg (msg.msgType, m);
				break;
			}
		case (short)MyMsgId.Event:
			{
				MsgEvent m = msg.ReadMessage<MsgEvent> ();
				sendUnitEvent (m.evt, m.param);
				if (isServer)sendMsg (msg.msgType, m);
				break;
			}
		}
	}
		
    protected abstract void onUnitInit();
	protected virtual void  onUnitParam(object param){}
    protected virtual void  onUnitState(int newState, int oldState){}
    protected abstract void onUnitUpdate();
    protected abstract void onUnitDeinit();
    public virtual int relation(Unit u){return 0;}
    public bool isRelation(Unit u, int relationMask)
    {
        if (u.guid == guid)
        {
            return (relationMask & (int)UnitRelation.Self) != 0;
        }
        else
        {
            int r = relation(u);
            UnitRelation ur =  r == 0 ? UnitRelation.Neutral : (r > 0 ? UnitRelation.Friend : UnitRelation.Emney);
            return (relationMask & (int)ur) != 0;
        }
    }
    public virtual float speed{get{return 0;}}
	public virtual AnimPlugin   anim{get{ return null;}}
	public virtual AttrPlugin   attr{get{ return null;}}
	public virtual Skill.Plug   skill{get{ return null;}}
	public virtual Buff.Plug    buff{get{ return null;}}
	public virtual EffectPlugin effect{get{ return null;}}
	public virtual Move.Plug    move{get{ return null;}}
	public virtual AIPlugin     ai{get{return null;}}

	public void OnCreateEffect(int effectId)
	{
		if (isServer)return;
		if(effect!=null)effect.playEffect (effectId);
	}

	public Transform getMount(string nodeName)
	{
		return getNode (transform, nodeName);
	}

	public Matrix4x4 localToWorld
	{
		get{ return Matrix4x4.TRS (pos, Quaternion.LookRotation (dir), Vector3.one); }
	}

	public static Transform getNode(Transform t, string name)
	{
		if (t.name == name)return t;
		for (int i = 0, max = t.childCount; i < max; ++i)
		{
			Transform r = getNode (t.GetChild (i), name);
			if(r!=null)return r;
		}
		return null;
	}


	#if UNITY_EDITOR
	void OnDrawGizmosSelected()
	{//对应的脚本在inspector必须为展开状态，否则不会被调用
		Gizmos.color = Color.green;
		Gizmos.DrawRay(pos, dir*2);
        if (move != null)move.drawDebug ();
		if (skill != null)skill.drawDebug ();
	}

	void OnDrawGizmos()
	{
		if (transform.parent == null)return;
		GameArea[] areas = transform.parent.GetComponentsInChildren<GameArea> ();
		if (areas.Length < 1)return;
		Material mat = GetComponentInChildren<Renderer> ().material;
		mat.color = Color.green;
		foreach (GameArea ga in areas)
		{
            if (ga.mArea == null)continue;
			Vector3 tpos = Matrix4x4.TRS (ga.transform.position, Quaternion.LookRotation (ga.transform.forward), Vector3.one).inverse.MultiplyPoint(pos); 
			if (ga.mArea.inArea (tpos))mat.color = Color.red;
		}
	}
		
	string testTID="配表ID";
	void OnGUI()
	{
        if (!object.Equals (UnityEditor.Selection.activeObject, gameObject) || attr==null || buff==null)return;
		float y = 100;
		GUI.color = Color.gray;
        string attrInfo = "STATE:"+Convert.ToString(state, 2)+" HP:"+attr.HP + " MP:" + attr.MP+" BUFF:" + buff.getBuffs(0xFFFF).Count;
		GUI.Label (new Rect(0,y,200,50), attrInfo);
		testTID = GUI.TextField (new Rect (0, y+=50, 100, 25), testTID);
		if (ai!=null && GUI.Button (new Rect (0, y+=25, 100, 25), ai.isPlaying?"关闭AI":"打开AI"))
		{
            sendEvent (ai.isPlaying?(int)UnitEvent.AIStop:(int)UnitEvent.AIStart,null);
		}
		if (buff != null && GUI.Button (new Rect (0, y += 25, 100, 25), "添加Buff"))
		{
			buff.addBuff (int.Parse(testTID));
		}
	}
	#endif


	public class Mgr
	{
		#region 同步单位管理器
		static uint mGuid;
		Dictionary<uint, Unit> mUnits = new Dictionary<uint, Unit>();
		List<Unit> mUnitList = new List<Unit>();
		public Transform  unitRoot{ get; protected set; }
		public uint accountId{ get; set; }
		bool isServer{ set; get;}

		public Mgr(bool isServer)
		{
			this.isServer = isServer;
			unitRoot = new GameObject(isServer?"Unit4S":"Unit4C").transform;
			GameObject.DontDestroyOnLoad(unitRoot);
		}

		public void dispose()
		{
			if (unitRoot != null)GameObject.Destroy (unitRoot.gameObject);
			unitRoot = null;
		}

        public void broadcastEvent(Unit sender, int evt, object param, float distance=3)
		{//把sender的事件广播给distance范围类的unit
			Vector3 senderPos = sender.pos;
			for (int i = 0, max = mUnitList.Count; i < max; ++i)
			{
				Unit moniter = mUnitList[i];
                if (Vector3.Distance(senderPos, moniter.pos) <= distance)
				{
                    Debug.Assert(false);//未实现
                    //moniter.onUnitEvent (evt, param, sender);
				}
			}
		}

		public Unit getUnit(uint guid, int unitType=0, int tid=0)
		{
			Unit unit;
			if (mUnits.TryGetValue(guid, out unit))return unit;
			if (unitType <= 0)return null;
			switch (unitType)
			{
			case UnitType.Player:
				unit = Player.Pool.alloc (tid) as Unit;
				break;
			case UnitType.Monster:
				unit = Monster.Pool.alloc(tid) as Unit;
				break;
			case UnitType.Bullet:
				unit = Bullet.Pool.alloc(tid) as Unit;
				break;
			case UnitType.Drop:
				unit = DropItems.Pool.alloc (tid) as Unit;
				break;
			default:
				Log.e("not surpport unit type="+unitType);
				break;
			}

			//======================
			if (guid==0)
			{//客户端必须指定guid创建对象
				if (!isServer)throw new Exception("client can't new Unit guid=0");
				unit.guid = ++mGuid;
			}
			else
			{//服务器不允许指定guid创建对象
				if (isServer)throw new Exception("server can't new Unit guid!=0");
				unit.guid = guid;
			}

			unit.type     = unitType;
			unit.tid      = tid;
            unit.mState   = UnitState.Init;
			unit.isServer = isServer;
			mUnitList.Add(unit);
			mUnits[unit.guid] = unit; 
			//======================
			unit.name = " "+unit.guid;
			unit.agentId  = 0;
            unit.scale = 1;
			unit.mTran = unit.transform;
			unit.mTran.SetParent(unit.mgr.unitRoot, false);
			unit.mTran.localScale = Vector3.one;
			GHelper.SetLayer (unit.mTran, isServer ? LayerMask.NameToLayer ("Server") : LayerMask.NameToLayer ("Client"));
			//=======================
			Log.i((isServer?"S ":"C ")+"create unit type=" + unit.type + ",guid=" + unit.guid, Log.Tag.Unit);
			unit.onUnitInit ();
			return unit;
		}

		//获取球体内的unit
		public List<Unit> getUnitInSphere(int unitType, Vector3 pos, float r)
		{
			List<Unit> ls = new List<Unit>();
			for (int i = 0, max = mUnitList.Count; i < max; ++i)
			{
				Unit u = mUnitList[i];
				if ((u.type&unitType)==0 || Vector3.Distance(pos,u.pos)>r)continue;
				ls.Add(u);
			}
			return ls;
		}

		//获取Area内的unit
		public List<Unit> getUnitInArea(int unitType, IArea area, Matrix4x4 mt)
		{
			List<Unit> ls = new List<Unit>();
			for (int i = 0, max = mUnitList.Count; i < max; ++i)
			{
				Unit u = mUnitList[i];
				if ((u.type&unitType)==0 || !area.inArea(mt.MultiplyPoint(u.pos)))continue;
				ls.Add(u);
			}
			return ls;
		}

		public List<Unit> getEnemy(Unit self, int unitType, float r, int count=0, Comparison<Unit> comp=null)
		{
			List<Unit> ls = new List<Unit>();
			for (int i = 0, max = mUnitList.Count; i < max; ++i)
			{
				Unit u = mUnitList[i];
				if ((u.type&unitType)==0 || self.relation(u)>=0 || Vector3.Distance (self.pos, u.pos)>r)continue;
				ls.Add (u);
			}

			if (comp != null)
			{
				ls.Sort (comp);
			}
			else
			{//距离最近
				ls.Sort(new DistanceComp(self));
			}
			return ls;
		}

		public static int minHPComp(Unit x, Unit y)
		{
			return y.attr.HP - x.attr.HP;
		}

		public class DistanceComp : IComparer<Unit>
		{
			public Unit self;
			public DistanceComp(Unit self)
			{
				this.self = self;
			}
			public int Compare(Unit x, Unit y)
			{
				float d = Vector3.SqrMagnitude(self.pos-y.pos) - Vector3.SqrMagnitude(self.pos-x.pos);
				if(d<0)return -1;
				else if(d>0)return 1;
				else return 0;
			}
		}
		#endregion


		#region 更新
		public void update()
		{
			for (int i = mUnitList.Count - 1; i >= 0; --i)
			{
				Unit it = mUnitList[i];
				if (!it.isState(UnitState.Exist))
				{
					Log.d("delUnit:"+it.guid, Log.Tag.Unit);
					mUnitList.RemoveAt(i);
					if (it.guid != 0)
					{
						mUnits.Remove(it.guid);
					}
					it.onUnitDeinit();
                    it.onUnitEventListener = null;
				}
				else
				{
					it.onUnitUpdate();
				}
			}
		}
		#endregion
	}
}
	


