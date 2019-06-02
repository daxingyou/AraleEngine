using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using Arale.Engine;

public class Player : Unit, PoolMgr<int>.IPoolObject
{
	public TBPlayer table{get;protected set;}
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
	Move.Plug   mMove;
	public override Move.Plug move{get{return mMove;}}
	AIPlugin    mAI;
	public override AIPlugin ai{get{return mAI;}}
	protected override void onAwake()
	{
		mAttr   = new AttrPlugin(this);
		mAnim   = new AnimPlugin(this);
		mSkill  = new Skill.Plug(this);
		mBuff   = new Buff.Plug(this);
		mEffect = new EffectPlugin(this);
		mMove   = new Move.Plug (this);
		mAI     = new LuaAI (this);
	}

	HeadInfo mHeadInfo;
    protected override void onUnitInit()
    {
		mAttr.reset ();
		mSkill.reset ();
		mBuff.reset ();
        mEffect.reset();
        mMove.reset();
        mAI.reset();

		mSkill.addSkills (GHelper.toIntArray (table.skills));
        mAttr.onAttrChanged += onAttrChanged;

        if (!isServer)
        {
            mHeadInfo = HeadInfo.Bind(this.transform, this);
            effect.playEffect(1);
        }
    }

    protected override void onUnitParam(object param)
    {
        if(isServer)mBuff.addBuff(1);
    }

	protected override void onUnitUpdate()
	{
		mSkill.update();
		mBuff.update();
		mEffect.update();
		mMove.update ();
		mAI.update ();
	}

	protected override void onUnitDeinit()
	{
		mAttr.onAttrChanged -= onAttrChanged;
		if (mHeadInfo != null)mHeadInfo.Unbind ();
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
		}
		return base.onEvent (evt, param, sender);
	}

	public override int relation(Unit u)
	{
		if (u.type != type)return -1;
		return 0;
	}

    public override float speed{get{return isState(UnitState.Move)?scale * table.speed:0;}}

	void onAttrChanged(int mask, object val)
	{
		if (!isState (UnitState.Alive))return;
        switch (mask)
        {
            case (int)AttrID.HP:
                int hp = (int)val;
                if (hp > 0)break;
                if(isServer)buff.addBuff(2);
                break;
            case (int)AttrID.Speed:
                scale = (float)val;
                break;
        }
	}

	#region 对象池
	public static PoolMgr<int> Pool = new PoolMgr<int> (delegate(int param) {
		TBPlayer tb = TableMgr.single.GetData<TBPlayer>(param);
		GameObject go = ResLoad.get(tb.model, ResideType.InScene).gameObject();
		Player o = go.AddComponent<Player>();
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
