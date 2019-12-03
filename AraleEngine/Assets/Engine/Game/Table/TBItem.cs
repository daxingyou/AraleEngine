using UnityEngine;
using System.Collections;

namespace Arale.Engine
{

	public class TBItem: TableBase
	{
		public int    type = 0;
		public string dropModel="";
		public float  dropRate=0f;//掉落率
        public float  dropInterval=0f;//掉落间隔s
		public override void Init(string[] value)
		{
		}
	}

}
