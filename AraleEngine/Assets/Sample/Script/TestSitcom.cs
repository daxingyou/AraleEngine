using UnityEngine;
using System.Collections;
using Arale.Engine;


public class TestSitcom : GRoot
{
	public TextAsset mSitcomFile;
    protected override void GameStart(){
    }
    protected override void GameExit(){
    }
    protected override void GameUpdate(){
    }
	
	void OnGUI()
	{
		int ox = 0;
		int oy = 0;
		string s = SitcomSystem.single.isPlaying?"停止":"播放";
		if (GUI.Button (new Rect (ox, oy, 100, 30), s))
		{
			if (mSitcomFile == null) {
				Debug.LogError ("设置你要执行的脚本");
				return;
			}
			if (SitcomSystem.single.isPlaying) {
				SitcomSystem.single.Stop ();
				return;
			}
			string path = GHelper.GetLoadPathFromAssetObject (mSitcomFile);
			SitcomSystem.single.Play (path, onSitcomComplete);
		}
	}

	void onSitcomComplete ()
	{
		Debug.Log ("sitcom over");
	}
}
