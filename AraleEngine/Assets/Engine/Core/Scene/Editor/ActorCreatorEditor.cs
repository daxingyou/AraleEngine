using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(ActorCreator))]
public class ActorCreatorEditor : Editor
{
	GUIStyle style1 = new GUIStyle();
	GUIStyle style2 = new GUIStyle();
	ActorCreator mTarget;
	void OnEnable()
	{
		style1.fontStyle = FontStyle.Bold;
		style1.normal.textColor = Color.white;
		style2.fontStyle = FontStyle.Bold;
		style2.normal.textColor = Color.red;
		mTarget = (ActorCreator)target;
	}

	public override void OnInspectorGUI(){		
		base.OnInspectorGUI ();
	}

	void OnSceneGUI()
	{
		if(!mTarget.enabled)return; 
		Undo.RecordObject(mTarget,"Undo.ActorCreatorEditor");
		for (int i = 0; i < mTarget.mActorInfo.Count; ++i)
		{
			ActorCreator.ActorInfo info = mTarget.mActorInfo [i];
			Handles.Label(info.pos, "ActorID="+info.actorId, style2);
			GUI.SetNextControlName("WHP" + i);
			info.pos = Handles.PositionHandle (info.pos, Quaternion.identity);
		}

		for (int i = 0; i < mTarget.mBornPos.Count; ++i)
		{
			Handles.Label(mTarget.mBornPos[i], i.ToString(), style1);
			GUI.SetNextControlName("WHP" + i+10000);
			mTarget.mBornPos [i] = Handles.PositionHandle (mTarget.mBornPos [i], Quaternion.identity);
		}

		string hotControl = GUI.GetNameOfFocusedControl();
		//Debug.LogError (hotControl);
	}
}
