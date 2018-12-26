using UnityEngine;
using System.Collections;

public class SkillEvent
{
	public struct Attr
	{
		public int    attrId;
		public float attrValue;
	}
	public float  delay;
	public bool   cd;
	public bool   run;
	public string anim;
	public Attr[] attr;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
