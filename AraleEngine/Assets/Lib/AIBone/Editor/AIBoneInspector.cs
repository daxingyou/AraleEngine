using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(AIBone))]
public class AIBoneInspector : Editor {
	AIBone mAIBone;
	Quaternion mOldQ;
	Vector3    mOldP;
	void OnEnable()
	{
		mAIBone = target as AIBone;
		mOldQ = mAIBone.transform.localRotation;
		mOldP = mAIBone.transform.localPosition;
	}

	void OnDisable()
	{
		if (mAIBone.testDirty) 
		{
			mAIBone.testDirty = false;
			mAIBone.transform.localRotation = mOldQ;
			mAIBone.transform.localPosition = mOldP;
		}
	}

	public override void OnInspectorGUI()
	{
		NGUIEditorTools.BeginContents ();
		EditorGUILayout.BeginHorizontal ();
		if (GUILayout.Button ("设置为根", GUILayout.Width (80)))onRootClick ();
		if (GUILayout.Button ("骨骼分裂", GUILayout.Width (80)))onDivideClick ();
		EditorGUILayout.EndHorizontal ();
		NGUIEditorTools.EndContents ();

		drawLengthGUI ();
		drawRotateGUI ();
		refreshSceneView ();
	}

	void refreshSceneView()
	{
		if(AIBone.mUpdate)EditorUtility.SetDirty (mAIBone);//call AIBone.update veryframe
	}

	void drawLengthGUI()
	{
		NGUIEditorTools.BeginContents ();
		EditorGUILayout.BeginHorizontal ();
		if (GUILayout.Button (mAIBone.length>0?"解锁":"锁定", GUILayout.Width (80)))onLockLengthClick ();
		EditorGUILayout.LabelField ("length", GUILayout.Width(30));
		mAIBone.length = EditorGUILayout.FloatField(mAIBone.length);
		EditorGUILayout.LabelField ("min", GUILayout.Width(30));
		mAIBone.minLength = EditorGUILayout.FloatField(mAIBone.minLength);
		EditorGUILayout.LabelField ("max", GUILayout.Width(30));
		mAIBone.maxLength = EditorGUILayout.FloatField(mAIBone.maxLength);
		mAIBone.length = Mathf.Clamp (mAIBone.length, mAIBone.minLength, mAIBone.maxLength);
		EditorGUILayout.EndHorizontal ();
		NGUIEditorTools.EndContents ();
	}

	void drawRotateGUI()
	{
		NGUIEditorTools.BeginContents ();
		EditorGUILayout.BeginHorizontal ();
		if (GUILayout.Button (mAIBone.xMin == 0 ? "xMin" : mAIBone.xMin.ToString (), GUILayout.Width (80)))mAIBone.xMin = mAIBone.transform.localRotation.eulerAngles.x;
		if (GUILayout.Button (mAIBone.xMax == 0 ? "xMax" : mAIBone.xMax.ToString (), GUILayout.Width (80)))mAIBone.xMax = mAIBone.transform.localRotation.eulerAngles.x;
		EditorGUILayout.EndHorizontal ();
		EditorGUILayout.BeginHorizontal ();
		if (GUILayout.Button (mAIBone.yMin == 0 ? "yMin" : mAIBone.yMin.ToString (), GUILayout.Width (80)))mAIBone.yMin = mAIBone.transform.localRotation.eulerAngles.y;
		if (GUILayout.Button (mAIBone.yMax == 0 ? "yMax" : mAIBone.yMax.ToString (), GUILayout.Width (80)))mAIBone.yMax = mAIBone.transform.localRotation.eulerAngles.y;
		EditorGUILayout.EndHorizontal ();
		EditorGUILayout.BeginHorizontal ();
		if (GUILayout.Button (mAIBone.zMin == 0 ? "zMin" : mAIBone.zMin.ToString (), GUILayout.Width (80)))mAIBone.zMin = mAIBone.transform.localRotation.eulerAngles.z;
		if (GUILayout.Button (mAIBone.zMax == 0 ? "zMax" : mAIBone.zMax.ToString (), GUILayout.Width (80)))mAIBone.zMax = mAIBone.transform.localRotation.eulerAngles.z;
		EditorGUILayout.EndHorizontal ();
		NGUIEditorTools.EndContents ();
	}

	void onRootClick()
	{
		mAIBone.setRoot ();
	}

	void onDivideClick()
	{
		if (mAIBone.isRoot())return;
		GameObject go = new GameObject ("Bone");
		go.AddComponent<AIBone> ();
		go.transform.parent = mAIBone.transform.parent;
		go.transform.localScale = Vector3.one;
		go.transform.position = 0.5f * (mAIBone.transform.position + mAIBone.transform.parent.position);
		mAIBone.transform.parent = go.transform;
	}

	void onLockLengthClick()
	{
		if (mAIBone.isRoot ())
			return;
		if (mAIBone.length > 0)
		{
			mAIBone.length = mAIBone.minLength = mAIBone.maxLength = 0;
		}
		else
		{
			mAIBone.length = (mAIBone.transform.position - mAIBone.transform.parent.position).magnitude;
			mAIBone.minLength = mAIBone.maxLength = mAIBone.length;
		}
	}
}
