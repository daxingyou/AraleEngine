using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using Arale.Engine;


public class EffectPlugin : Plugin
{
	public EffectPlugin(Unit unit):base(unit){enable = true;}
	public bool enable{ get; set;}
	List<Effect> mEffects = new List<Effect>();
    public void playEffect(int effectTID)
    {
		if (!enable)return;
		stopEffect (effectTID);
		Effect.play (effectTID, mUnit.transform, onEffectEvent);
		if (mUnit.attr != null)mUnit.attr.onAttrChanged += onAttrChange;
    }

	void onAttrChange(int mask, object val)
	{
		if (mask != (int)AttrID.Speed)return;
		for (int i = 0, max = mEffects.Count; i < max; ++i) {
			mEffects [i].setSpeed ((float)val);
		}
	}

	public void pauseEffect(int effectTID)
	{
		Effect effect = mEffects.Find(delegate(Effect e){return e.tb._id == effectTID;});
		if (effect == null)return;
		effect.pause ();
	}

	public void resumeEffect(int effectTID)
	{
		Effect effect = mEffects.Find(delegate(Effect e){return e.tb._id == effectTID;});
		if (effect == null)return;
		effect.resume();
	}

    public void stopEffect(int effectTID)
    {
		Effect effect = mEffects.Find(delegate(Effect e){return e.tb._id == effectTID;});
		if (effect == null)return;
		effect.stop ();
    }

    public void showEffect(int effectTID)
    {
		Effect effect = mEffects.Find(delegate(Effect e){return e.tb._id == effectTID;});
		if (effect == null)return;
		effect.show (true);
    }

    public void hideEffect(int effectTID)
    {
		Effect effect = mEffects.Find(delegate(Effect e){return e.tb._id == effectTID;});
		if (effect == null)return;
		effect.show (false);
    }

	void onEffectEvent(Effect.Event e, Effect effect)
	{
		if (e == Effect.Event.Play) {
			mEffects.Add (effect);
		} else if (e == Effect.Event.Stop) {
			mEffects.Remove (effect);
		}
	}

    public override void reset ()
    {
        for (int i = 0; i < mEffects.Count; ++i)
        {
            mEffects[i].stop();
        }
    }
}
