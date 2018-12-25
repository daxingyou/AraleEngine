#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class AIBone : MonoBehaviour {
	public float length;
	public float minLength;
	public float maxLength;
	public float xMin;
	public float xMax;
	public float yMin;
	public float yMax;
	public float zMin;
	public float zMax;

	void Start () {

	}

	void Update () 
	{
		#if UNITY_EDITOR
		testUpdate();
		#endif

		if (length != 0) {
			Vector3 dir = transform.position - transform.parent.position;
			dir.Normalize ();
			transform.position = transform.parent.position + dir * length;
		}
	}

	public Transform getRoot()
	{
		Transform root = transform;
		while (root.parent != null && root.parent.GetComponent<AIBone> () != null) 
		{
			root = root.parent;
		}
		return root;
	}

	public bool isRoot()
	{
		return transform.parent == null || transform.parent.GetComponent<AIBone> () == null;
	}

	public void setRoot()
	{
		Transform root = getRoot ();
		if(Object.ReferenceEquals(root, transform))return;
		Transform pre = transform;
		Transform cur = pre.parent;
		Transform next = cur.parent;
		pre.parent = root.parent;

		do 
		{
			cur.parent = pre;
			if(Object.ReferenceEquals (cur,root))break;
			pre = cur;
			cur = next;
			next = next.parent;
		} while(true);
	}

	public bool isChild(Transform bone)
	{
		if (bone == null)return false;
		Transform t = transform.parent;
		while(t!= null && t.GetComponent<AIBone> () != null)
		{
			if(Object.ReferenceEquals(t, bone))return true;
			t = t.parent;
		}
		return false;
	}

	#if UNITY_EDITOR
	public static bool mUpdate;
	[MenuItem("AIBone/Create")]
	static void createAIBone()
	{
		GameObject sel = Selection.activeGameObject;
		GameObject go = new GameObject ("RootBone");
		go.AddComponent<AIBone> ();
		if (sel != null&& !EditorUtility.IsPersistent(sel))go.transform.parent = sel.transform;
		go.transform.localPosition = Vector3.zero;
		go.transform.localScale = Vector3.one;
		Selection.activeGameObject = go;
	}

	[MenuItem("AIBone/Attach")]
	static void attachAIBone()
	{
		GameObject sel = Selection.activeGameObject;
		if (sel == null || EditorUtility.IsPersistent (sel))
			return;
		attachAIBone (sel.transform,true);
	}

	[MenuItem("AIBone/Play")]
	static void playAIBone()
	{
		mUpdate = true;
	}

	[MenuItem("AIBone/Stop")]
	static void stopAIBone()
	{
		mUpdate = false;
		Selection.activeObject = null;
	}

	static void attachAIBone(Transform bone, bool isRoot=false)
	{
		
		AIBone b = bone.GetComponent<AIBone>();
		if(b==null)b=bone.gameObject.AddComponent<AIBone> ();
		if (!isRoot) 
		{
			b.length = (b.transform.position - b.transform.parent.position).magnitude;
			b.minLength = b.maxLength = b.length;
		}
		for (int i = 0, max = bone.childCount; i < max; ++i) {
			attachAIBone (bone.GetChild (i));
		}
	}

	[MenuItem("AIBone/HideMesh")]
	static void hideMeshi()
	{
		SceneView sv = SceneView.lastActiveSceneView;
		if (sv == null)
			return;
		//sv.camera.cullingMask &= ~LayerMask.NameToLayer ("Default");
		sv.camera.cullingMask = 1;
		sv.camera.transform.position = new Vector3 (10000, 0, 0);
	}

	[DrawGizmo(GizmoType.NonSelected|GizmoType.Selected)]
	static void DrawAllBone(AIBone bone, GizmoType gizmoType)//AIBone改成Transform那么所有的Transform会被调用该函数
	{
		SceneView sv = SceneView.currentDrawingSceneView;
		if (sv == null)//game window
			return;
		bool sel = (gizmoType & GizmoType.Selected) != 0;
		Color oc = Gizmos.color;
		Handles.color = sel?Color.green:Color.white;
		if (bone.isChild (Selection.activeTransform))Handles.color = Color.magenta;
		Transform transform = bone.transform;

		int cc = transform.childCount;
		for (int i = 0; i < cc; ++i)
		{
			Handles.DrawLine (transform.position, transform.GetChild (i).position);
		}

		if (sel)
		{
			Camera c = sv.camera;
			transform.rotation.SetLookRotation (c.transform.position);
			Quaternion q = Quaternion.identity;
			q.SetLookRotation (c.transform.position - transform.position);
			Handles.CircleCap (0, transform.position, q, 0.2f);

			Quaternion handleRotation = Tools.pivotRotation == PivotRotation.Local ? transform.rotation : Quaternion.identity;
			transform.position = Handles.DoPositionHandle (transform.position, handleRotation);
			Handles.Label (transform.position, transform.gameObject.name);
		}
		Handles.color = oc;
	}

	void OnDrawGizmos ()
	{
		//显示系统Gizmos Icon名称
		//string systemGizmosIcon = EditorGUIUtility.ObjectContent (this, typeof(AIBone)).image.name;
		//Debug.LogError (systemGizmosIcon);
		if (isRoot ())
		{
			Gizmos.DrawIcon (transform.position, "sv_icon_dot6_pix16_gizmo");
		}
		else
		{
			if (length > 0)
				Gizmos.DrawIcon (transform.position, "sv_icon_dot12_pix16_gizmo");
			else
				Gizmos.DrawIcon (transform.position, "sv_icon_dot7_pix16_gizmo");
		}
	}

	//---------------------
	[System.NonSerialized]
	public bool testDirty;
	void testUpdate()
	{
		if (!mUpdate)return;
		testDirty = true;
		testRotateUpdate ();
	}

	void testRotateUpdate()
	{
		Transform t = Selection.activeTransform;
		if (t==null||!Object.ReferenceEquals (transform, t))return;
		float k = Mathf.PingPong (Time.realtimeSinceStartup, 5) / 5;
		float xdeg = xMin + k * (xMax - xMin);
		float ydeg = yMin + k * (yMax - yMin);
		float zdeg = zMin + k * (zMax - zMin);
		transform.localRotation = Quaternion.Euler(xdeg,ydeg,zdeg);
	}
	//---------------------
	#endif
}

