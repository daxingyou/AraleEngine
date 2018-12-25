using UnityEngine;
using System.Collections;

public class TCtrl : MonoBehaviour {
	const float CtrlScreenSize = 100;
	public enum SelType 
	{
		None,
		X,
		Y,
		Z,
		XY,
		XZ,
		YZ,
		XYZ,
		N,
	}

	public SelType   mSel;
	public float     mR=100;

	public Transform mTarget;
	protected Camera mCam;
	protected Material mMat;
	void Start()
	{
		mMat = new Material (Shader.Find ("Unlit/TCtrlShader"));
		mCam = GetComponent<Camera> ();
	}

	protected void Update()
	{
		if (mTarget == null || mCam == null)
			return;
		Vector3 v1 = mTarget.position;
		Vector3 v2 = mTarget.position + new Vector3 (1, 0, 0);
		v1 = mCam.WorldToScreenPoint (v1);
		v2 = mCam.WorldToScreenPoint (v2);
		mR = CtrlScreenSize/(v1 - v2).magnitude;
	}

	void getDistance(Ray ray, Vector3 start, Vector3 end)
	{
		
	}
}
