using UnityEngine;
using System.Collections;
using System;
using Arale.Engine;
using System.Collections.Generic;

public class Bullet : Unit, PoolMgr<int>.IPoolObject
{
	public uint          	mOwner{ set; get;}//GUID
	public int              mHarm{set;get;}
    public Action<int,int>  mOnEvent;
    protected float      	mDelay;
    protected int        	mState;
	public TBEffect table{ get; protected set;}
	Move.Plug mMove;
	public override Move.Plug move{get{ return mMove;}}
	Buff.Plug mBuff;
	public override Buff.Plug buff{get{ return mBuff;}}
	public void play(Vector3 vTarget, uint uTarget)
	{
        move.play (table.move, vTarget, mgr.getUnit(uTarget), false, onMoveEvent);
		if(this.isServer)sync ();
	}
	//=====
	protected override void onAwake()
	{
		mMove  = new Move.Plug(this);
		mBuff = new Buff.Plug (this);
	}

	void onMoveEvent(Move.Event evt, object param)
	{
		if (evt == Move.Event.Stop)
		{
			dispear ();
		}
		else if (evt == Move.Event.Arrive)
		{
			if (!isServer)
			{
				Invoke ("dispear", table.life);
				return;
			}

			Move mv = param as Move;
			switch (mv.table.type)
			{
			case 1:
				IArea area = GameArea.fromString ("0,1.0");
				Matrix4x4 mt = Matrix4x4.TRS (pos, Quaternion.LookRotation (dir), Vector3.one).inverse;
				List<Unit> units = mgr.getUnitInArea (UnitType.Monster|UnitType.Player, area, mt);
				Unit ower = mgr.getUnit (mOwner);
				for (int i = 0; i < units.Count; ++i)
				{
					Unit u = units[i];
					if(u.relation(ower)>=0)continue;
					u.anim.sendEvent (AnimPlugin.Hit);
					AttrPlugin ap = u.attr;
					ap.HP -= mHarm;
					ap.sync ();
				}
				break;
			case 3:
				Unit target = move.uTarget;
				if (target != null)
				{
					target.anim.sendEvent (AnimPlugin.Hit);
					AttrPlugin ap = target.attr;
					ap.HP -= mHarm;
					ap.sync ();
				}
				break;
			}
			Invoke ("dispear", table.life);
		}
	}

	void dispear()
	{
		decState (UnitState.Exist);
	}

	protected override void onUnitParam(object param)
	{
		Unit u = param as Unit;
		if (u!=null && !string.IsNullOrEmpty (table.srcMount))
		{
			pos = u.getMount(table.srcMount).position;
		}
	}

    protected override void onUnitInit()
    {
        mMove.reset();
        mBuff.reset();
    }

	protected override void onUnitUpdate()
	{
		mMove.update ();
		mBuff.update ();
		hit ();
	}

	protected override void onUnitDeinit()
	{
		Pool.recyle (this);
	}

	protected virtual void hit()
    {
		if (!isServer)return;
		//自己实现碰撞(可以对目标进行拣选)
		/*List<Unit> units = mgr.getUnitInSphere (1, pos, 1);
		Ray ray = new Ray (pos, dir);
		for (int i = 0, max = units.Count; i < max; ++i)
		{
			Unit u = units [i];
			float dis = 0;
			if (u.bound.IntersectRay(ray, out dis) && dis<mMove.speed)
			{
				//碰撞到u
				Debug.LogError("hit0:"+u.guid);
			}
		}*/
		//利用unity物理引擎检测碰撞体
		RaycastHit[] rh = Physics.RaycastAll (pos, dir, mMove.speed, 0x01<<LayerMask.NameToLayer ("Server"));
		if (rh!=null)
		{
			for (int i = 0; i < rh.Length; ++i)
			{
				Unit u = rh [i].collider.GetComponent<Unit> ();
				if (u == null)continue;
				Log.i ("hit target="+u.guid, Log.Tag.Skill);
				u.dir = -dir;
				u.anim.sendEvent (AnimPlugin.Hit);
				AttrPlugin ap = u.attr;
				ap.HP -= mHarm;
				ap.sync (); 
			}
		}
    }

	public void sync()
	{
		MsgCreateBullet msg = new MsgCreateBullet ();
		msg.guid    = guid;
		msg.pos     =  pos;
		msg.dir     =  dir;
		msg.state   = msg.state;
		msg.tid     = table._id;
		msg.vTarget = move.vTarget;
        msg.uTarget = move.uTarget==null?0:move.uTarget.guid;
		this.sendMsg ((short)MyMsgId.CreateBullet, msg);
	}

	#region 对象池
	public static PoolMgr<int> Pool = new PoolMgr<int> (delegate(int param) {
		TBEffect tb = TableMgr.single.GetData<TBEffect>(param);
		GameObject go = ResLoad.get(tb.model, ResideType.InScene).gameObject();
		Bullet o = go.AddComponent<Bullet>();
		o.table  = tb;
		return o;
	});

	public int getKey()
	{
		return table._id;
	}

	public void onReset()
	{
		gameObject.SetActive (true);
	}

	public void onRecycle()
	{
		transform.parent = null;
		gameObject.SetActive (false);
	}

	public void onDispose()
	{
		Destroy (gameObject);
	}
	#endregion
}