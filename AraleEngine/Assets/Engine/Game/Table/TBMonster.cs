using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.IO;
using Arale.Engine;


public partial class TBMonster: TableBase
{
    public string model="";
	public float  speed=0f;
    public string ai="";
	public string skills="";
	public int aggression=0;

    public override void Init(string[] value)
    {
        base.Init(value);

        model = value[1];
        model = model.Replace("\\n", "\n");
        ai = value[2];
        ai = ai.Replace("\\n", "\n"); 
    }
}
