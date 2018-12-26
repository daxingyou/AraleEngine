using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.IO;


namespace Arale.Engine
{

    public class TBMonster: TableBase
    {
		public string name="";
        public string model="";
		public float  speed=0f;
        public string ai="";
		public string skills="";

        public override void Init(string[] value)
        {
            base.Init(value);

            model = value[1];
            model = model.Replace("\\n", "\n");
            ai = value[2];
            ai = ai.Replace("\\n", "\n"); 
        }
    }

}
