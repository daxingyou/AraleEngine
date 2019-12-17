using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using Arale.Engine;

public class AnimPlugin : Plugin
{
	public const int StopAnim = 0;
	public const int PlayAnim = 1;
	public const int Run      = 3;
	public const int StopRun  = 4;
	public const int Hit      = 5;
	public const int Jump     = 6;
	public const int Climb    = 7;
	public const int Die      = 8;

    public Animation mAnim;
	public Animator  mAnimtor;
    public AnimPlugin(Unit unit):base(unit)
    {
		mAnimtor = unit.GetComponent<Animator> ();
		mAnim = unit.GetComponent<Animation>();
		if (mUnit.attr != null)mUnit.attr.onAttrChanged += onAttrChange;
    }

    public float Length(string clipName)
    {
        if (mAnim != null)
        {
            AnimationClip ac = mAnim.GetClip(clipName);
            return ac == null ? 0f : ac.length;
        }

        if (mAnimtor != null)
        {
            AnimationClip[] ac = mAnimtor.runtimeAnimatorController.animationClips;
            for (int i = 0; i < ac.Length; ++i)
            {
                if (ac[i].name == clipName)
                    return ac[i].length;
            }
            return 0f;
        }
        return 0f;
    }

	void onAttrChange(int mask, object val)
	{
		if (mask != (int)AttrID.Speed)return;
		if (mAnimtor != null)mAnimtor.speed = (float)val;
		if (mAnim != null)foreach (AnimationState state in mAnim)state.speed = (float)val;
	}
		
	public void sendEvent(int evt, string anim=null)
	{
		bool needSync = false;
		onEvent (evt, anim, ref needSync);
		if (needSync)sync (evt, anim);
	}

    float jioTime;
	protected virtual void onEvent(int evt, string anim, ref bool needSync)
	{
        if (!mUnit.isState(UnitState.Anim))return;
		switch (evt)
		{
    		case StopAnim:
    			if (mAnimtor != null)mAnimtor.Stop ();
    			if(mAnim!=null)mAnim.Stop (anim);
    			needSync=true;
    			break;
    		case PlayAnim:
    			if (mAnimtor != null)mAnimtor.Play (anim);
    			if(mAnim!=null)mAnim.Play (anim, PlayMode.StopAll);
    			needSync=true;
    			break;
    		case StopRun:
    			if (mAnimtor != null)mAnimtor.SetBool ("Run", false);
    			if (mAnim != null)mAnim.Play ("idle", PlayMode.StopAll);
    			break;
    		case Run:
    			if (mAnimtor != null)mAnimtor.SetBool("Run", true);
    			if (mAnim != null)mAnim.Play ("run", PlayMode.StopAll);

                //显示脚印
                jioTime+=Time.deltaTime;
                if (jioTime > 0.5f && !mUnit.isServer)
                {
                    jioTime -= 0.5f;
                    MeshEffect.addQuad(mUnit.pos, mUnit.transform.eulerAngles.y, Vector2.one, 0);
                }
    			break;
    		case Jump:
    			if (mAnimtor != null)mAnimtor.SetTrigger("Jump");
    			if (mAnim != null)mAnim.Play ("jump", PlayMode.StopAll);
    			break;
            case Hit:
                if (mUnit.isState(UnitState.Break))break;
    			if (mAnimtor != null)mAnimtor.SetTrigger("Hit");
    			if (mAnim != null)mAnim.Play ("hit", PlayMode.StopAll);
    			needSync=true;
    			break;
            case Die:
                if (mAnimtor != null)mAnimtor.SetTrigger("Die");
                if (mAnim != null)mAnim.Play("die", PlayMode.StopAll);
    			needSync=true;
    			break;
		}
	}

    public override void reset ()
    {
        //if (mAnimtor != null)mAnimtor.Stop ();
        //if(mAnim!=null)mAnim.Stop ();
    }

    public override void onSync(MessageBase message)
    {
        MsgAnim msg = message as MsgAnim;
		mUnit.onSyncState (msg);
		bool needSync = false;
		onEvent (msg.evt, msg.anim, ref needSync);
    }

    void sync(int evt, string anim)
    {
        MsgAnim msg = new MsgAnim();
        msg.guid  = mUnit.guid;
        msg.anim  = anim;
		msg.evt   = evt;
        msg.pos   = mUnit.pos;
        msg.dir   = mUnit.dir;
        msg.state = mUnit.state;
        mUnit.sendMsg((short)MyMsgId.Anim, msg);
    }
}
