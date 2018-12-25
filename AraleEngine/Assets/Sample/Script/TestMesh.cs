using UnityEngine;
using System.Collections;
using Arale.Engine;

public class TestMesh : MonoBehaviour {

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {

	}

	void OnGUI() {
		float y = 0;
		if(GUI.Button(new Rect(0,y,120,30),"create mesh"))
		{
			MeshFilter mf = gameObject.GetComponent<MeshFilter> ();
			mf.mesh = MeshTools.createFromHMap (null, 5, 5, 10, 10, 0);
		}
		if(GUI.Button(new Rect(0,y+=30,120,30),"combine mesh"))
		{
			MeshFilter mf = gameObject.GetComponent<MeshFilter> ();
			MeshTools.combine (transform);
		}
		if(GUI.Button(new Rect(0,y+=30,120,30),"weld mesh"))
		{
			MeshFilter mf = gameObject.GetComponent<MeshFilter> ();
			MeshTools.weld (transform,1f,true);
		}
		if(GUI.Button(new Rect(0,y+=30,120,30),"split mesh"))
		{
			MeshFilter mf = gameObject.GetComponent<MeshFilter> ();
			MeshFilter splitplane = GameObject.Find ("splitplane").GetComponent<MeshFilter> ();
			Vector3[] v = splitplane.mesh.vertices;
			int[] tri = splitplane.mesh.triangles;
			Matrix4x4 m = gameObject.transform.worldToLocalMatrix*splitplane.transform.localToWorldMatrix;
			mf.mesh = MeshTools.split (mf.mesh, new Plane(m.MultiplyPoint(v[tri[0]]), m.MultiplyPoint(v[tri[1]]), m.MultiplyPoint(v[tri[2]])));
		}
		if(GUI.Button(new Rect(0,y+=30,120,30),"sub polytope"))
		{
			MeshFilter mf = gameObject.GetComponent<MeshFilter> ();
			mf.mesh = MeshTools.subdivision (mf.mesh);
		}
	}
}
