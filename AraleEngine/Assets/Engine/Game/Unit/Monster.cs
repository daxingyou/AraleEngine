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
	public int[] drops;

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
		mAI     = new LuaAI(this);
		mMove   = new Move.Plug (this);
	}

	HeadInfo mHeadInfo;
    protected override void onUnitInit()
    {
		mAttr.reset ();
		mSkill.reset ();
		mBuff.reset ();
        mMove.reset ();
        mAI.reset();

        mAttr.onAttrChanged += onAttrChanged;
		mSkill.addSkills (GHelper.toIntArray (table.skills));

		if (!isServer)
		{
			mHeadInfo = HeadInfo.Bind (this.transform, this); 
		}
    }

	protected override void onUnitParam(object param)
	{
		if(isAgent)mAI.startAI (table.ai);
	}

	protected override void onUnitUpdate()
	{
		mAI.update ();
		mSkill.update();
		mBuff.update();
		mEffect.update();
		mMove.update ();
	}

	protected override void onUnitDeinit()
	{
		mAttr.onAttrChanged -= onAttrChanged;
		if (mHeadInfo != null)mHeadInfo.Unbind ();
		mAI.stopAI ();
		mAnim.sendEvent (AnimPlugin.Die);
		if (isServer)
		{
			if (drops != null)
			{
                int itemId = drops[Random.Range(0, drops.Length)];
                if(Randoms.drop(itemId,1))
                {
                    DropItems u = NetMgr.server.createDropItems (itemId, pos, Vector3.right, 0);
                }
			}
		}
		Invoke ("recyle", 5f);
	}

	void recyle()
	{
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
				//headInfo.mName.text += tb.flag;
			}
			return true;
		case (int)UnitEvent.BuffDec:
			{
				TBBuff tb = TableMgr.single.GetData<TBBuff> ((short)param);
				//headInfo.mName.text = headInfo.mName.text.Replace(tb.flag,"");
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

    public override float speed{get{return isState(UnitState.Move)?0:scale * table.speed;}}

    void onAttrChanged(int mask, object val)
    {
        if (!isState (UnitState.Alive))return;
        switch (mask)
        {
            case (int)AttrID.HP:
                int hp = (int)val;
                if (hp > 0)break;
                decState (UnitState.Alive);
                addState (UnitState.Skill | UnitState.MoveCtrl | UnitState.Move);
                mAnim.sendEvent (AnimPlugin.Die);
                break;
            case (int)AttrID.Speed:
                scale = (float)val;
                break;
        }
    }

	#region 对象池
	public static PoolMgr<int> Pool = new PoolMgr<int> (delegate(int param) {
		TBMonster tb = TableMgr.single.GetData<TBMonster>(param);
		GameObject go = ResLoad.get(tb.model, ResideType.InScene).gameObject();
		Monster o = go.AddComponent<Monster>();
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
