using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;


public interface IArea
{
	float R{ get;}
	bool inArea (Vector3 pos);
	void debugDraw ();
	void inspDraw();
	string toString();
	void   fromString (string s);
}

public class CircleArea : IArea
{
	public float r = 3;
	public float R{get{return r;}}

	public bool inArea(Vector3 pos)
	{
		return pos.sqrMagnitude <= r * r;
	}

	public void debugDraw()
	{
		UnityEditor.Handles.DrawWireDisc (Vector3.zero, Vector3.up, r);
	}

	public void inspDraw()
	{
		r = EditorGUILayout.FloatField ("半径", r);
	}

	public string toString()
	{
		return string.Format ("{0:F2}", r);
	}

	public void fromString(string s)
	{
		r = float.Parse (s);
	}
}

public class SquareArea : IArea {
	public float inR = 3;
	public float R{get{return inR;}}
	public bool inArea(Vector3 pos)
	{
		return pos.x<inR&&pos.x>-inR&&pos.z<inR&&pos.z>-inR;
	}

	public void debugDraw()
	{
		Vector3[] vs = new Vector3[5];
		vs [0] = new Vector3 (inR, 0, inR);
		vs [1] = new Vector3 (inR, 0, -inR);
		vs [2] = new Vector3 (-inR, 0, -inR);
		vs [3] = new Vector3 (-inR, 0, inR);
		vs [4] = new Vector3 (inR, 0, inR);
		UnityEditor.Handles.DrawPolyLine (vs);
	}

	public void inspDraw()
	{
		inR = EditorGUILayout.FloatField ("内切圆半径", inR);
	}

	public string toString()
	{
		return string.Format ("{0:F2}", inR);
	}

	public void fromString(string s)
	{
		inR = float.Parse (s);
	}
}

public class RectangleArea : IArea {
	public float w = 3;
	public float l = 6;
	public float R{get{return l;}}
	public bool inArea(Vector3 pos)
	{
		return pos.x<w/2&&pos.x>-w/2&&pos.z<l&&pos.z>0;
	}

	public void debugDraw()
	{
		Vector3[] vs = new Vector3[5];
		vs [0] = new Vector3 (w/2, 0, 0);
		vs [1] = new Vector3 (-w/2, 0, 0);
		vs [2] = new Vector3 (-w/2, 0, l);
		vs [3] = new Vector3 (w/2, 0, l);
		vs [4] = new Vector3 (w/2, 0, 0);
		UnityEditor.Handles.DrawPolyLine (vs);
	}

	public void inspDraw()
	{
		w = EditorGUILayout.FloatField ("径宽", w);
		l = EditorGUILayout.FloatField ("径深", l);
	}

	public string toString()
	{
		return string.Format ("{0:F2},{1:F2}", w, l);
	}

	public void fromString(string s)
	{
		string[] ss = s.Split (',');
		w = float.Parse (ss [0]);
		l = float.Parse (ss [1]);
	}
}

public class FanArea : IArea {
	public float r = 3;
	public float ang = 60;
	public float R{get{return r;}}
	public bool inArea(Vector3 pos)
	{
		if (pos.sqrMagnitude > r * r)return false;
		Vector3 left  = Quaternion.AngleAxis (ang/2, Vector3.up) * Vector3.forward;
		Vector3 right = Quaternion.AngleAxis (-ang/2, Vector3.up) * Vector3.forward;
		float jj = Vector3.Angle (right, pos);
		Vector3 n1 = Vector3.Cross (right, left);
		Vector3 n2 = Vector3.Cross (right, pos);
		return ang <= 180 ? Vector3.Dot (n1, n2) > 0 && jj < ang : !(Vector3.Dot (n1, n2) > 0 && jj < (360 - ang));
	}	

	public void debugDraw()
	{
		Vector3 pos = Vector3.zero;
		Vector3 dir = Vector3.forward;
		Vector3 left  = Quaternion.AngleAxis (ang/2, Vector3.up) * dir;
		Vector3 right = Quaternion.AngleAxis (-ang/2, Vector3.up) * dir;
		UnityEditor.Handles.DrawWireArc(pos, Vector3.up, right, ang, r);
		UnityEditor.Handles.DrawLine (pos, pos + left * (r+1));
		UnityEditor.Handles.DrawLine (pos, pos + right * r);
	}

	public void inspDraw()
	{
		r = EditorGUILayout.FloatField ("半径", r);
		ang = EditorGUILayout.FloatField ("夹角", ang);
	}

	public string toString()
	{
		return string.Format ("{0:F2},{1:F2}", r, ang);
	}

	public void fromString(string s)
	{
		string[] ss = s.Split (',');
		r = float.Parse (ss [0]);
		ang = float.Parse (ss [1]);
	}
}

public class GameArea : MonoBehaviour {
	public enum AreaType
	{
		circle,     //圆形区域
		Square,     //矩形区域
		Rectangle,  //长方形区域
		Fan,        //扇形区域
	}

	public static IArea ceateArea(AreaType type)
	{
		switch (type)
		{
		case AreaType.circle:
			return new CircleArea ();
		case AreaType.Square:
			return new SquareArea ();
		case AreaType.Rectangle:
			return new RectangleArea();
		case AreaType.Fan:
			return new FanArea ();
		}
		return null;
	}

	public static IArea fromString(string s)
	{
		int i = s.IndexOf (',');
		AreaType t = (AreaType)int.Parse (s.Remove (i));
		IArea area = GameArea.ceateArea (t);
		Debug.Assert (area != null);
		area.fromString (s.Substring (i + 1));
		return area;
	}

	public string toString()
	{
		return string.Format("{0},{1}", (int)mType, mArea.toString ());
	}
		
	public AreaType mType;
	public IArea    mArea = new CircleArea();
	#if UNITY_EDITOR
	void OnDrawGizmosSelected()
	{
		if (mArea == null)return;
		UnityEditor.Handles.color = Color.gray;
		Matrix4x4 m = UnityEditor.Handles.matrix;
		UnityEditor.Handles.matrix = Matrix4x4.TRS (transform.position, Quaternion.LookRotation (transform.forward), Vector3.one);
		mArea.debugDraw ();
		UnityEditor.Handles.matrix = m;
	}
	#endif
}
