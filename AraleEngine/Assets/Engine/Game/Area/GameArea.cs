using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;


public interface IArea
{
	float R{ get;}
    GameArea.AreaType type{ get;}
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
    public GameArea.AreaType type{ get{return GameArea.AreaType.circle;}}

	public bool inArea(Vector3 pos)
	{
		return pos.sqrMagnitude <= r * r;
	}

	public void debugDraw()
	{
		#if UNITY_EDITOR
		UnityEditor.Handles.DrawWireDisc (Vector3.zero, Vector3.up, r);
		#endif
	}

	public void inspDraw()
	{
		#if UNITY_EDITOR
		r = EditorGUILayout.FloatField ("半径", r);
		#endif
	}

	public string toString()
	{
        return string.Format ("{0},{1:F2}", (int)type,r);
	}

	public void fromString(string s)
	{
		r = float.Parse (s);
	}
}

public class SquareArea : IArea {
	public float inR = 3;
	public float R{get{return inR;}}
    public GameArea.AreaType type{ get{return GameArea.AreaType.Square;}}

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
		#if UNITY_EDITOR
		UnityEditor.Handles.DrawPolyLine (vs);
		#endif
	}

	public void inspDraw()
	{
		#if UNITY_EDITOR
		inR = EditorGUILayout.FloatField ("内切圆半径", inR);
		#endif
	}

	public string toString()
	{
        return string.Format ("{0},{1:F2}", (int)type,inR);
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
    public GameArea.AreaType type{ get{return GameArea.AreaType.Rectangle;}}

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
		#if UNITY_EDITOR
		UnityEditor.Handles.DrawPolyLine (vs);
		#endif
	}

	public void inspDraw()
	{
		#if UNITY_EDITOR
		w = EditorGUILayout.FloatField ("径宽", w);
		l = EditorGUILayout.FloatField ("径深", l);
		#endif
	}

	public string toString()
	{
        return string.Format ("{0},{1:F2},{2:F2}", (int)type,w, l);
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
    public GameArea.AreaType type{ get{return GameArea.AreaType.Fan;}}

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
		#if UNITY_EDITOR
		UnityEditor.Handles.DrawWireArc(pos, Vector3.up, right, ang, r);
		UnityEditor.Handles.DrawLine (pos, pos + left * (r+1));
		UnityEditor.Handles.DrawLine (pos, pos + right * r);
		#endif
	}

	public void inspDraw()
	{
		#if UNITY_EDITOR
		r = EditorGUILayout.FloatField ("半径", r);
		ang = EditorGUILayout.FloatField ("夹角", ang);
		#endif
	}

	public string toString()
	{
        return string.Format ("{0},{1:F2},{2:F2}", (int)type, r, ang);
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
        None=int.MaxValue,
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
		return mArea.toString ();
	}
		
	public AreaType mType;
	public IArea    mArea = new CircleArea();
	#if UNITY_EDITOR
	void OnDrawGizmosSelected()
	{
		if (mArea == null)return;
        UnityEditor.Handles.color = Color.red;
		Matrix4x4 m = UnityEditor.Handles.matrix;
		UnityEditor.Handles.matrix = Matrix4x4.TRS (transform.position, Quaternion.LookRotation (transform.forward), Vector3.one);
		mArea.debugDraw ();
		UnityEditor.Handles.matrix = m;
	}
	#endif
}
