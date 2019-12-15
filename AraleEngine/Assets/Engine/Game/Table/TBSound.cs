using UnityEngine;
using System.Collections;
using Arale.Engine;


public partial class TBSound : TableBase
{
    public string asset = "";
    public int    loop = 0;
    public float  duration = 0f;
    public float  volume = 1f;
    public override void Init(string[] value)
    {
    }
}
