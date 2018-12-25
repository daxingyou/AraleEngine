using UnityEngine;
using System.Collections;
using Arale.Engine;

public class TestDebug : MonoBehaviour {
	void Start()
	{
		Log.init ();
	}

	void OnDestroy()
	{
		Log.deinit ();
	}

	void OnGUI()
	{
		int ox = 0;
		int oy = 30;
		Log.mWriteFile = GUI.Toggle (new Rect (ox, oy, 100, 30), Log.mWriteFile, "writeFile");
		Log.mWriteScreen = GUI.Toggle (new Rect (ox, oy+=30, 100, 30), Log.mWriteScreen, "writeScreen");
		Log.mWriteFileImmediate = GUI.Toggle (new Rect (ox, oy+=30, 100, 30), Log.mWriteFileImmediate, "writeFileImmediate");
		Log.mShowTimeStamp = GUI.Toggle (new Rect (ox, oy+=30, 100, 30), Log.mShowTimeStamp, "whowTimeStamp");

		if (GUI.Button(new Rect(ox, oy+=30, 100, 30), "Log.d"))
		{
			Log.d ("this is log");
		}
		if (GUI.Button(new Rect(ox, oy+=30, 100, 30), "Log.i"))
		{
			Log.i ("this is log");
		}
		if (GUI.Button(new Rect(ox, oy+=30, 100, 30), "Log.w"))
		{
			Log.w ("this is log");
		}
		if (GUI.Button(new Rect(ox, oy+=30, 100, 30), "Log.e"))
		{
			Log.e ("this is log");
		}
	}
}
