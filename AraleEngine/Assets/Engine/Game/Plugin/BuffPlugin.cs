using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using Arale.Engine;

#region BUFF基类(BUFF只运行于服务端)
public class Buff
{
	public enum Filter
	{
		ID,
		Kind,
	};
    public const int EvtInit   = 0;
    public const int EvtDeinit = 1;
    public const int EvtMutex  = 2;
	public const int EvtOverlyingBegin = 102;
	public const int EvtOverlyingEnd   = 103;
    protected int mUnitState=UnitState.ALL;
    public void addUnitState(Unit unit, int mask, bool sync=false)
    {//如果有一个buff锁定了该状态就不能恢复
        mUnitState |= mask;
        int m = unit.buff.buffUnitState();
        unit.addState(mask & m, sync);
    }
    public void decUnitState(Unit unit, int mask, bool sync=false)
    {//禁用状态可以随便禁用
        mUnitState &= (~mask);
        unit.decState(mask, sync);
    }
	protected TBBuff  mTB;
    protected float   mTime;
    protected int     mState;
	public int state{get{return mState;}set{mState = value;}}
	public bool   isOver{get{return mState <= 0;}}
	public TBBuff table{get{return mTB;}}
	public TimeMgr.TimeAxis timer{ get; protected set;}
	protected void init(Unit unit)
	{
		mTime = 0;
		mState= 1;
		timer = new TimeMgr.TimeAxis ();
		onInit (unit);
	}

	protected void deinit()
	{
		onDeinit ();
		timer = null;
        if(mUnitState!=UnitState.ALL)Log.e("buff state not clear!!! id=" + mTB.id, Log.Tag.Skill);
	}

	protected void update()
	{
		onUpdate ();
		timer.Update(mTime);
		mTime += Time.deltaTime;
	}

    protected virtual void onMutex(Unit unit, TBBuff buff, ref bool reject)
	{
		if((buff.kind & mTB.mutex) == 0)return;
		if (buff.priority >= mTB.priority)
		{//谁优先级高保留谁
			mState = 0;
		} 
		else
		{
            reject = true;	
		}
	}
	protected virtual void onInit(Unit unit){}
	protected virtual void onUpdate(){}
	protected virtual void onDeinit(){}
	protected virtual bool onEvent(int evt, object param){return false;}


	#region 插件
	public class Plug: Plugin
	{
		List<Buff> mBuffs = new List<Buff> ();
        delegate void OnMutex(Unit unit, TBBuff newBuff, ref bool reject);
		OnMutex onMutex;
		public Plug(Unit unit):base(unit)
		{
		}

		public override void reset ()
		{
			clearBuff (0xFFFF);
		}

		//creator是buff的创建者,用于计算动态伤害参数
		public void addBuff(int buffTID, Unit creator=null)
		{
			Debug.Assert (mUnit.isServer);
			Log.i("addBuff id=" + buffTID, Log.Tag.Skill);
			TBBuff tb = TableMgr.single.GetData<TBBuff>(buffTID);
			if (tb == null)return;
			bool isReject = false;
            if(onMutex!=null)onMutex (mUnit, tb, ref isReject);
			if(isReject)return;
			//挂载buff
			Buff buff = null;
			switch (tb.type)
			{
    			case 0:
    				buff = new LuaBuff ();
    				break;
    			case 1:
    				buff = new GameSkillBuff ();
    				break;
                case 2:
                    buff = new GameSkillBuffEx();
                    break;
    			default:
    				return;
			}
			buff.mTB = tb;
			buff.init(mUnit);
			mBuffs.Add(buff);
			addBuff (buff);
			if (tb.mutex != 0)onMutex += buff.onMutex;
		}

		public void clearBuff(int kindMask)
		{
			for (int i = 0,max=mBuffs.Count; i<max; ++i)
			{
				Buff buff = mBuffs [i];
				if (((1<<buff.mTB.kind) & kindMask) == 0)
					continue;
				buff.mState = 0;
			}
		}

