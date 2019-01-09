using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using Arale.Engine;

public enum MyMsgId
{
    Time   = 998,
    Login  = 999,
	State  = 1000,
	Anim   = 1001,
	Create = 1002,  
    Buff   = 1004,
    Attr   = 1005,
    Nav    = 1006,
	Move   = 1007,
	Event  = 1008,
	Skill  = 1009,
	ReqUnit= 2007,
	ReqPick= 2008,
    ReqEnterBattle=3009,
	ReqCreateHero=3010,
	CreateBullet=4010,
}

#region game message
public class MsgTime  : MessageBase
{
    public long clientUtcNs;
    public long serverUtcNs;
    public long serverLocalNs;
    public override void Serialize(NetworkWriter w)
    {
        base.Serialize (w);
        w.Write (clientUtcNs);
        w.Write (serverUtcNs);
        w.Write (serverLocalNs);
    }

    public override void Deserialize(NetworkReader r)
    {
        base.Deserialize (r);
        clientUtcNs  = r.ReadInt64 ();
        serverUtcNs = r.ReadInt64 ();
        serverLocalNs = r.ReadInt64();
    }
}

public class SCLogin  : MessageBase
{
    public uint    accountId;
    public override void Serialize(NetworkWriter w)
    {
        base.Serialize (w);
        w.Write (accountId);
    }

    public override void Deserialize(NetworkReader r)
    {
        base.Deserialize (r);
        accountId  = r.ReadUInt32 ();
    }
}

public class MsgCreate : MsgState
{
	public uint    agentId;
	public int     unitType;
	public int     tid;

	public override void Serialize(NetworkWriter w)
	{
		base.Serialize (w);
		w.Write (agentId);
		w.Write (unitType);
		w.Write (tid);
	}

	public override void Deserialize(NetworkReader r)
	{
		base.Deserialize (r);
		agentId = r.ReadUInt32();
		unitType = r.ReadInt32();
		tid   = r.ReadInt32 ();
	}
}

public class MsgCreateBullet : MsgState
{
	public int tid;
	public Vector3 vTarget;
	public uint    uTarget;
	public override void Serialize(NetworkWriter w)
	{
		base.Serialize (w);
		w.Write (tid);
		w.Write (vTarget);
		w.Write (uTarget);
	}

	public override void Deserialize(NetworkReader r)
	{
		base.Deserialize (r);
		tid     = r.ReadInt32();
		vTarget = r.ReadVector3 ();
		uTarget = r.ReadUInt32 ();
	}
}

public class MsgReqUnit : MessageBase
{
	public uint	guid;
	public override void Serialize(NetworkWriter w)
	{
		base.Serialize (w);
		w.Write (guid);
	}

	public override void Deserialize(NetworkReader r)
	{
		base.Deserialize (r);
		guid   = r.ReadUInt32 ();
	}
}

public class MsgSkill : MsgState
{
	public int     skillTID;
	public Vector3 targetPos;
	public uint    targetGUID;
	public override void Serialize(NetworkWriter w)
	{
		base.Serialize (w);
		w.Write (skillTID);
		w.Write (targetPos);
		w.Write (targetGUID);
	}

	public override void Deserialize(NetworkReader r)
	{
		base.Deserialize (r);
		skillTID   = r.ReadInt32 ();
		targetPos = r.ReadVector3 ();
		targetGUID = r.ReadUInt32 ();
	}
}

public class MsgPick : MessageBase
{
	public uint dropGuid;
	public uint pickerGuid;
}
#endregion


#region unit message
public class MsgUnit : MessageBase
{
	public uint    guid;
	public override void Serialize(NetworkWriter w)
	{
		base.Serialize (w);
		w.Write (guid);
	}

	public override void Deserialize(NetworkReader r)
	{
		base.Deserialize (r);
		guid  = r.ReadUInt32 ();
	}
}

public class MsgState : MsgUnit
{
	public Vector3 pos;
	public Vector3 dir;
	public int     state;
	public long    time;
	public override void Serialize(NetworkWriter w)
	{
		base.Serialize (w);
		w.Write (pos);
		w.Write (dir);
		w.Write (state);
		w.Write (time);
	}

