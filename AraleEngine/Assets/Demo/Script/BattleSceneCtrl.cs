using UnityEngine;
using System.Collections;
using Arale.Engine;

public class BattleSceneCtrl : SceneCtrl {

	// Use this for initialization
	void Start () {
        MsgReqEnterBattle msg = new MsgReqEnterBattle();
        msg.sceneID = 1;
        NetMgr.client.sendMsg((short)MyMsgId.ReqEnterBattle, msg);
        Camera cam = CameraMgr.single.GetCamera("MainCamera");
	}
	
    void OnDestroy(){
    }
	// Update is called once per frame
	void Update () {
	
	}
}
