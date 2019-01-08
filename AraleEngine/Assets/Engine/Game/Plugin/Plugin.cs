using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Plugin
{
    protected Unit mUnit;
	public Unit unit{get{return mUnit;}}
    public Plugin(Unit unit){ mUnit = unit; }

	public virtual void reset (){}
    public virtual void update(){}
	public virtual void onSync(MessageBase msg){}
}
