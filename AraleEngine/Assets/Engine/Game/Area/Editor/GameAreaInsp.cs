using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(GameArea), true)]
public class GameAreaInsp : Editor {
	public override void OnInspectorGUI()
	{
		GameArea ga = (GameArea)target;
		GameArea.AreaType nt = (GameArea.AreaType)EditorGUILayout.EnumPopup ("区域类型", ga.mType);
		if (ga.mType != nt)
		{
			ga.mType = nt;
			ga.mArea = GameArea.ceateArea (ga.mType);
		}
		ga.mArea.inspDraw ();
		EditorGUILayout.TextField ("序列化",ga.toString ());
		EditorUtility.SetDirty (ga);
	}
}
