#if UNITY_EDITOR
//#define CHECK_PATH
using UnityEditor;
#endif
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Arale.Engine
{
	public enum ResideType
	{//缓存有效期从小到大排列
		None,   //不缓存/
		Ref,    //引用计数/
		InTime, //时间内缓存/
		InScene,//场景内缓存/
		InGame, //游戏内缓存/
	}

	public class ResLoad : System.IDisposable
	{
		class CallData
		{
			public CallData(ResLoad loader, OnLoadFinish func)
			{
				this.loader = loader;
				this.func = func;
			}

			public CallData call()
			{
				if(null!=func)func (loader);
				loader = null;
				func   = null;
				CallData cd = next;
				next = null;
				return cd;
			}

			OnLoadFinish func;
			ResLoad loader;
			public CallData next;
		}

		class CachItem
		{
			static Dictionary<string, CachItem> mCach = new Dictionary<string, CachItem>();
			static List<CachItem> mPool = new List<CachItem> ();
			//--------------
			internal string      mKey;
			internal string      mTag;
			internal float       mTime;
			internal int         mRef;
			internal ResideType  mReside;
			internal uint        mUniqueID;
			//--------------
			internal CallData    mCallData;
			internal AssetBundle mAssetBundle;
			internal Object 	 mAsset;
			internal int    	 mVer;
			internal bool        mIsPrefab;
			internal bool        mIsAB;
			internal AssetRef    mAssetRef;
			internal string[]    mDepends;
			//--------------
			#region cach管理
			internal static CachItem getFromCach(string key, ResideType reside)
			{
				CachItem item = null;
				if(mCach.TryGetValue(key,out item))
				{
					if(item.mReside != ResideType.Ref && item.mReside<reside)
						item.mReside = reside;
					item.mTime = Time.realtimeSinceStartup;
				}
				else
				{
					if (mPool.Count > 0) 
					{
						item = mPool[0];
						mPool.RemoveAt(0);
					}
					else
					{
						item = new CachItem();
					}

					item.mKey      = key;
					item.mReside   = reside;
					item.mRef      = 0;
					item.mVer      = 0;
					item.mIsPrefab = false;
					item.mIsAB     = false;
					item.mCallData = null;
					item.mTag      = null;

					if (item.mReside != ResideType.None) 
					{
						item.mTime = Time.realtimeSinceStartup;
						mCach [item.mKey] = item;
					}
				}
				item.mUniqueID = ++uniqueID;
				#if UNITY_EDITOR
				traceLoad(item);
				#endif
				return item;
			}

			static void recyle(CachItem item)
			{
				mCach.Remove(item.mKey);
				item.mKey = null;
				item.mRef = 0;
				item.mReside = ResideType.None;
				mPool.Add (item);
			}

			internal static void addAssetRef(string key)
			{
				CachItem item = null;
				if (mCach.TryGetValue (key, out item)) 
				{
					if (item.mReside != ResideType.Ref)
						return;
					++item.mRef;
				}
			}

			internal static void decAssetRef(string key)
			{
				CachItem item = null;
				if (mCach.TryGetValue (key, out item)) {
					if (item.mReside != ResideType.Ref)
						return;
					item.release ();
				}
			}

			internal static void clearCach()
			{//清除cach,忽略所有引用，注意存在引用计数的资源
				IDictionaryEnumerator e = mCach.GetEnumerator();
				while(e.MoveNext())
				{
					CachItem it = e.Value as CachItem;
					//避免ab被unload(true)导致当前显示的资源丢失
					it.mReside = ResideType.None;
					it.innerRelease ();
				}
				mCach.Clear ();
				//解除引用计数关系
				AssetRef[] ars = Resources.FindObjectsOfTypeAll<AssetRef> ();
				for(int i=0,max=ars.Length;i<max;++i)
				{
					if(ars[i].hasDepends())ars [i].setDepends (null);
				}
			}

			internal static void clearByPath(string path)
			{
				CachItem item = null;
				if (mCach.TryGetValue (path, out item)) 
				{
					item.release (true);
				}
			}

			internal static void clearByTag(string tag)
			{
				ArrayList ls = new ArrayList ();
				IDictionaryEnumerator e = mCach.GetEnumerator();
				while(e.MoveNext())
				{
					CachItem it = e.Value as CachItem;
					if (tag == it.mTag)ls.Add (it);
				}
				for (int i=0,max=ls.Count; i<max; ++i)
				{
					CachItem it = ls[i] as CachItem;
					it.release (true);
				}
			}

			internal static void clearByReside(ResideType riside)
			{
				ArrayList ls = new ArrayList ();
				IDictionaryEnumerator e = mCach.GetEnumerator();
				while(e.MoveNext())
				{
					CachItem it = e.Value as CachItem;
					if(it.mReside==riside)
					{
						ls.Add(it);
					}
				}
				for (int i=0,max=ls.Count; i<max; ++i)
				{
					CachItem it = ls[i] as CachItem;
					it.release (true);
				}
			}
			#endregion

			internal void release(bool force=false)
			{
				if(mKey==null)
					return;
				if(mCallData!=null)//异步调用未完成
					return;

				if (mReside == ResideType.None) 
				{
					force = true;
				}
				else if (mReside == ResideType.Ref)
				{
					if(--mRef<1)force = true;
				}	

				if(force)
				{
					innerRelease ();
					recyle (this);
				}
			}

			void innerRelease()
			{
				if (mDepends != null) 
				{
					AssetRef.decRef (mDepends);
					mDepends  = null;
				}

				if (mAssetBundle != null) 
				{
					mAssetBundle.Unload (mReside == ResideType.Ref);
					mAssetBundle = null;
				}

				if (mAsset != null) 
				{
					if (mIsPrefab) 
					{//小心勿改,编辑模式下DestroyImmediate会删除原始资源
						if(mIsAB)GameObject.DestroyImmediate(mAsset,mIsAB);
					} 
					else 
					{
						Resources.UnloadAsset (mAsset);
					}
					mAsset = null;
				}
				mAssetRef = null;
				mCallData = null;
				#if UNITY_EDITOR
				traceUnload(this);
				#endif
			}

			#region 资源加载
			void loadAssetBundle()
			{
				if (mAssetBundle != null)return;
				if(mVer>0)
				{
					IEnumerator e = wwwLoad();
					while(e.MoveNext());
				}
				else
				{
					mAssetBundle = AssetBundle.LoadFromFile(resPath+mKey+ext);
				}
			}

			void loadDepends()
			{
				//图片应该都打成依赖包，这样就可以使用引用计数管理
				mDepends = mManifest.GetDirectDependencies (mKey + ext);
				for (int i = 0, max = mDepends.Length; i < max; ++i) 
				{
					mDepends[i] = mDepends[i].Remove (mDepends[i].Length - 5);
					CachItem ci = CachItem.getFromCach (mDepends[i], ResideType.Ref);
					++ci.mRef;
					if(null==ci.assetBundle ())Log.e ("load ab failed path="+mKey,Log.Tag.RES);
					//有的assetbundle是全局预加载的使用的非ref模式，如shader
					if (ci.mReside != ResideType.Ref)mDepends[i] = null;
				}
			}

			void setAssetRef(GameObject prefab)
			{
				if (prefab == null)return;
				mIsPrefab = true;
				if (mAssetRef != null)return;
				mAssetRef = prefab.GetComponent<AssetRef> ();
				if (mIsAB)
				{
					if (null == mAssetRef)mAssetRef = prefab.AddComponent<AssetRef> ();
					mAssetRef.setDepends (mDepends);
				}
			}

			internal AssetBundle assetBundle()
			{
				loadAssetBundle();
				return mAssetBundle;
			}

			internal T asset<T>(string assetName) where T:Object{
				if(mAsset==null)
				{
					if(File.Exists(resPath+mKey+ext))
					{
						mIsAB = true;
						loadDepends ();
						loadAssetBundle();
						string mainAssetName = mAssetBundle.GetAllAssetNames ()[0];
						mAsset = assetName==null?mAssetBundle.LoadAsset(mainAssetName):mAssetBundle.LoadAsset(mainAssetName,typeof(T));
					}
					else
					{
						mAsset = assetName==null?Resources.Load(mKey):Resources.Load(mKey,typeof(T));
					}
				}

				if (null==mAsset)Log.e ("load asset failed path="+mKey,Log.Tag.RES);
				//添加依赖引用计数管理
				setAssetRef(mAsset as GameObject);
				return mAsset as T;
			}

			internal GameObject gameObject()
			{
				return GameObject.Instantiate(asset<GameObject>(null)) as GameObject;
			}
			#endregion

			#region 异步加载 
			internal void asyncLoad<T>(ResLoad loader, OnLoadFinish onLoadFinish) where T : UnityEngine.Object
			{
				if(mAsset!=null)
				{
					if(null!=onLoadFinish)onLoadFinish(loader);
					return;
				}

				if (mCallData==null)
				{
					mCach[mKey] = this;
					mCallData = new CallData(loader, onLoadFinish);
					mMono.StartCoroutine (yieldLoad (typeof(T)));
				}
				else
				{
					CallData cd = new CallData(loader, onLoadFinish);
					cd.next = mCallData;
					mCallData = cd;
				}
			}


			IEnumerator wwwLoad()
			{
				WWW w = WWW.LoadFromCacheOrDownload ("file:///"+resPath+mKey+ext, mVer);
				yield return w;
				mAssetBundle = w.assetBundle;
			}

			IEnumerator yieldLoad(System.Type type)
			{
				//float t = Time.realtimeSinceStartup;
				string path = resPath + mKey + ext;
				if(File.Exists(path))
				{
					mIsAB = true;
					loadDepends ();
					if(mVer>0)
					{
						WWW w = WWW.LoadFromCacheOrDownload ("file:///"+path, mVer);
						yield return w;
						mAssetBundle = w.assetBundle;
					}
					else
					{
						AssetBundleCreateRequest req = AssetBundle.LoadFromFileAsync (path);
						yield return req;
						mAssetBundle = req.assetBundle;
					}

					string mainAssetName = mAssetBundle.GetAllAssetNames ()[0];
					AssetBundleRequest abr = mAssetBundle.LoadAssetAsync(mainAssetName,type);
					yield return abr;
					mAsset = abr.asset;
				}
				else
				{
					ResourceRequest rr = Resources.LoadAsync(mKey,type);//不能调用LoadAsync<T>,android对同名模板存在识别问题，找不到方法/
					yield return rr;
					mAsset = rr.asset;
				}
				//Debug.Log ("use time="+(Time.realtimeSinceStartup-t));
				for(CallData cd = mCallData;cd!=null;cd=cd.call())yield return null;
				mCallData = null;
				release ();
			}
			#endregion


			#region 资源调试(请勿用)
			internal static List<string> getCachAssets()
			{
				List<string> ls = new List<string> ();
				IDictionaryEnumerator e = mCach.GetEnumerator();
				while(e.MoveNext())
				{
					CachItem it = e.Value as CachItem;
					ls.Add (it.mKey + ":" + it.mReside + ":" + it.mRef);
				}
				ls.Sort (delegate(string x, string y){
					return x.CompareTo(y);
				});
				return ls;
			}

			static string traceAsset;
			internal static void setTraceAsset(string path)
			{
				traceAsset = path;
			}

			static void traceLoad(CachItem rl)
			{
				if (rl.mKey != traceAsset)
					return;
				Debug.Log ("load reside="+rl.mReside+" ref="+rl.mRef);
			}

			static void traceUnload(CachItem rl)
			{
				if (rl.mKey != traceAsset)
					return;
				Debug.Log ("unload reside="+rl.mReside+" ref="+rl.mRef);
			}
			#endregion
		}


		#region ResLoad
		public static string resPath;
		public static string ext;
		public delegate void OnLoadFinish(ResLoad resLoad);
		static uint uniqueID;
		static MonoBehaviour mMono;
		static AssetBundleManifest mManifest;
		//---------------
		CachItem mItem;
		uint mUniqueID;
		public object mParam1;
		public object mParam2;
		public object mParam3;
		//---------------
		static void loadManifest()
		{
			string manifestPath = resPath+"Data"+ext;
			if(!File.Exists(manifestPath))return;
			AssetBundle ab = AssetBundle.LoadFromFile(manifestPath);
			if (ab != null) 
			{
				mManifest = ab.LoadAsset(ab.GetAllAssetNames()[0],typeof(AssetBundleManifest)) as AssetBundleManifest;
				ab.Unload (false);
			}
		}

		public static void init(MonoBehaviour mono)
		{
			#if CHECK_PATH
			initCheckMap();
			#endif
			resPath = Application.persistentDataPath+"/Res/";
			ext = ".data";
			mMono = mono;
			loadManifest ();
            InitVersionPart();
			Log.i ("res path=" + ResLoad.resPath, Log.Tag.RES);
		}

		public static void deinit()
		{
			mMono = null;
		}
		//----------------
		ResLoad(CachItem ci)
		{
			mItem = ci;
			mUniqueID = mItem.mUniqueID;
			if (mItem.mReside == ResideType.Ref)
				++mItem.mRef;
		}

		public ResLoad(ResLoad rl)
		{
			mItem = rl.mItem;
			mUniqueID = mItem.mUniqueID;
			if (mItem.mReside == ResideType.Ref)
				++mItem.mRef;
		}

		public static ResLoad get(string path, ResideType reside=ResideType.None, int v=0)
		{
			#if CHECK_PATH
			checkPathCase(path);
			#endif
			CachItem it = CachItem.getFromCach (path, reside);
			it.mVer = v;
			return new ResLoad(it);
		}

		public static ResLoad get(string path, string tag, int v=0)
		{
			#if CHECK_PATH
			checkPathCase(path);
			#endif
			CachItem it = CachItem.getFromCach (path, ResideType.InGame);
			it.mVer = v;
			it.mTag = tag;
			return new ResLoad(it);
		}

		public void release()
		{
			if (mItem == null)
				return;
			if (mUniqueID != mItem.mUniqueID)
				return;
			mItem.release ();
			mItem = null;
			mParam1 = null;
			mParam2 = null;
			mParam3 = null;
		}

		internal AssetRef depends()
		{
			if (mUniqueID != mItem.mUniqueID) 
			{
				Log.e ("ResLoad has expired", Log.Tag.RES);
				return null;
			}
			return mItem.mAssetRef;
		}

		public AssetBundle assetBundle()
		{
			if (mUniqueID != mItem.mUniqueID) 
			{
				Log.e ("ResLoad has expired", Log.Tag.RES);
				return null;
			}
			return mItem.assetBundle ();
		}

		public T asset<T>(string assetName = null) where T:Object
		{
			if (mUniqueID != mItem.mUniqueID) 
			{
				Log.e ("ResLoad has expired", Log.Tag.RES);
				return null;
			}
			return mItem.asset<T> (assetName);
		}

		public GameObject gameObject()
		{
			if (mUniqueID != mItem.mUniqueID) 
			{
				Log.e ("ResLoad has expired", Log.Tag.RES);
				return null;
			}
			GameObject go = mItem.gameObject();
			release ();
			return go;
		}

		public void asyncLoad(OnLoadFinish onLoadFinish, object param1=null, object param2=null, object param3=null)
		{
			if (mUniqueID != mItem.mUniqueID) 
			{
				Log.e ("ResLoad has expired", Log.Tag.RES);
				return;
			}
			mParam1 = param1;
			mParam2 = param2;
			mParam3 = param3;
			mItem.asyncLoad<GameObject>(this, onLoadFinish);
		}

		public void asyncLoadT<T>(OnLoadFinish onLoadFinish, object param1=null, object param2=null, object param3=null) where T : UnityEngine.Object
		{
			if (mUniqueID != mItem.mUniqueID) 
			{
				Log.e ("ResLoad has expired", Log.Tag.RES);
				return;
			}
			mParam1 = param1;
			mParam2 = param2;
			mParam3 = param3;
			mItem.asyncLoad<T>(this, onLoadFinish);
		}

		internal static void addAssetRef (string path)
		{
			CachItem.addAssetRef (path);
		}

		internal static void decAssetRef (string path)
		{
			CachItem.decAssetRef (path);
		}

		public static void clearCach()
		{
			CachItem.clearCach ();
		}

		public static void clearByPath(string path)
		{
			CachItem.clearByPath (path);
		}

		public static void clearByTag(string tag)
		{
			CachItem.clearByTag (tag);
		}

		public static void clearReside(ResideType riside)
		{
			CachItem.clearByReside (riside);
		}

		public static List<string> getCachAssets()
		{
			return CachItem.getCachAssets ();
		}

		public static void setTraceAsset(string path)
		{
			CachItem.setTraceAsset (path);
		}
		#endregion


		#region IDisposable
		public void Dispose ()
		{
			release ();
		}
		#endregion


        #region 资源版本
        public static int version{ get; protected set;}
        public static int part{ get; protected set;}
        const string ResVerKey  = "Res.Ver";
        const string ResPartKey = "Res.part";
        public static void SetVersionPart(int ver, int part)
        {
            ResLoad.version = ver;
            ResLoad.part = part;
            UnityEngine.PlayerPrefs.SetInt (ResVerKey,ver);
            UnityEngine.PlayerPrefs.SetInt (ResPartKey, part);
            UnityEngine.PlayerPrefs.Save();
            Log.i ("SetVersionPart:" + ResLoad.version + "." + ResLoad.part, Log.Tag.Update);
        }

        public static void ClearVersionPart()
        {
            UnityEngine.PlayerPrefs.DeleteKey(ResVerKey);
            UnityEngine.PlayerPrefs.DeleteKey(ResPartKey);
            UnityEngine.PlayerPrefs.Save();
        }

        static void InitVersionPart()
        {
            #if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
            ResLoad.version = 1000;
            ResLoad.part    = 1000;
            #else
            ResLoad.version = UnityEngine.PlayerPrefs.GetInt(ResVerKey, 0);
            ResLoad.part    = UnityEngine.PlayerPrefs.GetInt(ResPartKey, 0);
            #endif
            Log.i ("InitVersionPart:" + ResLoad.version + "." + ResLoad.part, Log.Tag.Update);
        }

        static bool IsResOK()
        {
            return true;
        }
        #endregion


		#if CHECK_PATH
		static Dictionary<string, int> assetsCheckMap = null;
		public static void initCheckMap()
		{
		assetsCheckMap = new Dictionary<string, int>();
		string[] paths = AssetDatabase.GetAllAssetPaths();
		for(int i=0;i<paths.Length;++i)
		{
		string path = paths[i];
		if(!path.StartsWith("Assets/Resources/"))continue;
		int idx = path.LastIndexOf('.');
		int n   = path.LastIndexOf('/');
		if(idx<0||idx<n)idx = path.Length;
		path = path.Substring(17, idx-17);
		assetsCheckMap[path]=1;
		}
		}

		static void checkPathCase(string path)
		{
		if (null != assetsCheckMap && !assetsCheckMap.ContainsKey(path))
		{
		Log.e("!!!path case error ,must be corrected:path=" + path, Log.Tag.RES);
		}
		}
		#endif
	}

}