﻿using UnityEngine;
using System.Collections;
using Arale.Engine;

public class TestCamera : GRoot {
    protected override void GameStart(){}
    protected override void GameExit(){}
    protected override void GameUpdate(){}
	string camName = "";
	void OnGUI()
	{
		int ox = 0;
		int oy = 30;
		camName = GUILayout.TextField (camName, GUILayout.Width(100));
		if (GUI.Button(new Rect(ox, oy, 100, 30), "CreateCamera"))
		{
			CameraMgr.single.CreateCamera (camName, Vector3.zero, Vector3.forward);
		}
	}
}
