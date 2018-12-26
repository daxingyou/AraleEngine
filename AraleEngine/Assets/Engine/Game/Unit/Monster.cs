using UnityEngine;
using System.Collections;
using Arale.Engine;

public class Monster : Unit, PoolMgr<int>.IPoolObject
{
    public const int CMD_ENABLE_AI = 1;
    public const int CMD_RUN = 2;
    public const int CMD_SKILL = 3;
    public const int CMD_DEATH = 6;
    public const int CMD_ATTACKED = 7;
    public const int CMD_SHOWHEAD = 8;
    public const int CMD_STOPRUN = 9;
    public const int CMD_LOOKAT = 10;
    public const int CMD_JUMP = 11;
    public const int CMD_HIT = 13;

    public const int CMD_NAV_TARGET = 14;//导航到目标
    public const int CMD_NAV_POS = 15;   //导航到位置
    public const int CMD_NAV_STOP = 16;  //停止导航
	public TBMonster table{get;protected set;}

    AnimPlugin   mAnim;
	public override AnimPlugin anim{get{return mAnim;}}
    AttrPlugin   mAttr;
	public override AttrPlugin attr{get{return mAttr;}}
    Skill.Plug  mSkill;
	public override Skill.Plug skill{get{return mSkill;}}
	Buff.Plug   mBuff;
	public override Buff.Plug buff{get{return mBuff;}}
    EffectPlugin mEffect;
	public override EffectPlugin effect{get{ return mEffect;}}
    NavPlugin    mNav;
	public override NavPlugin nav{get{return mNav;}}
	AIPlugin     mAI;
	public override AIPlugin ai{get{return mAI;}}
	Move.Plug   mMove;
	public override Move.Plug move{get{return mMove;}}
	public HeadInfo headInfo{ get; protected set;}
	protected override void onAwake()
	{
		mAnim   = new AnimPlugin(this);
		mAttr   = new AttrPlugin(this);
		mSkill  = new Skill.Plug(this);
		mBuff   = new Buff.Plug(this);
		mEffect = new EffectPlugin(this);
		mNav    = new NavPlugin(this); 
		//mAI     = new BTAI(this);
		mMove   = new Move.Plug (this);
	}

    protected override void onUnitInit()
    {
		base.onUnitInit ();
		mNav.speedCfg = table.speed;
		mSkill.addSkills (GHelper.toIntArray (table.skills));

		if (!isServer)
		{
			GameObject go = ResLoad.get ("UI/HeadInfo").gameObject ();
			HeadInfo h = go.GetComponent<HeadInfo> ();
			h.mTarget = transform;
			h.mName.text = table.name;
			go.transform.SetParent (GRoot.single.uiRoot, false);
			headInfo = h;
		}
    }

	protected override void onUnitUpdate()
	{
		mAI.update ();
		mSkill.update();
		mBuff.update();
		mNav.update();
		mEffect.update();
		mMove.update ();
	}

	protected override void onUnitDeinit()
	{
		mAI.stopAI ();
		Pool.recyle (this);
	}

	protected override bool onEvent (int evt, object param, object sender)
	{
		switch (evt)
		{
		case (int)UnitEvent.AIStart:
			mAI.startAI (table.ai);
			return true;
		case (int)UnitEvent.AIStop:
			mAI.stopAI ();
			return true;
		case (int)UnitEvent.BuffAdd:
			{
				TBBuff tb = TableMgr.single.GetData<TBBuff> ((short)param);
				headInfo.mName.text += tb.flag;
			}
			return true;
		case (int)UnitEvent.BuffDec:
			{
				TBBuff tb = TableMgr.single.GetData<TBBuff> ((short)param);
				headInfo.mName.text = headInfo.mName.text.Replace(tb.flag,"");
			}
			return true;
		}
		return base.onEvent (evt, param, sender);
	}

	public override int relation(Unit u)
	{
		if(u.type!=type)return -1;
		return 0;
	}

	#region 对象池
	public static PoolMgr<int> Pool = new PoolMgr<int> (delegate(int param) {
		TBMonster tb = TableMgr.single.GetData<TBMonster>(param);
		GameObject go = ResLoad.get(tb.model, ResideType.InScene).gameObject();
		Monster o = go.GetComponent<Monster>();
		o.table = tb;
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
