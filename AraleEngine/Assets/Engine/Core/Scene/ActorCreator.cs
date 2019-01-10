using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Net;
using Arale.Engine;

[RequireComponent(typeof(SphereCollider))]
public class ActorCreator : MonoBehaviour
{
	[System.Serializable]
	public class ActorInfo
	{
		public int actorId;  //角色id
		public int userData; //自定义数据
		public Vector3 pos;  //角色出生位置
		public string drop;  //掉落
		public virtual void Serialize(XmlDocument xml, XmlNode n)
		{
			XmlAttribute at = null;
			at = xml.CreateAttribute("id");
			at.Value = actorId.ToString ();
			n.Attributes.Append (at);
			at = xml.CreateAttribute("userdata");
			at.Value = userData.ToString ();
			n.Attributes.Append (at);
			at = xml.CreateAttribute("pos");
			Vector3 v = pos;
			at.Value = string.Format("{0},{1},{2}",v.x,v.y,v.z);
			n.Attributes.Append (at);
			if (!string.IsNullOrEmpty (drop))
			{
				at = xml.CreateAttribute ("drop");
				at.Value = drop;
				n.Attributes.Append (at);
			}
		}
	}

	public int mWaves;       //刷新波次,0无限
	public int mRefreshType; //刷新类型
	public List<ActorInfo> mActorInfo = new List<ActorInfo>(); //角色刷新信息列表
	public List<Vector3>   mBornPos   = new List<Vector3>();   //随机出生点
	int mUnitCount;
	// Use this for initialization
    void OnTriggerEnter()
    {
		Debug.LogError ("OnTriggerEnter");
		if (NetMgr.server == null)return;//不是服务器
		if (mUnitCount > 0)return;//刷新点怪没有清光
		StartCoroutine(CreateActor());
    }

	IEnumerator CreateActor()
	{
		mUnitCount = mActorInfo.Count;
		for (int i = 0; i < mActorInfo.Count; ++i)
		{
			ActorInfo a = mActorInfo [i];
			Unit u = NetMgr.server.createMonster(a.actorId, Vector3.right, a.pos);
			(u as Monster).drops = GHelper.toIntArray (a.drop);
			u.AddStateListener (OnUnitStateChange);
			yield return null;
		}
	}

	void OnUnitStateChange(Unit u)
	{
		if (u.isState (UnitState.Exist))return;
		--mUnitCount;
	}

	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		for (int i = 0; i < mBornPos.Count; ++i)Gizmos.DrawWireSphere(mBornPos [i], 0.5f);
		for (int i = 0; i < mActorInfo.Count; ++i)Gizmos.DrawWireSphere(mActorInfo [i].pos, 0.5f);
	}

	public virtual void Serialize(XmlDocument xml, XmlNode n)
	{
		XmlAttribute at = null;
		at = xml.CreateAttribute("pos");
		Vector3 v = transform.position;
		at.Value = string.Format("{0},{1},{2}",v.x,v.y,v.z);
		n.Attributes.Append (at);
		SphereCollider sc = GetComponent<SphereCollider> ();
        sc.isTrigger = true;
		at = xml.CreateAttribute("radius");
		at.Value = sc.radius.ToString ();
		n.Attributes.Append (at);
		at = xml.CreateAttribute("wave");
		at.Value = mWaves.ToString ();
		n.Attributes.Append (at);
		at = xml.CreateAttribute("refreshtype");
		at.Value = mRefreshType.ToString ();
		n.Attributes.Append (at);

		for (int i = 0; i < mBornPos.Count; ++i)
		{
			XmlNode posNode = xml.CreateElement("Pos");
			n.AppendChild(posNode);

			at = xml.CreateAttribute("pos");
			v = mBornPos [i];
			at.Value = string.Format("{0},{1},{2}",v.x,v.y,v.z);
			posNode.Attributes.Append (at);
		}

		for (int i = 0; i < mActorInfo.Count; ++i)
		{
			XmlNode actorNode = xml.CreateElement("Actor");
			n.AppendChild(actorNode);
			mActorInfo [i].Serialize (xml, actorNode);
		}
	}
}
