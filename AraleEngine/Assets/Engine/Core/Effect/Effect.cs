using UnityEngine;
using System.Collections;
namespace Arale.Engine
{
	public sealed class Effect : LuaMono, PoolMgr<int>.IPoolObject
	{
		public enum Event
		{
			Play,
			Stop,
		}

		public delegate void OnEvent(Event e, Effect effect);
		public TBEffect tb{ get; protected set;}
		public OnEvent  onEvent;
		public void show(bool show)
		{
			gameObject.SetActive (show);
		}

		public void play(Transform target, OnEvent onEvent=null)
		{
			this.onEvent = onEvent;
			if (string.IsNullOrEmpty (tb.srcMount))
			{
				transform.SetParent (target, false);
			}
			else
			{
				Transform t =Unit.getNode(target,tb.srcMount);
				transform.SetParent (t, false);
			}
			gameObject.SetActive (true);
			transform.localPosition = tb.srcPos;
			transform.localEulerAngles = tb.srcDir;
			if (tb.life > 0)Invoke ("stop", tb.life);
			if(onEvent!=null)onEvent (Event.Play, this);
		}

		public void pause()
		{
		}

		public void resume()
		{
		}

		public void setSpeed(float speed)
		{
			ParticleSystem[] ps = GetComponentsInChildren<ParticleSystem> (true);
			for (int i = 0; i < ps.Length; ++i)ps [i].playbackSpeed = speed;
		}

		public static Effect play(int effectId, Transform target, OnEvent onEvent=null)
		{
			Effect effect = Effect.Pool.alloc(effectId) as Effect;
			if (effect == null)return null;
			effect.play (target, onEvent);
			return effect;
		}
			
		#region 对象池
		public static PoolMgr<int> Pool = new PoolMgr<int>(delegate(int tid) {
			TBEffect tb = TableMgr.single.GetData<TBEffect> (tid);
			if(tb==null)return null;
			GameObject go = ResLoad.get(tb.model, ResideType.InScene).gameObject();
			Effect e = go.AddComponent<Effect>();
			e.tb = tb;
			return e;
		});

		public void stop()
		{
			if(onEvent!=null)onEvent (Event.Stop, this);
			Pool.recyle (this);
		}

		public int getKey()
		{
			return tb._id;
		}

		public void onReset()
		{
			gameObject.SetActive (true);
		}

		public void onRecycle()
		{
			onEvent = null;
			transform.parent = null;
			gameObject.SetActive (false);
		}

		public void onDispose()
		{
			onEvent = null;
			Destroy (gameObject);
		}
		#endregion
	}
}