	public override void Deserialize(NetworkReader r)
	{
		base.Deserialize (r);
		pos   = r.ReadVector3 ();
		dir   = r.ReadVector3();
		state = r.ReadInt32 ();
		time  = r.ReadInt64 ();
	}
}

public class MsgAnim  : MsgState
{
	public int   evt;
	public string anim;
	public override void Serialize(NetworkWriter w)
	{
		base.Serialize (w);
		w.Write (evt);
		w.Write (anim);
	}

	public override void Deserialize(NetworkReader r)
	{
		base.Deserialize (r);
		evt    = r.ReadInt32 ();
		anim  = r.ReadString ();
	}
}


public class MsgBuff : MsgUnit
{
	public short change;
	public short buff;
	public List<short> flags = new List<short>();
	public override void Serialize(NetworkWriter w)
	{
		base.Serialize (w);
		w.Write (change);
		w.Write(buff);
		w.Write(flags.Count);
		for (int i = 0; i < flags.Count; ++i)
		{
			w.Write (flags [i]);
		}
	}

	public override void Deserialize(NetworkReader r)
	{
		base.Deserialize (r);
		change= r.ReadInt16 ();
		buff  = r.ReadInt16 ();
		int count  = r.ReadInt32 ();
		for(int i=0;i<count;++i)
		{
			flags.Add (r.ReadInt16 ());
		}
	}
}

public class MsgEvent : MsgUnit
{
	public int evt;
	public string param;
	public uint sender;
	public override void Serialize(NetworkWriter w)
	{
		base.Serialize (w);
		w.Write(evt);
		w.Write(param);
		w.Write(sender);
	}

	public override void Deserialize(NetworkReader r)
	{
		base.Deserialize (r);
		evt = r.ReadInt32 ();
		param = r.ReadString ();
		sender = r.ReadUInt32 ();
	}
}

public enum AttrID
{
	Name=1,
	HP = 2,
	MP = 3,
	BuffEffect= 4,
	State=5,
	LV = 6,
	Speed=7,
}

[System.Serializable]
public class Attr 
{
	public Attr(int id, object val)
	{
		this.id = id;
		this.val = val;
	}
	public int id;
	public object val;
}

public class MsgAttr : MsgUnit
{
	public List<Attr> attrs = new List<Attr>();
	public override void Serialize(NetworkWriter w)
	{
		base.Serialize (w);
		w.Write(attrs.Count);
		for (int i = 0; i < attrs.Count; ++i)
		{
			byte[] bs = GHelper.Object2Bytes(attrs[i]);
			w.WriteBytesFull(bs);
		}
	}

	public override void Deserialize(NetworkReader r)
	{
		base.Deserialize (r);
		int count  = r.ReadInt32 ();
		for(int i=0;i<count;++i)
		{
			byte[] bs = r.ReadBytesAndSize();
			Attr attr = (Attr)GHelper.Bytes2Object(bs);
			attrs.Add(attr);
		}
	}
}

public class MsgEffect : MsgUnit
{
	public int  effectTID;
	public override void Serialize(NetworkWriter w)
	{
		base.Serialize (w);
		w.Write(effectTID);
	}

	public override void Deserialize(NetworkReader r)
	{
		base.Deserialize (r);
		effectTID = r.ReadInt32();
	}
}

public class MsgNav : MsgState
{
	public Vector3 speed;
	public byte  navState;
	public override void Serialize(NetworkWriter w)
	{
		base.Serialize (w);
		w.Write (speed);
		w.Write(navState);
	}

	public override void Deserialize(NetworkReader r)
	{
		base.Deserialize (r);
		speed = r.ReadVector3 ();
		navState = r.ReadByte();
	}
}

public class MsgMove : MsgState
{
	public int     moveId;
	public uint    uTarget;
	public Vector3 vTarget;
	public override void Serialize(NetworkWriter w)
	{
		base.Serialize (w);
		w.Write (moveId);
		w.Write (uTarget);
		w.Write (vTarget);
	}

	public override void Deserialize(NetworkReader r)
	{
		base.Deserialize (r);
		moveId  = r.ReadInt32();
		uTarget = r.ReadUInt32 ();
		vTarget = r.ReadVector3 ();
	}
}
#endregion


#region 战场
public class MsgReqEnterBattle : MessageBase
{
    public int sceneID;
}

public class MsgReqCreateHero : MessageBase
{
	public int heroID;
}
#endregion