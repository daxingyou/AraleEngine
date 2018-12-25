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
        public string model="";
        public string btPath="";

        public override void Init(string[] value)
        {
            base.Init(value);

            model = value[1];
            model = model.Replace("\\n", "\n");
            btPath = value[2];
            btPath = btPath.Replace("\\n", "\n"); 
        }
    }

}
