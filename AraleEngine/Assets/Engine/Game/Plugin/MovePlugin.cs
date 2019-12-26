using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Arale.Engine;
using UnityEngine.Networking;
using System;

public class Move
{
	public enum Event
	{
		Start,
		Pause,
		Resume,
		Stop,
		Arrive,
	}

    public enum State
    {
        None,
        Move,
        Run,
        Jump,
        Climb,
    }

    public class Step
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
	
	public delegate void OnMoveEvent(Event evt, Move move);
	protected OnMoveEvent onEvent;
    public TBMove table{ get; protected set;}
    public Vector3   vTarget;
    public Unit      uTarget;
    protected float  mSpeed;
	protected virtual void start(Unit unit){}
    protected virtual void update(Unit unit){}
    protected virtual void stop(Unit unit, bool arrived)
    {
        unit.move.moveState = State.None;
        unit.move.mMove = null;
        unit.sendEvent((int)UnitEvent.MoveEnd, arrived);
        if (null != onEvent)onEvent(arrived?Event.Arrive:Event.Stop, this);
    }
    protected virtual void sync(Unit unit)
    {
        MsgMove msg = new MsgMove ();
        msg.guid   = unit.guid;
        msg.pos    = unit.pos;
        msg.dir    = unit.dir;
        msg.state  = unit.state;
        msg.moveId = table.id;
        msg.uTarget= uTarget==null?0:uTarget.guid;
        msg.vTarget= vTarget;
        unit.sendMsg((short)MyMsgId.Move, msg);
    }

    #region 移动插件
	public class Plug : Plugin
	{
		bool mPause;
        public Move mMove;
        CtrlMove mCtrlMove;
        NavMove  mNavMove;
        public State   moveState;
        public float   speed{get{return   mMove==null?0:mMove.mSpeed;}}
        public Move    curMove{get{return mMove;}}
        public Plug(Unit unit):base(unit)
		{
            mCtrlMove = new CtrlMove();
            NavMeshAgent agent = unit.GetComponent<NavMeshAgent> ();
            if(agent!=null)
            {
                mNavMove  = new NavMove();
                mNavMove.mAgent = agent;
            }
		}

        public Move play(int id, Vector3 vTarget, Unit uTarget, bool bSync=false, OnMoveEvent callbck=null)
        {
			TBMove tb = TableMgr.single.GetData<TBMove>(id);
			if (tb == null)return null;
			switch (tb.type)
			{
    			case 1:
    				mMove = new PosMove ();
    				break;
    			case 2:
    				mMove = new DirMove ();
    				break;
    			case 3:
    				mMove = new TraceMove ();
    				break;
    			case 4:
    				mMove = new JumpMove ();
    				break;
                case 5:
                    mMove = new PhysicMove();
                    break;
    			default:
    				Log.e ("unsupport move type=" + tb.type, Log.Tag.Unit);
    				return null;
			}

            mMove.vTarget = vTarget;
            mMove.uTarget = uTarget;
            mMove.table = tb;
            mMove.onEvent = callbck;
            mMove.start(mUnit);
            if (bSync)mMove.sync(mUnit);
            return mMove;
		}

        public void move(Vector3 dir)
        {//控制移动
            if(!mUnit.isState(UnitState.Move))return;
            if(mMove!=null&&!object.ReferenceEquals(mMove, mCtrlMove))mMove.stop(mUnit,false);
            mMove = mCtrlMove;
            mCtrlMove.vTarget = dir;
            mCtrlMove.uTarget = null;
            mCtrlMove.start(mUnit);
        }

        public void nav(Vector3 pos, float stopDistance=0, Action<bool> callback=null)
        {//导航移动
            if (moveState == State.Move)
            {
                if (null != callback)callback(false);
                return;
            }
            if (mNavMove == null || !mUnit.isState(UnitState.Move))
            {
                if (null != callback)callback(false);
                return;
            }
            if(mMove!=null&&!object.ReferenceEquals(mMove, mNavMove))mMove.stop(mUnit,false);
            mMove = mNavMove;
            mNavMove.uTarget = null;
            mNavMove.vTarget = pos;
            mNavMove.mCallback = callback;
            mNavMove.mAgent.stoppingDistance = stopDistance;
            mNavMove.start(mUnit);
        }

