using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using Arale.Engine;
using System;

public class NavPlugin : Plugin
{
	public const float FollowDelay = 0.1f;//值越小跟随越紧
	public const float G = -10f;//重力加速度
	enum State
	{
		None,
		Run,
		Jump,
		Climb,
	}

    public NavMeshAgent mAgent;
    public float speedCfg = 4f;
	float   mYSpeed;
	float   mDelay;//同步延迟时间
	float   mScale = 1f;
	bool    mNaving;
	Unit    mTarget;
	State   mNavState;
	Vector3 mNavDir;
	Action<bool>  mNavCallback;
    public NavPlugin(Unit unit):base(unit)
	{
		mAgent = unit.GetComponent<NavMeshAgent> ();
		if (mUnit.attr != null)mUnit.attr.onAttrChanged += onAttrChange;
    }

	void onAttrChange(int mask, object val)
	{
		if (mask != (int)AttrID.Speed)return;
		mScale = (float)val;
	}

    bool arrive()
    {
		if(!mAgent.enabled || mAgent.pathPending)
            return false;
		if(mAgent.remainingDistance <= mAgent.stoppingDistance)
            return true;
        return false;
    }

	float speed
	{
		get{ return mUnit.isState (UnitState.Move)? 0 : speedCfg*mScale; }
	}

	void updateBySelf()
	{//导航Agent存在加速度问题,在同步时会产生速度方向不一致抖动的效果
	 //要自己同步需将角度加速度和移动加速度都设置为0
		if (mAgent.angularSpeed > 0f || mAgent.acceleration > 0f)return;
		Vector3 dir = (mAgent.steeringTarget - mUnit.pos).normalized;
		if (dir != mNavDir)
		{//关键帧通知
			mUnit.setDir(mNavDir = dir);
			sync (State.Run);
		}

		float d = Time.deltaTime * speed;
		if (d >= Vector3.Distance (mAgent.steeringTarget, mUnit.pos))
		{
			mUnit.pos = mAgent.steeringTarget;
		}
		else
		{
			mUnit.pos += d * mUnit.dir;
		}
		mAgent.nextPosition = mUnit.pos;
	}

    public override void update()
    {
		switch (mNavState)
		{
		case State.Run:
			mUnit.anim.sendEvent (AnimPlugin.Run);
			break;
		case State.Jump:
			mUnit.anim.sendEvent (AnimPlugin.Jump);
			break;
		case State.Climb:
			mUnit.anim.sendEvent (AnimPlugin.Climb);
			break;
		default:
			mUnit.anim.sendEvent (AnimPlugin.StopRun);
			if (mSteps.Count > 0)break;
			return;
		}

		if (mAgent.enabled)
		{//代理端
			agentUpdate();
			return;
		}

		if (mSteps.Count > 0)
		{//0.5s内采样完成 
			Step step = mSteps [0];
			mDelay = 0.001f*(RTime.R.utcTickMs - step.time);
			float dt = mDelay<=FollowDelay?0:Time.deltaTime/FollowDelay*mDelay;
			float detalTime = Time.deltaTime+dt;
			mNavDir = (step.pos - mUnit.pos).normalized;
			mDelay -= dt;
			if (Vector3.Distance (mUnit.pos, step.pos) <= detalTime * speed)
			{
				mSteps.RemoveAt (0);
				mUnit.pos = step.pos;
				mUnit.dir = step.dir;
				mNavDir   = step.speed.normalized;
				mNavState    = step.state;
				switch (mNavState)
				{
				case State.Jump:
					mYSpeed = 320f / 30f;
					break;
				case State.None:
					mNavDir = Vector3.zero;
					break;
				}
			}
			else
			{
				updatePos (detalTime);
			}
		}
		else
		{
			updatePos (Time.deltaTime);
		}
    } 


	void updatePos(float dTime)
	{
		//水平方向
		mUnit.setDir(mNavDir);
		mUnit.pos += dTime * mNavDir * speed;
		//垂直方向
		if (mNavState == State.Jump)
		{
			if (mAgent.enabled)mAgent.enabled = false;
			float h = mYSpeed * dTime;
			RaycastHit hit;
			if (h < 0 && Physics.Raycast (mUnit.pos, Vector3.down, out hit, Math.Abs (h), 0x01 << LayerMask.NameToLayer ("Default"))) {
				if (!mAgent.enabled)mAgent.enabled = true;
				mUnit.pos = hit.point;
				mNavState = State.None;
			} else {
				mUnit.pos += Vector3.up * h;
				mYSpeed = mYSpeed + G * dTime;
			}
		}
	}

	void updateLine(bool forward)
	{
		Vector3 b=Vector3.one;
		Vector3 e=Vector3.one;
		mUnit.pos = b + (e - b);
	}
		

	void agentUpdate()
	{
		if (arrive ())
		{
			stopNav (true);
		}
		else
		{//自己控制角色方向和移动
			updateBySelf ();	
		}

		//目标跟踪
		if (mTarget != null && mTarget.isState (UnitState.Alive))
		{
			mAgent.SetDestination (mTarget.pos);
		}
		else
		{
			mTarget = null;
		}
	}

