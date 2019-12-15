using UnityEngine;
using System.Collections;
using Arale.Engine;

  
public partial class TBBuff: TableBase
{
    public int    type = 0;
	public int    param= 0;
	public int    kind=0;//buff分类掩码,>16为增益buff
	public int    mutex=0;//buff互斥
	public int priority=0;//buff优先级,值越大优先级越高
	public string flag="";
    public string lua="";
    public override void Init(string[] value)
    {
    }
}
