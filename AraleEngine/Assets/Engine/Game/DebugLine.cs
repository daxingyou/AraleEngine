using UnityEngine;
using System.Collections;

public class DebugLine
{
	public static void drawCircle(Vector3 pos, float r, Color clr, float yOffset=0.01f)
	{
		Matrix4x4 oldMat = Gizmos.matrix;
		Gizmos.matrix = Matrix4x4.TRS (pos, Quaternion.identity, Vector3.one);
		Color oldClr = Gizmos.color;
		Gizmos.color = clr;
		Vector3 begin = new Vector3(r*Mathf.Cos (0), yOffset, r*Mathf.Sin(0));
		Vector3 end   = new Vector3(0,yOffset,0);
		for (int i = 1, max = 24; i <= max; ++i)
		{
			end.x = r * Mathf.Cos (Mathf.PI*i/12);
			end.z = r * Mathf.Sin (Mathf.PI*i/12);
			Gizmos.DrawLine (begin, end);
			begin = end;
		}
		Gizmos.color = oldClr;
		Gizmos.matrix = oldMat;
	}

	public static void drawLines(Vector3[] points, Color clr, float yOffset=0.01f)
	{
		Color oldClr = Gizmos.color;
		Gizmos.color = clr;
		for (int i = 0, max = points.Length-1; i < max; ++i)
		{
			Gizmos.DrawLine (points[i], points[i+1]);
		}
		Gizmos.color = oldClr;
	}
}
