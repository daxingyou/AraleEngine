using UnityEngine;
using System.Collections;

public class RayTools
{
	public static bool intersectTriangle(Ray r, Vector3 v1, Vector3 v2, Vector3 v3)
	{
		Plane p = new Plane (v1, v2, v3);
		Vector3 n = p.normal;
		float d = 0;
		if (p.Raycast (r, out d)) {
			Vector3 v = r.GetPoint (d);
			Vector3 v12 = v2 - v1;
			Vector3 v23 = v3 - v2;
			Vector3 v31 = v1 - v3;
			bool left = leftSide (v12, v - v1, n);
			if (left) {
				return leftSide (v23, v - v2, n)&&leftSide (v31, v - v3, n);
			} else {
				return (!leftSide (v23, v - v2, n))&&(!leftSide (v31, v - v3, n));
			}
		}
		return false;
	}

	//凸多边形有效
	public static bool intersectQuad(Ray r, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
	{
		Plane p = new Plane (v1, v2, v3);
		Vector3 n = p.normal;
		float d = 0;
		if (p.Raycast (r, out d)) {
			Vector3 v = r.GetPoint (d);
			Vector3 v12 = v2 - v1;
			Vector3 v23 = v3 - v2;
			Vector3 v34 = v4 - v3;
			Vector3 v41 = v1 - v4;
			bool left = leftSide (v12, v - v1, n);
			if (left) {
				return leftSide (v23, v - v2, n)&&leftSide (v34, v - v3, n)&&leftSide (v41, v - v4, n);
			} else {
				return (!leftSide (v23, v - v2, n))&&(!leftSide (v34, v - v3, n))&&(!leftSide (v41, v - v4, n));
			}
		}
		return false;
	}

	//向量v2在向量左边,n为v1->v2平面的法线
	static bool leftSide(Vector3 v1, Vector3 v2, Vector3 n)
	{
		Vector3 nr = Vector3.Cross (v2, v1);
		return Vector3.Dot (nr, n) > 0;
	}

	public static float intersectSphere(Ray r, Vector3 center, float radius)
	{
		Vector3 raydir = r.direction;
		Vector3 rayorig = r.origin - center;
		if (rayorig.sqrMagnitude <= radius*radius)
		{//射线原点在球内
			return 0;
		}

		// Mmm, quadratics
		// Build coeffs which can be used with std quadratic solver
		// ie t = (-b +/- sqrt(b*b + 4ac)) / 2a
		float a = Vector3.Dot(raydir,raydir);
		float b = 2 * Vector3.Dot (rayorig, raydir);
		float c = Vector3.Dot(rayorig,rayorig) - radius*radius;

		// Calc determinant
		float d = (b*b) - (4 * a * c);
		if (d < 0)
		{
			return 0;
		}
		else
		{
			// BTW, if d=0 there is one intersection, if d > 0 there are 2
			// But we only want the closest one, so that's ok, just use the 
			// '-' version of the solver
			float t = ( -b - Mathf.Sqrt(d) ) / (2 * a);
			if (t < 0)t = ( -b + Mathf.Sqrt(d) ) / (2 * a);
			return t;
		}
	}
}
