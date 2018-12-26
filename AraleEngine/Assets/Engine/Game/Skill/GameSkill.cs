using UnityEngine;
using System.Collections;

public class GameSkill : MonoBehaviour
{
	public class Seg
	{
		public float  delay;
		public string anim;
		public bool   loop;
		public string area;
	}

	public class effect
	{
		bool   skillable;
		bool   breakable;
		bool   runable;
		Attr[] attr;
	}
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
