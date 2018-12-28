using UnityEngine;
using System.Collections;
using Arale.Engine;

public class BattleSceneCtrl : SceneCtrl {
	public Unit player{ get; protected set;}
	protected override void onAwake()
	{
		EventMgr.single.AddListener ("Game.Player", OnBindPlayer);
	}

	protected override void onDestroy()
	{
		EventMgr.single.RemoveListener ("Game.Player", OnBindPlayer);
	}

	protected override void onUpdate()
	{
		if (player == null)return;
		if (Input.GetMouseButtonDown (0))
		{//行走目标选择
			if(GUIUtility.hotControl!=0)return;//点在GUI上了
			if(Camera.main==null)return;
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;
			if (!Physics.Raycast (ray, out hit) || hit.collider.name != "NavMesh")return;
			player.nav.startNav (hit.point);
		}

		if (Input.GetKeyDown (KeyCode.F1)) {
			player.skill.playIndex(0);
		}
		if (Input.GetKeyDown (KeyCode.F2)) {
			player.skill.playIndex(1);
		}
		if (Input.GetKeyDown (KeyCode.F3)) {
			player.skill.playIndex(2);
		}
		if (Input.GetKeyDown (KeyCode.F4)) {
			player.skill.playIndex(3);
		}
	}

	void OnBindPlayer(EventMgr.EventData ed)
	{
		player = ed.data as Player;
	}
}
