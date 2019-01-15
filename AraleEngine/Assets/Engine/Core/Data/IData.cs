using UnityEngine;
//using System.Collections;
using System.Collections.Generic;

//if use lua ,IData write by lua also;
public class IData{
	public delegate void OnDataChanged(int mask, object val=null);
	public OnDataChanged onDataChanged;
	public void AddOnDataChanged(OnDataChanged callback){onDataChanged += callback;}
	public void RemoveOnDataChanged(OnDataChanged callback){onDataChanged -= callback;}
	public virtual void Notify(int mask, object val=null)
	{
        if (onDataChanged != null)onDataChanged(mask, val);
	}
}
