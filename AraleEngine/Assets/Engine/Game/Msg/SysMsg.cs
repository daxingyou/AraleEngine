using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

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