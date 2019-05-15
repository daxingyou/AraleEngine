using UnityEngine;
using System.Collections;
using Arale.Engine;

public class DropItems : Unit, PoolMgr<int>.IPoolObject
{
	public TBItem table{get;protected set;}
	public bool pick (uint pickerGUID)
	{
		if (!isState (UnitState.Alive))return false;
		if (isServer)
		{
			MsgPick msg = new MsgPick ();
			msg.dropGuid = guid;
			msg.pickerGuid = pickerGUID;
			sendMsg ((short)MyMsgId.ReqPick, msg);
			dispear ();
			return true;
		}
		else
		{
			MsgPick msg = new MsgPick ();
			msg.dropGuid = guid;
			sendMsg ((short)MyMsgId.ReqPick, msg);
			return true;
		}
	}

	protected override void onUnitInit()
	{
		Invoke ("dispear", 60f);
	}

    protected override void onUnitUpdate()
    {
    }

	protected override void onUnitDeinit()
	{
		Pool.recyle (this);
	}

	void dispear()
	{
		decState (UnitState.Alive | UnitState.Exist);
	}

	#region 对象池
	public static PoolMgr<int> Pool = new PoolMgr<int> (delegate(int param) {
		TBItem tb = TableMgr.single.GetData<TBItem>(param);
		GameObject go = ResLoad.get("Model/dropitem", ResideType.InScene).gameObject();
		DropItems o = go.AddComponent<DropItems>();
		o.table = tb;
		return o;
	});

	public int getKey()
	{
		return 1;
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
