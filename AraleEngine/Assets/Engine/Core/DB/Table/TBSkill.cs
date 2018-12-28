using UnityEngine;
using System.Collections;

namespace Arale.Engine
{
    
public class TBSkill: TableBase
{
    public int    type =0;
    public int    skillBuff = 0;
    public int    distance = 0;
    public int    cd = 0;
	public string icon="";
    public override void Init(string[] value)
    {
    }
}

}
