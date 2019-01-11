using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class Client
{
	public uint accoundId{ get; set;}//账号id
	public uint playerGUID{get; set;}//关联的角色guid
	public Bag  bag;//关联背包
	public NetworkConnection conn{ get; set;}
	public Client()
	{
	}

	public void init()
	{
		bag = new Bag ();
	}

	public void deinit()
	{
		accoundId  = 0;
		playerGUID = 0;
		bag = null;
	}
}
