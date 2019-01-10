﻿using System;
using System.IO;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using XLua;

namespace Arale.Engine
{

public class TableBase
{
    public int _id;
    public int id { get { return _id; } }
	public string _extend;
    public TableBase()
    {
		
    }
    public virtual void Init(string[] value)
    {
		int.TryParse(value[0], out _id);
    }
}

}