using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class MsgReqEnterBattle : MessageBase
{
	public int sceneID;
}

public class MsgReqCreateHero : MessageBase
{
	public int heroID;
}