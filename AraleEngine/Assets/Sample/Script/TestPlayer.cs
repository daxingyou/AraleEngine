using UnityEngine;
using System.Collections;

public class TestPlayer : MonoBehaviour {
	uint playerId = 0;
	void OnGUI()
	{
		#if UNITY_EDITOR
		int ox = 0;
		int oy = 30;
		/*if (GUI.Button(new Rect(ox, oy, 100, 30), "Create"))
		{
            Player player = Player.create (++playerId, 1001);
			player.show (true);
		}

		if (GUI.Button(new Rect(ox, oy+=30, 100, 30), "EnableAI"))
		{
			GameObject go = UnityEditor.Selection.activeGameObject;
			if(go==null)
			{
				Debug.Log("please select play");
				return;
			}
            go.GetComponent<UnitMono>().mUnit.doCmd(Player.CMD_ENABLE_AI, true);
		}

		if (GUI.Button(new Rect(ox, oy+=30, 100, 30), "DisableAI"))
		{
			GameObject go = UnityEditor.Selection.activeGameObject;
			if(go==null)
			{
				Debug.Log("please select play");
				return;
			}
            go.GetComponent<UnitMono>().mUnit.doCmd(Player.CMD_ENABLE_AI, false);
		}*/
		#endif
	}
}