        public void nav(Unit unit, float stopDistance=0, Action<bool> callback=null)
        {//导航移动
            if (moveState == State.Move)
            {
                if (null != callback)callback(false);
                return;
            }
            if (mNavMove == null || !mUnit.isState(UnitState.Move))
            {
                if (null != callback)callback(false);
                return;
            }
            if(mMove!=null&&!object.ReferenceEquals(mMove, mNavMove))mMove.stop(mUnit,false);
            mMove = mNavMove;
            mNavMove.uTarget = unit;
            mNavMove.vTarget = unit.pos;
            mNavMove.mCallback = callback;
            mNavMove.mAgent.stoppingDistance = stopDistance;
            mNavMove.start(mUnit);
        }

        public void path(string path, bool autoPlay=true, Vector3 startPos=default(Vector3), bool inverse=false)
        {//路径移动
            if (!mUnit.isState(UnitState.Move))return;
            if(mMove!=null)mMove.stop(mUnit,false);
            PathMove pm = new PathMove();
            mMove = pm;
            pm.vTarget = startPos;
            pm.uTarget = null;
            pm.play(mUnit, path, autoPlay, inverse);
        }

        public void jump()
        {
        }

		public void update()
		{
            if (unit.anim != null)
            {
                switch (moveState)
                {
                    case State.Run:
                        unit.anim.sendEvent(AnimPlugin.Run);
                        break;
                    case State.Jump:
                        unit.anim.sendEvent(AnimPlugin.Jump);
                        break;
                    case State.Climb:
                        unit.anim.sendEvent(AnimPlugin.Climb);
                        break;
                    default:
                        unit.anim.sendEvent(AnimPlugin.StopRun);
                        break;
                }
            }

            if (moveState!=State.Move)stepSync();
            if (moveState==State.None||mMove==null)return;
            if (!mPause)mMove.update(mUnit);
		}

        public void stop(bool all=true)
		{
            if (mMove == null)return;
            moveState = State.None;
            mMove.stop(mUnit,false);
            mMove = null;
		}

        public void moveStop()
        {//停止控制移动
            if (object.ReferenceEquals(mMove, mCtrlMove))stop();
        }

		public void pause()
		{
			mPause = true;
		}

		public void resume()
		{
			mPause = false;
		}

        public override void reset ()
        {
            mStepping = false;
            mSteps.Clear();
            stop();
        }

		public void onSyncMove(MsgMove msg)
		{
            unit.onSyncState (msg);
            mStepping = false;
            mSteps.Clear();
            play (msg.moveId, msg.vTarget, unit.mgr.getUnit(msg.uTarget));
		}

        #region 行走插值同步
        const float FollowDelay = 0.1f;//值越小跟随越紧
        float mDelay;//同步延迟时间
        public bool  mStepping;
        public List<Step> mSteps = new List<Step>();
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
            mStepping = true;
            mSteps.Add(new Step(msg.time, msg.pos, msg.dir, msg.speed, (State)msg.navState));
        }

        void stepSync()
        {//0.5s内采样完成 
            if(!mStepping||!mUnit.isState(UnitState.Move))return;
            Move move = mCtrlMove;
            if (mSteps.Count < 1)
            {
                unit.setDir(move.vTarget);
                unit.pos += Time.deltaTime * move.vTarget * move.mSpeed;
                return;
            }

            Step step = mSteps [0];
            move.mSpeed = unit.speed;
            mDelay = 0.001f*(RTime.R.utcTickMs - step.time);
            float dt = mDelay<=FollowDelay?0:Time.deltaTime/FollowDelay*mDelay;
            float detalTime = Time.deltaTime+dt;
            move.vTarget = (step.pos - unit.pos).normalized;
            mDelay -= dt;
            if (Vector3.Distance (unit.pos, step.pos) <= detalTime * move.mSpeed)
            {
                mSteps.RemoveAt (0);
                unit.pos = step.pos;
                unit.dir = step.dir;
                move.vTarget  = step.speed.normalized;
                moveState= step.state;
                if (moveState == State.None)
                {
                    mStepping = mSteps.Count>0;
                }
            }
            else
            {
                unit.setDir(move.vTarget);
                unit.pos += detalTime * move.vTarget * move.mSpeed;
            }
        }
        #endregion

        public void drawDebug()
        {
            if (mNavMove == null)return;
            Vector3[] points= mNavMove.mAgent.path.corners;
            DebugLine.drawLines (points, Color.blue);
        }
	}
    #endregion
}
