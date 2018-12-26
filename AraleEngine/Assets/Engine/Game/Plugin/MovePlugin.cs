using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Arale.Engine;

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
	
	public delegate void OnMoveEvent(Event evt, object param);
	protected OnMoveEvent   onEvent;
	protected TBMove    table;
	protected Vector3   vTarget;
	protected uint      uTarget;
	protected bool isOver{ get; set;}
	protected virtual void init(Unit unit){}
	protected virtual void deinit(Unit unit){}
	protected virtual void update(Unit unit){}

	protected void arrived()
	{
		isOver = true;
		if (null != onEvent)onEvent(Event.Arrive, 0);
	}

	protected void stop()
	{
		isOver = true;
		if (null != onEvent)onEvent(Event.Stop, 0);
	}

	public class Plug : Plugin
	{
		bool mPause;
		List<Move> mMoves = new List<Move> ();
		Move mMove;
		public float   speed{get{return mMove.table.speed;}}
		public Vector3 vTarget{get{return mMove.vTarget;}}
		public uint    uTarget{get{return mMove.uTarget;}}
		public Plug(Unit unit):base(unit)
		{
		}
		
		public void play(int id, Unit other, bool bSync=false, OnMoveEvent callbck=null)
		{
			
		}

		public void play(int id, Vector3 vTarget, uint uTarget, bool bSync=false, OnMoveEvent callbck=null)
		{
			TBMove tb = TableMgr.single.GetData<TBMove>(id);
			if (tb == null)return;
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
			default:
				return;
			}
			mMove.table = tb;
			mMove.vTarget= vTarget;
			mMove.uTarget= uTarget;
			mMove.onEvent = callbck;
			mMoves.Add (mMove);
			mMove.init (mUnit);
			if (bSync)sync ();
		}

		public void update()
		{
			for (int i = mMoves.Count-1; i >= 0; --i)
			{
				Move mov = mMoves [i];
				if (mov.isOver)
				{
					mMoves.RemoveAt (i);
					mov.deinit (mUnit);
				}
				else
				{
					if(!mPause)mov.update (mUnit);
				}
			}
		}

		public void stop()
		{
			//if (null != onEvent)onEvent(Event.Stop, this);
		}

		public void pause()
		{
			mPause = true;
			//if (null != onEvent)onEvent(Event.Pause, this);
		}

		public void resume()
		{
			mPause = false;
			//if (null != onEvent)onEvent(Event.Resume, this);
		}

		void sync()
		{
			MsgMove msg = new MsgMove ();
			msg.guid   = mUnit.guid;
			msg.pos    = mUnit.pos;
			msg.dir    = mUnit.dir;
			msg.state  = mUnit.state;
			msg.moveId = mMove.table.id;
			msg.uTarget= mMove.uTarget;
			msg.vTarget= mMove.vTarget;
			mUnit.sendMsg ((short)MyMsgId.Move, msg);
		}

		public void onSync(MsgMove msg)
		{
			mUnit.onSyncState (msg);
			play (msg.moveId, msg.vTarget, msg.uTarget);
		}
	}
}
