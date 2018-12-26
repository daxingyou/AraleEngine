using UnityEngine;
using System.Collections;

namespace Arale.Engine
{
    
public class TBBuff: TableBase
{
    public int    type = 0;
	public int    param= 0;
	public int    kind=0;//buff分类掩码,>16为增益buff
	public int    mutex=0;
	public int priority=0;//buff优先级
	public string flag;
    public string lua="";
    public override void Init(string[] value)
    {
    }
}

}
