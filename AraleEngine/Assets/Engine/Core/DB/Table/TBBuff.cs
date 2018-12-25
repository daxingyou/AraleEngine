using UnityEngine;
using System.Collections;

namespace Arale.Engine
{
    
public class TBBuff: TableBase
{
    public int    type = 0;
    public float  duration = 0f;
    public string model="";
    public string lua  ="";
    public override void Init(string[] value)
    {
    }
}

}
