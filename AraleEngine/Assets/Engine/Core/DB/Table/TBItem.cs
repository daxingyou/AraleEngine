using UnityEngine;
using System.Collections;

namespace Arale.Engine
{

	public class TBItem: TableBase
	{
		public int    type = 0;
		public string dropModel="";
		public float  dropRate=0f;//掉落率
		public override void Init(string[] value)
		{
		}
	}

}
