using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.SceneManagement;


namespace Arale.Engine
{

    public class SceneMgr : MgrBase<SceneMgr>
    {
        AsyncOperation mLoadAO;
    	string mNextScene;
    	int    mPreloadCount;
        //是否等待网络包
        bool mWaitPacket;
    	public bool waitPacket{set{mWaitPacket=value;}}
        //是否正在加载场景
        bool mIsLoading;
        public bool isLoading{get{return mIsLoading;}}
        //加载进度回调用
        public delegate void OnProgress(float pro);
        OnProgress mProgress;
        public OnProgress onProgress{set{mProgress = value;}get{return mProgress;}}
  
    	public void LoadScene(string levelName, string loadingWindow=null, bool loadScene = false)
    	{
            if(mIsLoading)
            {
                mNextScene = levelName;
    			return;
    		}
 
            if (loadingWindow != null)
            {
                WindowMgr.single.GetWindow(loadingWindow, true);
            }

    		//preloadCount = ResManager.single.preload(ResManager.PreloadKeyType.Scene, levelName, onPreloadFinish);
    		if(loadScene)
    		{
                mNextScene = levelName;
    			Application.LoadLevel("Loading");
    		}
    		else
    		{
                Application.LoadLevel(levelName);
                //mNextScene = levelName;
                //OnLevelWasLoaded();
            }
    	}
            
    	void OnPreloadFinish(Object asset, int index, object param1=null, object param2=null, object param3=null)
    	{
            --mPreloadCount;
    	}

    	IEnumerator LoadLevel(string levelName)
    	{
            WaitForSeconds w = new WaitForSeconds(0.1f);
            float cursor = Random.Range(0.3f, 0.5f);
            if (mPreloadCount > 0)
            {
                for (int i = 0, max = mPreloadCount; i <= max; i = max - mPreloadCount)
                {
                    UpdateLoadProgress(cursor * i / max);
                    if (i == max)break;
                    yield return w;
                }
            }
            else
    		{
    			yield return w;
    		}
            UpdateLoadProgress (cursor);

    		AssetBundle ab = null;
            string sceneFile = ResLoad.resPath+"Scene/"+levelName+".data";
            if (File.Exists(sceneFile))
            {
                WWW www = new WWW("file:///" + sceneFile);
                while (!www.isDone)yield return www;
                ab = www.assetBundle;
            }
            else
            {
                for (int i = 0; i < 10; ++i)
                {
                    UpdateLoadProgress(1.0f*i/10);
                    yield return w;
                }
            }

            while (mWaitPacket)
            {
                yield return w;
            }
                
            mLoadAO = Application.LoadLevelAsync(levelName);
            //mLoadAO.allowSceneActivation = false;
            yield return mLoadAO;
            if (ab != null) ab.Unload(false);
            UpdateLoadProgress(1f);
            mLoadAO = null;
    	}

    	public void OnLevelWasLoaded()
    	{
            Log.i("Begin Load Scene: " + Application.loadedLevelName, Log.Tag.Scene);
            if (null == mNextScene)return;
            string loadingScene = mNextScene;
            mNextScene = null;
    	    GRoot.single.StartCoroutine(LoadLevel(loadingScene));
    	}

        void UpdateLoadProgress(float p)
        {
            if (mProgress == null)return;
            mProgress(p);
        }
    }

}