using UnityEngine;
using System.Collections;

public class TestCollide : MonoBehaviour {
	public GameObject target;
	int collideType = 0;
	// Use this for initialization
	void OnGUI () {
		int ox = 0;
		int oy = 0;
		if (GUI.Button (new Rect (ox, oy, 200, 30), "Physics.OverlapBox"))collideType = 0;
		if (GUI.Button (new Rect (ox, oy+30, 200, 30), "Physics.BoxCastAll"))collideType = 1;
	}
	
	// Update is called once per frame
	void Update () {
		if (target == null)return;
		MeshRenderer mr = target.GetComponent<MeshRenderer> ();
		mr.material.color = testHit(target.GetComponent<Collider>())?Color.red:Color.blue;
	}

	bool testHit(Collider c)
	{
		switch(collideType)
		{
		case 0:
			return collide (c);
		case 1:
			return castCollide (c);
		}
		return false;
	}

	bool collide(Collider c)
	{
		BoxCollider bc = GetComponent<BoxCollider> ();
		//return bc.bounds.Intersects (c.bounds);//bc.bounds是正坐标系坐标，是一个正包围盒，所以旋转后碰撞是错误的
		Collider[] cs = Physics.OverlapBox(bc.center+bc.transform.position, bc.size/2, bc.transform.rotation);
		for (int i = 0; i < cs.Length; ++i)//bc.center如果不是Vector3.zero，旋转也有错
		{
			if (object.ReferenceEquals (cs [i], c))return true;
		}
		return false;
	}

	bool castCollide(Collider c)
	{
		BoxCollider bc = GetComponent<BoxCollider> ();
		Vector3 center = bc.center + bc.transform.position;
		Vector3 screenPos = Camera.main.WorldToScreenPoint (center);
		Ray ray = Camera.main.ScreenPointToRay (screenPos);
		RaycastHit[] hits = Physics.BoxCastAll (center, bc.size / 2, ray.direction, bc.transform.rotation, 1000);
		for (int i = 0; i < hits.Length; ++i)
		{
			if (object.ReferenceEquals (hits[i].transform, c.transform))return true;
		}
		return false;
	}
}
