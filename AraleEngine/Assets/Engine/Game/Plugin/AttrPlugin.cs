using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class AttrPlugin : Plugin
{
    public delegate void OnAttrChanged(int mask, object val=null);
    public OnAttrChanged onAttrChanged;
	public void AddAttrListener(OnAttrChanged callback){onAttrChanged += callback;}
	public void RemoveAttrListener(OnAttrChanged callback){onAttrChanged -= callback;}
    public virtual void notify(int attrID, object val=null)
	{
        if (mUnit.isServer)
        {
            changes.Add(new Attr(attrID, val));
        }
		if (onAttrChanged != null)onAttrChanged(attrID, val);
    }

    string mName;
    public string name
    {
        set{mName = value;notify((int)AttrID.Name, value);}
        get{return mName;}
    }

    int mState;
    public int state
    {
        set{mState = value;notify((int)AttrID.State, value);}
        get{return mState;}
    }

    int mLV;
    public int lv
    {
        set{mLV = value;notify((int)AttrID.LV, value);}
        get{return mLV;}
    }

    int mHP;
    public int HP
    {
        set{
			mHP = value;
			notify((int)AttrID.HP, value);
			if (mHP <= 0)
			{
				mUnit.decState (UnitState.Alive|UnitState.Exist,true);
			}
		}
        get{return mHP;}
    }

    int mMP;
    public int MP
    {
        set{mMP = value;notify((int)AttrID.MP, value);}
        get{return mMP;}
    }

	float mSpeed;
	public float speed
	{
		set{mSpeed = value;notify((int)AttrID.Speed, value);}
		get{return mSpeed;}
	}

    int[] mHPS;//0最终属性1基础属性2装备加成3技能加成
    int[] mHPRS;
    int[] mMPS;
    int[] mMPRS;
    int[] mDamageS;
    int[] mDamageMagicS;
    int[] mDamageDefS;
    int[] mDamageMagicDefS;
    int[] mStrangeS;
    public AttrPlugin(Unit unit):base(unit)
    {
        mHP = 100;
        mMP = 100;
		mSpeed = 1f;
    }

    void setAttr(Attr attr)
    {
        switch (attr.id)
        {
        case (int)AttrID.Name:
            name = (string)attr.val;
            break;
		case (int)AttrID.HP:
			HP = (int)attr.val;
            break;
        case (int)AttrID.MP:
            MP = (int)attr.val;
            break;
        case (int)AttrID.BuffEffect:
            int buffEffect = (int)attr.val;
            break;
		case (int)AttrID.Speed:
			speed = (float)attr.val;
			break;
        }
    }

    public override void onSync(MessageBase message)
    {
        if (mUnit.isServer)return;
        MsgAttr msg = message as MsgAttr;
        int count = msg.attrs.Count;
        for (int i = 0; i < count; ++i)
        {
            Attr attr = msg.attrs[i];
            setAttr(attr);
        }
    }

    List<Attr> changes = new List<Attr>();
    public void sync()
    {
        if (!mUnit.isServer)return;
        MsgAttr msg = new MsgAttr();
        msg.guid = mUnit.guid;
        msg.attrs = changes;
        mUnit.sendMsg((short)MyMsgId.Attr, msg);
        changes.Clear();
    }
}
