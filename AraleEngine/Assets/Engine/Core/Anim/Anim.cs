/// <summary>
/// 该类主要用于游戏内动画,如特效动画控制 
/// </summary>
using UnityEngine;
using System.Collections;

public class Anim{
	public enum AnimEvent
	{
		Begin,
		End,
	}
	public float mDuration;
	public float mElapse;
	public delegate void OnAnimListener (AnimEvent id, object value);
	OnAnimListener mOnAnimListener=null;
	protected void SendEvent(AnimEvent id, object val = null)
	{
		if(null!=mOnAnimListener)mOnAnimListener(id,val);
	}
	protected virtual void Update () {
	}
}
