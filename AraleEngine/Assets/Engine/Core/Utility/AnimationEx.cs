using UnityEngine;
using System.Collections;

namespace Arale.Engine
{
    
    public class AnimationEx : MonoBehaviour
    {
    	public delegate void OnComplite();
    	Animation anim;
    	public void Play(string clipRes, OnComplite onComplete, bool unscaleTime)
    	{
    		anim = gameObject.GetComponent<Animation>();
    		if(anim==null)
    			anim = gameObject.AddComponent<Animation>();
    		AnimationClip ac = ResLoad.get(clipRes).asset<AnimationClip>();
    		anim.AddClip(ac,ac.name);
    		if(unscaleTime)
    		{
    			StartCoroutine(Play(ac.name,onComplete));
    		}
    		else
    		{
    			anim.Play(ac.name);
    		}
    	}

    	IEnumerator Play(string clipName, OnComplite onComplete)
    	{   
    		AnimationState _currState = anim[clipName];
    		bool isPlaying = true;
    		float _progressTime = 0F;
    		float _timeAtLastFrame = 0F;
    		float _timeAtCurrentFrame = 0F;
    		float deltaTime = 0F;
    		anim.Play(clipName);
    		_timeAtLastFrame = Time.realtimeSinceStartup;
    		while (isPlaying)
    		{
    			_timeAtCurrentFrame = Time.realtimeSinceStartup;
    			deltaTime = _timeAtCurrentFrame - _timeAtLastFrame;
    			_timeAtLastFrame = _timeAtCurrentFrame;
    			_progressTime += deltaTime;
    			_currState.normalizedTime = _progressTime / _currState.length;
    			anim.Sample ();
    			if (_progressTime >= _currState.length)
    			{
    				if(_currState.wrapMode != WrapMode.Loop)
    				{
    					isPlaying = false;
    				}
    				else
    				{
    					_progressTime = 0.0f;
    				}
    			}
    			yield return null;
    		}
    		if(onComplete != null)  
    		{
    			onComplete();
    		}
    	}
    }

}
