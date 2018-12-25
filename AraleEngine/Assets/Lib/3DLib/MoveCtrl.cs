using UnityEngine;
using System.Collections;

public class MoveCtrl : TCtrl
{
	void OnPostRender()
	{
		if (!mMat||!mTarget)return;
		mMat.SetPass (1);
		Matrix4x4 m = Matrix4x4.TRS(mTarget.position, mTarget.localRotation, Vector3.one);

		GL.PushMatrix ();
		GL.MultMatrix(m*Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0,0,0), Vector3.one));
		drawArraw (mSel==SelType.Z?Color.yellow:Color.blue);
		GL.MultMatrix(m*Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(-90,0,0), Vector3.one));
		drawArraw (mSel==SelType.Y?Color.yellow:Color.green);
		GL.MultMatrix(m*Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0,90,0), Vector3.one));
		drawArraw (mSel==SelType.X?Color.yellow:Color.red);

		GL.MultMatrix(m*Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0,0,0), Vector3.one));
		drawQuad (mSel==SelType.YZ?Color.yellow:Color.red);
		GL.MultMatrix(m*Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0,0,-90), Vector3.one));
		drawQuad (mSel==SelType.XZ?Color.yellow:Color.green);
		GL.MultMatrix(m*Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0,90,0), Vector3.one));
		drawQuad (mSel==SelType.XY?Color.yellow:Color.blue);
		GL.PopMatrix ();
	}
		
	void drawArraw(Color clr)
	{
		//画线
		GL.Begin (GL.LINES);
		GL.Color (clr);//只有放在begin,end之间有效
		GL.Vertex3 (0, 0, 0);
		GL.Vertex3 (0, 0, mR);
		GL.End ();
		//画箭头
		GL.Begin (GL.TRIANGLES);
		GL.Color (clr);
		float r = 0.1f * mR;
		float x1 = 0;
		float y1 = r;
		for (float i = 1; i <= 360; i += 10)
		{
			GL.Vertex3 (0, 0,   mR);
			GL.Vertex3 (x1, y1, mR);
			float x2 = r*Mathf.Cos(i/180*Mathf.PI);
			float y2 = r*Mathf.Sin(i/180*Mathf.PI);
			GL.Vertex3 (x2, y2, mR);
			GL.Vertex3 (0, 0,   mR+3*r);
			GL.Vertex3 (x2, y2, mR);
			GL.Vertex3 (x1, y1, mR);
			x1 = x2;
			y1 = y2;
		}
		GL.End ();
	}

	void drawQuad(Color clr)
	{
		GL.Begin (GL.LINES);
		GL.Color (clr);
		GL.Vertex3 (0, 0.3f*mR, 0);
		GL.Vertex3 (0, 0.3f*mR, 0.3f*mR);
		GL.Vertex3 (0, 0.3f*mR, 0.3f*mR);
		GL.Vertex3 (0, 0, 0.3f*mR);
		GL.End ();

		GL.Begin (GL.TRIANGLE_STRIP);
		clr.a = 0.3f;
		GL.Color (clr);
		GL.Vertex3 (0, 0.3f*mR, 0);
		GL.Vertex3 (0, 0.3f*mR, 0.3f*mR);
		GL.Vertex3 (0, 0, 0);
		GL.Vertex3 (0, 0, 0.3f*mR);
		GL.End ();
	}

	void Update()
	{
		base.Update ();
		mSel = SelType.None;
		if (mCam==null || !Input.GetMouseButton(0))return;
		Ray ray = mCam.ScreenPointToRay(Input.mousePosition);
		Matrix4x4 m = Matrix4x4.TRS(mTarget.position, mTarget.localRotation, Vector3.one);
		Vector3[] vs = new Vector3[]{ new Vector3 (0, 0, 0), new Vector3 (mR, 0, 0), new Vector3 (0, mR, 0), new Vector3 (0, 0, mR) };
		for(int i=0;i<4;++i)vs[i] = m.MultiplyPoint(vs[i]); 
		Bounds xbd = new Bounds ((vs [0] + vs [1]) / 2, m.MultiplyVector(new Vector3(mR, mR/10, mR/10)));
		Bounds ybd = new Bounds ((vs [0] + vs [2]) / 2, m.MultiplyVector(new Vector3(mR/10, mR, mR/10)));
		Bounds zbd = new Bounds ((vs [0] + vs [3]) / 2, m.MultiplyVector(new Vector3(mR/10, mR/10, mR)));
		if (xbd.IntersectRay (ray))
			mSel = SelType.X;
		if (ybd.IntersectRay (ray))
			mSel = SelType.Y;
		if (zbd.IntersectRay (ray))
			mSel = SelType.Z;
		
		if (RayTools.intersectQuad(ray, m.MultiplyPoint(new Vector3(0,0,0)),  m.MultiplyPoint(new Vector3(0.3f*mR,0,0)), m.MultiplyPoint(new Vector3(0.3f*mR,0.3f*mR,0)), m.MultiplyPoint(new Vector3(0,0.3f*mR,0))))
			mSel =SelType.XY;
		if (RayTools.intersectQuad(ray, m.MultiplyPoint(new Vector3(0,0,0)),  m.MultiplyPoint(new Vector3(0.3f*mR,0,0)), m.MultiplyPoint(new Vector3(0.3f*mR,0,0.3f*mR)), m.MultiplyPoint(new Vector3(0,0,0.3f*mR))))
			mSel =SelType.XZ;
		if (RayTools.intersectQuad(ray, m.MultiplyPoint(new Vector3(0,0,0)),  m.MultiplyPoint(new Vector3(0,0.3f*mR,0)), m.MultiplyPoint(new Vector3(0,0.3f*mR,0.3f*mR)), m.MultiplyPoint(new Vector3(0,0,0.3f*mR))))
			mSel =SelType.YZ;

	}
}
