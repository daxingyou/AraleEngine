using UnityEngine;
using System.Collections;
using Arale.Engine;


public class TestWindow : GRoot
{
	static int WinID = 0;
    protected override void gameStart()
    {
        WindowMgr.SetWindowRes ("TestWindow", "UI/TestWindow");
    }
    protected override void gameExit()
    {
    }
    protected override void gameUpdate()
    {
    }

	void OnGUI()
	{
		int ox = 0;
		int oy = 30;
		if (GUI.Button(new Rect(ox, oy, 100, 30), "Create"))
		{
			Window win = WindowMgr.single.GetWindow ("TestWindow", true, WinID.ToString());
			++WinID;
		}
	}
}
