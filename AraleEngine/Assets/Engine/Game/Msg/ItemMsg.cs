using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class MsgPick : MessageBase
{
	public uint dropGuid;
	public uint pickerGuid;
}

public class MsgItem : MessageBase
{
	public int  itemId;
	public uint count;
	public override void Serialize(NetworkWriter w)
	{
		base.Serialize (w);
		w.Write (itemId);
		w.Write (count);
	}

	public override void Deserialize(NetworkReader r)
	{
		base.Deserialize (r);
		itemId   = r.ReadInt32 ();
		count = r.ReadUInt32 ();
	}
}