using UnityEngine;
using System.Collections;
using Arale.Engine;
using UnityEditor;
using UnityEngine.EventSystems;

public class TestUnit : GRoot
{
	public static Unit mPlayer;
	protected override void gameStart()
	{
		Log.mFilter = (int)(Log.Tag.Net | Log.Tag.Unit | Log.Tag.Skill | Log.Tag.Default);
		Log.mDebugLevel = 2;
		gameObject.AddComponent<LuaRoot> ();
		EventMgr.single.AddListener ("Game.Login", onLogin);
		EventMgr.single.AddListener ("Game.Player", onPlayer);
		NetMgr.single.createLanHost ("wanghuan");
		TableMgr.TestModel = true;
		Selection.selectionChanged = delegate {
			GameObject go = Selection.activeObject as GameObject;
			if(go==null)return;
			Unit u = go.GetComponent<Unit>();
			if(u==null)return;
			if((u.type==1||u.type==2) && u.name.StartsWith("*"))mPlayer=u;
		};
			
	}

	protected override void gameExit()
	{
		Selection.selectionChanged = null;
		mPlayer = null;
		NetMgr.single.Deinit ();
		EventMgr.single.RemoveListener ("Game.Login", onLogin);
		EventMgr.single.RemoveListener ("Game.Player", onPlayer);
	}

	protected override void gameUpdate()
	{
		if (mPlayer == null)return;

		if (Input.GetKey (KeyCode.UpArrow)) {
			mPlayer.nav.move (Vector3.forward);
		} else if (Input.GetKey (KeyCode.DownArrow)) {
			mPlayer.nav.move (Vector3.back);
		} else if (Input.GetKey (KeyCode.LeftArrow)) {
			mPlayer.nav.move (Vector3.left);
		} else if (Input.GetKey (KeyCode.RightArrow)) {
			mPlayer.nav.move (Vector3.right);
		} else if (Input.GetKey (KeyCode.J)) {
			mPlayer.nav.jump ();
		} else {
			mPlayer.nav.stopMove ();
		}

		if (Input.GetMouseButtonDown (0))
		{//行走目标选择
			if(GUIUtility.hotControl!=0)return;//点在GUI上了
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;
			if (!Physics.Raycast (ray, out hit) || hit.collider.gameObject.name != "AgentMesh")return;
			mPlayer.nav.startNav (hit.point);
		}

		if (Input.GetMouseButtonDown (1))
		{//技能目标选择
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast (ray, out hit))
			{
				Unit u = hit.collider.gameObject.GetComponent<Unit> ();
				if (u != null)
				{
					mPlayer.skill.targetPos  = u.pos;
					mPlayer.skill.targetUnit = u;
				}
				else if(hit.collider.gameObject.name == "AgentMesh")
				{
					mPlayer.skill.targetPos = hit.point;
				}
				mPlayer.forward (mPlayer.skill.targetPos);
				mPlayer.addState (0, true);
			}
		}

		if (Input.GetKeyDown (KeyCode.F1)) {
			mPlayer.skill.playIndex(0);
		}
		if (Input.GetKeyDown (KeyCode.F2)) {
			mPlayer.skill.playIndex(1);
		}
		if (Input.GetKeyDown (KeyCode.F3)) {
			mPlayer.skill.playIndex(2);
		}
		if (Input.GetKeyDown (KeyCode.F4)) {
			mPlayer.skill.playIndex(3);
		}
	}

	void onLogin(EventMgr.EventData ed)
	{
		MsgReqEnterBattle m = new MsgReqEnterBattle ();
		m.sceneID = 1001;
		NetMgr.client.sendMsg ((short)MyMsgId.ReqEnterBattle, m);
	}

	void onPlayer(EventMgr.EventData ed)
	{
		mPlayer = ed.data as Player;
	}


	string unitId="1001";
	void OnGUI()
	{
		GUI.color = Color.gray;
		if (mPlayer == null)
		{
			if (GUI.Button (new Rect (0, 0, 100, 30), "开始游戏"))
			{
				NetMgr.single.createLanClient ();
				NetMgr.client.connet ("127.0.0.1", 5003);
			}
		}
		else
		{
			if (GUI.Button (new Rect (0, 0, 100, 30), "退出游戏"))
			{
				NetMgr.single.destroyLanClient();
				mPlayer = null;
			}
			if (GUI.Button (new Rect (100, 0, 100, 30), "重载配表"))
			{
				string tableName = Selection.activeObject.name;
				System.Type t = System.Type.GetType ("Arale.Engine." + tableName);
				if (t == null)
				{
					Debug.LogError ("请选中要重加载的配表文件");
					return;
				}
				TableMgr.single.ReloadData (t);
			}
			if (GUI.Button (new Rect (200, 0, 100, 30), "重载Lua"))
			{
				LuaRoot.single.Init ();
			}
			unitId = GUI.TextField (new Rect (100, 30, 100, 30), unitId);
			if (GUI.Button (new Rect (0, 30, 100, 30), "创建敌人"))
			{
				NetMgr.server.createMonster (int.Parse(unitId), Vector3.zero, Vector3.forward);
			}
			if (GUI.Button (new Rect (0, 60, 100, 30), "创建玩家"))
			{
				NetMgr.server.createPlayer (int.Parse(unitId), Vector3.zero, Vector3.forward);
			}
		}
	}
}
