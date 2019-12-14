using System;
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
        [TableField("Lua表:如{a=1;b=2;}")]
    	public string _extend;
        public TableBase()
        {
    		
        }
        public virtual void Init(string[] value)
        {
    		int.TryParse(value[0], out _id);
        }

        public virtual void read(BinaryReader r, int[] idx)
        {
            Debug.LogError("read not implement:"+this.GetType());
        }
        public virtual void write(BinaryWriter w)
        {
            Debug.LogError("write not implement"+this.GetType());
        }
    }

    public class TableField:Attribute
    {
        public TableField(string desc)
        {
            this.desc = desc;
        }
        public string desc;
    }

}