		public void update()
		{
			for (int i = mBuffs.Count-1; i >= 0; --i)
			{
				Buff buff = mBuffs [i];
				if (buff.isOver)
				{
					mBuffs.RemoveAt (i);
					if (buff.mTB.mutex != 0)onMutex -= buff.onMutex;
					decBuff (buff);
					buff.deinit ();
				}
				else
				{
					buff.update ();
				}
			}
		}

		public List<Buff> getBuffs(int kindMask)
		{
			List<Buff> ls = new List<Buff> ();
			for (int i = 0,max=mBuffs.Count; i<max; ++i)
			{
				Buff bf = mBuffs [i];
				if ((kindMask & (1<<bf.mTB.kind)) == 0)continue;
				ls.Add (bf);
			}
			return ls;
		}

		public void sendEvent(Buff.Filter filterType, int filter, int evt, object param)
		{
			Debug.Assert (evt >= 100);//<100为系统事件
			if (filterType == Buff.Filter.ID)
			{
				for (int i = 0,max=mBuffs.Count; i<max; ++i)
				{
					Buff bf = mBuffs [i];
					if (filter != 0 && bf.mTB.id != filter)continue;
					bf.onEvent (evt, param);
				}
			}
			else
			{
				for (int i = 0,max=mBuffs.Count; i<max; ++i)
				{
					Buff bf = mBuffs [i];
					if ((filter & (1<<bf.mTB.kind)) == 0)continue;
					bf.onEvent (evt, param);
				}
			}
		}

		void clearBuff(Buff buff)
		{
			if (buff.table.flag == null)return;
			MsgBuff msg = new MsgBuff ();
			msg.guid = mUnit.guid;
			msg.change = 0;//clear
			msg.buff = (short)buff.table.id;
			mUnit.sendMsg ((short)MyMsgId.Buff, msg);
		}

		void addBuff(Buff buff)
		{
			if (buff.table.flag == null)return;
			MsgBuff msg = new MsgBuff ();
			msg.guid = mUnit.guid;
			msg.change = 1;//add
			msg.buff = (short)buff.table.id;
			mUnit.sendMsg ((short)MyMsgId.Buff, msg);
		}

		void decBuff(Buff buff)
		{
			if (buff.table.flag == null)return;
			MsgBuff msg = new MsgBuff ();
			msg.guid = mUnit.guid;
			msg.change = 2;//dec
			msg.buff = (short)buff.table.id;
			mUnit.sendMsg ((short)MyMsgId.Buff, msg);
		}

        public int buffUnitState()
        {
            int mask = UnitState.ALL;
            for (int i = 0, max = mBuffs.Count; i < max; ++i)mask &= mBuffs[i].mUnitState;
            return mask;
        }

        public void drawDebug()
        {
            
        }

		public void sync()
		{
			MsgBuff msg = new MsgBuff ();
			msg.guid = mUnit.guid;
			for (int i = 0, max = mBuffs.Count; i < max; ++i)
			{
				Buff bf = mBuffs [i];
				if (bf.table.flag != null)msg.flags.Add ((short)bf.table.id);
			}
			mUnit.sendMsg ((short)MyMsgId.Buff, msg);
		}

		public override void onSync(MessageBase message)
		{
			MsgBuff msg = message as MsgBuff;
			if (msg.buff != 0)
			{
				switch (msg.change)
				{
				case 0:
					mUnit.sendEvent ((int)UnitEvent.BuffClear, msg.buff);
					break;
				case 1:
					mUnit.sendEvent ((int)UnitEvent.BuffAdd, msg.buff);
					break;
				case 2:
					mUnit.sendEvent ((int)UnitEvent.BuffDec, msg.buff);
					break;
				}
			}
			else
			{
				for (int i = 0, max = msg.flags.Count; i < max; ++i)
				{
					mUnit.sendEvent ((int)UnitEvent.BuffAdd, msg.flags [i]);
				}
			}
		}
	}
	#endregion
}
#endregion
