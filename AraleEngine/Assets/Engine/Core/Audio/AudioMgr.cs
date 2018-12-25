using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Arale.Engine
{

    public class AudioMgr : MgrBase<AudioMgr>
    {
        public static bool  mEffectOn = true;
        public static bool  mMusicOn  = true;
        public static float mEffectVolume= 1f;
        public static float mMusicVolume = 1f;


        Audio mCurMusic;//当前背景音乐
        public Audio curMusic{get{return mCurMusic;}}
    	int mCurMusicId;
        public int curMusicId{get{return mCurMusicId;}}

        public enum StateType
        {
            Waitting,//等待运行状态/
            Run,     //运行状态/
            Stop,    //强制停止/
            pause,   //暂停状态/
        }

        public enum SwitchType
        {
            Immediate,//立刻进行切换
            Fade,     //淡入淡出
            Loop,     //循环播放
        }

        public class Audio
        {
            public StateType state;
            public AudioSource audioSource;
            public TBSound tb;
            public SwitchType switchType;
            public Transform trans;

            public void Stop()
            {
    			if (state != StateType.Stop) 
    			{
    				if(null != trans)
    				{
    					GameObject.Destroy(trans.gameObject);
    				}

    				if(audioSource!=null)
    				{
    					audioSource.Stop();
    					GameObject.Destroy(audioSource.gameObject);
    					audioSource=null;
    				}
    				state = StateType.Stop;
    			}
            }
        }

        //AudioListener节点,非3d音效都加在该节点下,该节点动态绑定到camera实现3d位置效果/
        Transform transform;
        public override void Init()
        {
            GameObject t = new GameObject("Sounds");
            t.AddComponent<AudioListener>();
            GameObject.DontDestroyOnLoad(t);
            transform = t.transform;
        }

        public void BindCamera()
        {
            if (Camera.main != null)
            {
                AudioListener al = Camera.main.GetComponent<AudioListener>();
                if (al != null) al.enabled = false;
                transform.parent = Camera.main.transform;
                transform.localPosition = Vector3.zero;
            }
        }

        public void UnbindCamera()
        {
            transform.parent = null;
            transform.position = Vector3.zero;
        }

        public void StopMusic()
        {
            if (mCurMusic != null)
            {
                mCurMusic.Stop();
                mCurMusic = null;
            }
        }

    	//播放栈顶音乐(最近播放的)
    	public Audio PlayMusic()
    	{
            if(mCurMusicId==0)return null;
            return Play(mCurMusicId, null, SwitchType.Fade);
    	}

    	public Audio Play(int soundId, GameObject source, SwitchType switchType = SwitchType.Fade, string tag=null)
        {
            if (soundId == 0)
                return null;

            // 背景音乐关闭
            if (soundId <= 100 && false == mMusicOn)
                return null;

            // 音效关闭
            if (soundId > 100 && false == mEffectOn)
               return null;

    		//该背景音乐正在播放
            if (soundId <= 100 && mCurMusic != null && mCurMusic.tb.id == soundId)
                return mCurMusic;

            TBSound tb = TableMgr.single.GetDataByKey(typeof(TBSound), soundId) as TBSound;
            if (null == tb) return null;
            GameObject src;
            if (source == null)
            {//2d声音/
                src = new GameObject(soundId.ToString());
    			if(tag!=null)src.tag = tag;
                src.transform.parent = transform;
                src.transform.localPosition = Vector3.zero;
            }
            else
            {
                src = new GameObject(soundId.ToString());
    			if(tag!=null)src.tag = tag;
                src.transform.parent = source.transform;
                src.transform.localPosition = Vector3.zero;
            }

            Audio audio = new Audio();
            audio.switchType = switchType;
            audio.tb = tb;
            audio.trans = src.transform;
            audio.state = StateType.Waitting;
    		if(tb.id<=100)
    		{
                if(mCurMusic!=null)mCurMusic.Stop();
                mCurMusic = audio;
                mCurMusicId = soundId;
    		}
           
    		//异步的引用计数会出错,可能src对象销毁了asset才加载进来
            ResLoad rl = ResLoad.get(tb.asset, ResideType.Ref);
            src.AddComponent<AudioRef>().resLoad=rl;
            OnLoadAssetFinish (rl.asset<AudioClip>(), src, audio, null);
            return audio;
        }

        void OnLoadAssetFinish(Object asset, object param1 = null, object param2 = null, object param3 = null)
        {
            AudioClip ac  = asset as AudioClip;
            GameObject go = param1 as GameObject;
            Audio audio   = param2 as Audio;
    		if(!go)return;
            if(audio.state == StateType.Stop) return;

            AudioSource asrc = go.GetComponent<AudioSource>();
            if (asrc == null)asrc = go.AddComponent<AudioSource>();
          
            if (audio.tb.id <= 100)
            {//背景音乐/
                audio.audioSource = asrc;
                asrc.volume = mMusicVolume*audio.tb.volume;
                asrc.clip   = ac;
                asrc.pitch  = 1f;//这个可以控制播放速率
                if (audio.switchType == SwitchType.Fade)
                {
                    asrc.volume = 0f;
    				GRoot.single.StartCoroutine(AudioFadeEffect());
                }
                asrc.loop = audio.tb.loop>0;
                asrc.Play();
                if (!asrc.loop)
                {
                    GameObject.Destroy(go, ac.length);
                }
            }
            else
            {//音效/
                audio.audioSource = asrc;
                asrc.volume = mEffectVolume*audio.tb.volume;
                asrc.clip   = ac;
                if (audio.switchType == SwitchType.Loop || audio.tb.loop>0)
                {
                    asrc.loop = true;
                    asrc.Play();
                }
                else
                {
                    asrc.loop = false;
                    asrc.Play();
                    GameObject.Destroy(go, ac.length);
                }
                audio.state = StateType.Run;
            }
        }

    	System.Collections.IEnumerator AudioFadeEffect()
    	{
    		//增加背景音乐渐入渐出处理
            AudioSource cur = AudioMgr.single.curMusic.audioSource;
            float volume = AudioMgr.single.curMusic.tb.volume;
            while (cur && cur.volume < volume)
    		{
    			cur.volume += 0.01f;
    			yield return null;
    		}
    	}
    }

}