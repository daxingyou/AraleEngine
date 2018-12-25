using UnityEngine;
using System.Collections;

public class RotateCtrl : TCtrl {
	void OnPostRender()
	{
		if (!mMat||!mTarget)return;
		mMat.SetPass (0);
		Matrix4x4 m = Matrix4x4.TRS(mTarget.position, mTarget.localRotation, Vector3.one);

		GL.PushMatrix ();
		GL.MultMatrix(m*Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0,0,0), Vector3.one));
		drawCircle(0,0, mR, 0, 2*Mathf.PI, Mathf.PI/180, mSel==SelType.Z?Color.yellow:Color.blue);
		GL.MultMatrix(m*Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90,0,0), Vector3.one));
		drawCircle(0,0, mR, 0, 2*Mathf.PI, Mathf.PI/180, mSel==SelType.Y?Color.yellow:Color.green);
		GL.MultMatrix(m*Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0,90,0), Vector3.one));
		drawCircle(0,0, mR, 0, 2*Mathf.PI, Mathf.PI/180, mSel==SelType.X?Color.yellow:Color.red);

		Quaternion q = Quaternion.FromToRotation (Vector3.forward, -mCam.transform.forward);
		GL.MultMatrix(Matrix4x4.TRS(mTarget.position, q, Vector3.one));
		drawCircle(0,0, 1.1f*mR, 0, 2*Mathf.PI, Mathf.PI/180, mSel==SelType.N?Color.yellow:Color.white);
		drawCircle(0,0, mR, 0, 2*Mathf.PI, Mathf.PI/180, mSel==SelType.XYZ?Color.yellow:Color.white);
		GL.PopMatrix ();
	}

	void drawCircle(float x, float y, float r, float startAng, float endAng, float detalAng, Color clr,  bool fill=false)
	{
		GL.Begin (fill?GL.TRIANGLE_STRIP:GL.LINES);
		GL.Color (clr);
		float x1 = r*Mathf.Cos(startAng);
		float y1 = r*Mathf.Sin(startAng);
		for (float i = startAng+detalAng; i <= endAng; i += detalAng)
		{
			GL.Vertex3 (x1+x, y1+y, 0);
			x1 = r*Mathf.Cos(i);
			y1 = r*Mathf.Sin(i);
			GL.Vertex3 (x1+x, y1+y, 0);
		}
		GL.End ();
	}

	void Update()
	{
		base.Update ();
		//显示前半球
		Vector3 center = mCam.worldToCameraMatrix.MultiplyPoint(mTarget.position);
		mMat.SetVector("_Center", center);
		//=======
		mSel = SelType.None;
		Ray ray = mCam.ScreenPointToRay(Input.mousePosition);
		if (RayTools.intersectSphere(ray, mTarget.position, 1.1f*mR)!=0)
			mSel =SelType.N;

		float d = RayTools.intersectSphere (ray, mTarget.position, mR);
		if (d == 0)return;
		mSel =SelType.XYZ;
		Vector3 p = pos=ray.GetPoint (d);
		Matrix4x4 m = Matrix4x4.TRS(mTarget.position, mTarget.localRotation, Vector3.one);
		Plane x = new Plane (m.MultiplyVector (Vector3.left),m.MultiplyPoint (Vector3.zero));
		Plane y = new Plane (m.MultiplyVector (Vector3.up),m.MultiplyPoint (Vector3.zero));
		Plane z = new Plane (m.MultiplyVector (Vector3.forward),m.MultiplyPoint (Vector3.zero));
		if (Mathf.Abs (x.GetDistanceToPoint (p)) < 0.1f)mSel = SelType.X;
		if (Mathf.Abs (y.GetDistanceToPoint (p)) < 0.1f)mSel = SelType.Y;
		if (Mathf.Abs (z.GetDistanceToPoint (p)) < 0.1f)mSel = SelType.Z;
	}

	Vector3 pos = Vector3.zero;
	void OnDrawGizmos()
	{
		Gizmos.DrawSphere (pos, 0.1f);
	}
}
