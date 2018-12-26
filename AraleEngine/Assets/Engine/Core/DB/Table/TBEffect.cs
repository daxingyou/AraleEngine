using UnityEngine;
using System.Collections;

namespace Arale.Engine
{
	public class TBEffect: TableBase
	{
		public string model = "";
		public int    move  = 0;
		public float  life = 0f;
		public string  srcMount="";
		public Vector3 srcPos=Vector3.zero;
		public Vector3 srcDir=Vector3.forward;
		public override void Init(string[] value)
		{
		}
	}

}
