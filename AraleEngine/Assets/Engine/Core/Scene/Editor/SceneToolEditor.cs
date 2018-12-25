using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(SceneTool))]
public class SceneToolEditor : Editor
{
	SceneTool mTarget;
	void OnEnable()
	{
		mTarget = (SceneTool)target;
	}

	public override void OnInspectorGUI(){		
		base.OnInspectorGUI ();
	}

	void OnSceneGUI()
	{
		if(!mTarget.enabled)return;
		mTarget.transform.position = Vector3.zero;
		mTarget.transform.rotation = Quaternion.identity;
		Handles.BeginGUI ();
		if (GUI.Button(new Rect(0, 0, 100, 30), "创建刷怪点"))
		{
			GameObject go = new GameObject ("BornPoint");
			go.transform.SetParent (mTarget.transform, false);
			go.AddComponent<ActorCreator> ();
		}
		if (GUI.Button(new Rect(0, 40, 100, 30), "导出配置"))
		{
            mTarget.ExportXml ();
			SetDirty ();
		}
		Handles.EndGUI ();
	}
}
