using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Atlas))]
public class AtlasAssetEditor : Editor
{
	Atlas atlas;
	public void OnEnable()
	{
		atlas = (Atlas)target;
	}

	Vector2 mScroll;
	public override void OnInspectorGUI()
	{
		mScroll = GUILayout.BeginScrollView (mScroll);
		Sprite[] s = atlas._sprites;
		GUILayout.Label ("sprite:");
		for (int i = 0; i < s.Length; ++i)
		{
			GUILayout.Label (s [i].name);
		}
		GUILayout.EndScrollView ();
	}
}
