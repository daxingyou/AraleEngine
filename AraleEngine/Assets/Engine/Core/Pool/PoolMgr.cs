using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class PoolMgr<T>
{
	public interface IPoolObject
	{
		T getKey();
		void onReset();
		void onRecycle();
		void onDispose();
	}

	public delegate IPoolObject CreateObject(T param);
	CreateObject mCreator;
	public PoolMgr(CreateObject creator)
	{
		mCreator = creator;
	}


	Dictionary<T, List<IPoolObject>> mPool = new Dictionary<T, List<IPoolObject>>();
	public IPoolObject alloc(T key)
	{
		IPoolObject t = null;
		List<IPoolObject> ls;
		if (mPool.TryGetValue (key, out ls) && ls.Count > 0)
		{
			t = ls [0];
			ls.RemoveAt (0);
			t.onReset ();
			return t;
		}
		t = mCreator (key);
		if (t == null)return null;
		t.onReset ();
		return t;
	}

	public void free(T key)
	{
		List<IPoolObject> ls;
		if (!mPool.TryGetValue (key, out ls))return;
		for (int i = 0, max = ls.Count; i < max; ++i)ls [i].onDispose ();
		ls.Clear ();
		mPool.Remove (key);
	}

	public void recyle(IPoolObject o)
	{
		Unit u = o as Unit;
		GameObject.Destroy (u.gameObject);
		/*对象回收时,plugin还在运行，导致重用状态错误，暂时关闭
		o.onRecycle ();
		List<IPoolObject> ls;
		if (!mPool.TryGetValue (o.getKey (), out ls))
		{
			ls = new List<IPoolObject> ();
			mPool [o.getKey ()] = ls;
		}
		ls.Add (o);*/
	}
}
