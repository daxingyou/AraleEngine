using UnityEngine;
using System.Collections;

namespace Arale.Engine
{

	public class TBMove: TableBase
    {
        [TableField("1:pos,2:dir,3:trace,4:jump,5:physic")]
		public int    type = 0;
		public float  speed= 0;
		public float  life = 0;
		public string param= "";
		public override void Init(string[] value)
		{
		}
	}

}