	public void jump()
	{
		if (mNavState == State.Jump)return;
		stopNav();
		mNavState = State.Jump;
		mYSpeed = 320f/30f;
		sync (mNavState);
	}

	public void move(Vector3 dir)
	{
		if (mUnit.isState (UnitState.MoveCtrl))return;
		if (mScale == 0)return;
		if (mNavState == State.Jump)return;
        stopNav();
		mNavState = State.Run;
		if (dir != mNavDir)
		{
			mUnit.setDir(mNavDir = dir);
			sync (mNavState);
		}
    }

    public void stopMove()
	{
		if (mNaving)return;
		if (mNavState==State.None || mNavState==State.Jump)return;
		mNavState = State.None;
		mNavDir= Vector3.zero;
		sync(mNavState);
    }

	public void startNav(Vector3 pos, float stopDistance=0, Action<bool> callback=null)
	{
		if (mUnit.isState (UnitState.MoveCtrl))return;
		if (mAgent == null || mScale==0)return;
		if (callback != null)callback (false);
		mTarget = null;
		mNavCallback = callback;
		mAgent.enabled = true;
		mAgent.SetDestination(pos);
		mAgent.stoppingDistance = stopDistance;
		mNavState  = State.Run;
		mNaving = true;
		mUnit.sendUnitEvent ((int)UnitEvent.NavBegin, 0);
    }

	public void startNav(Unit unit, float stopDistance=0, Action<bool> callback=null)
	{
		if (mAgent == null || mScale==0)return;
		if (callback != null)callback (false);
		mTarget = unit;
		mNavCallback = callback;
		mAgent.enabled = true;
		mAgent.SetDestination(mTarget.pos);
		mAgent.stoppingDistance = stopDistance;
		mNavState  = State.Run;
		mNaving = true;
		mUnit.sendUnitEvent((int)UnitEvent.NavBegin, 0);
	}

	public void stopNav(bool isArrive=false)
	{
		if (mAgent == null||mAgent.enabled==false)return;
		if (mNavCallback != null)mNavCallback (isArrive);
		mNavCallback = null;
		mAgent.Stop ();
		mAgent.enabled = false;
		mNavState  = State.None;
		mNavDir = Vector3.zero;
		mNaving = false;
		mTarget = null;
		sync(State.None);
		mUnit.sendUnitEvent((int)UnitEvent.NavEnd, 0);
    }

	public void drawDebug()
	{
		if (mAgent != null)
		{
			Vector3[] points= mAgent.path.corners;
			DebugLine.drawLines (points, Color.blue);
		}
	}

    #region 行走插值同步
	class Step
    {
		public long    time;
        public Vector3 pos;
		public Vector3 dir;
		public Vector3 speed;
		public State   state;
		public Step(long time, Vector3 pos, Vector3 dir, Vector3 speed, State state)
        {
			this.time = time;
            this.pos  = pos;
			this.dir  = dir;
			this.speed= speed;
			this.state=state;
        }
    }

    List<Step> mSteps = new List<Step>();
	void sync(State navState)
	{
		if (!mUnit.isAgent)return;
        MsgNav msg = new MsgNav();
        msg.guid  = mUnit.guid;
        msg.time  = RTime.R.utcTickMs;
		msg.state = mUnit.state;
        msg.pos   = mUnit.pos;
        msg.dir   = mUnit.dir;
		msg.speed = mNavDir;
		msg.navState = (byte)navState;
        mUnit.sendMsg((short)MyMsgId.Nav, msg);
    }

    public override void onSync(MessageBase message)
    {
		if (mUnit.isAgent)return;
        MsgNav msg =  message as MsgNav;
		if (mUnit.isServer)
		{//检测合法性,不合法强制拉回
			//mUnit.onSyncState (msg);
			//msg.time  = RTime.R.utcTickMs;
			//mUnit.sendMsg ((short)MyMsgId.Nav, msg);
		}
		mSteps.Add(new Step(msg.time, msg.pos, msg.dir, msg.speed, (State)msg.navState));
    }
    #endregion

    int getOffMeshLinkAction()
	{
		if (!mAgent.isOnOffMeshLink)return 0;
		OffMeshLinkData lk = mAgent.currentOffMeshLinkData;
		mAgent.autoTraverseOffMeshLink = false;
		Vector3 b = lk.startPos;
		Vector3 e = lk.endPos;
		if (Vector3.Distance (mUnit.pos, b) > Vector3.Distance (mUnit.pos, e))
		{
			b = lk.endPos;
			e = lk.startPos;
		}
		int lnkType = lk.offMeshLink.area;
		switch (lnkType)
		{
		case 2:
			//mUnit.StartCoroutine (jump (b,e));
			return 1;
		case 3:
			//mUnit.StartCoroutine (climb (b,e));
			return 1;
		}
		return 0;
	}
}
