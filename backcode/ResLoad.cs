#if UNITY_EDITOR
//#define CHECK_PATH
using UnityEditor;
#endif
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using XLua;
namespace Scripts.CoreScripts.Core
{
	[LuaCallCSharp]
	public enum ResideType
	{//缓存有效期从小到大排列
		None,   //不缓存/
		Ref,    //引用计数/
		InTime, //时间内缓存/
		InScene,//场景内缓存/
		InGame, //游戏内缓存/
	}

	[LuaCallCSharp]
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
				TraceLoad(item);
				#endif
				return item;
			}

			static void Recyle(CachItem item)
			{
				mCach.Remove(item.mKey);
				item.mKey = null;
				item.mRef = 0;
				item.mReside = ResideType.None;
				mPool.Add (item);
			}

			internal static bool IsInCach(string key)
			{
				return mCach.ContainsKey (key);
			}

			internal static void AddAssetRef(string key)
			{
				CachItem item = null;
				if (mCach.TryGetValue (key, out item)) 
				{
					if (item.mReside != ResideType.Ref)
						return;
					++item.mRef;
				}
			}

			internal static void DecAssetRef(string key)
			{
				CachItem item = null;
				if (mCach.TryGetValue (key, out item)) {
					if (item.mReside != ResideType.Ref)
						return;
					item.Release ();
				}
			}

			internal static void ClearCach()
			{//清除cach,忽略所有引用，注意存在引用计数的资源
				IDictionaryEnumerator e = mCach.GetEnumerator();
				while(e.MoveNext())
				{
					CachItem it = e.Value as CachItem;
                    it.mReside = ResideType.None;//避免后面的AB被unload(true),导致当前显示资源丢失
                    it.InnerRelease ();
				}
				mCach.Clear ();

                AssetRef[] ars = Resources.FindObjectsOfTypeAll<AssetRef>();
                for (int i = 0, max = ars.Length; i < max; ++i)
                {
                    if (ars[i].hasDepends()) ars[i].SetDepends(null);
                }
            }

            internal static void ClearByPath(string path)
			{
				CachItem item = null;
				if (mCach.TryGetValue (path, out item)) 
				{
					item.Release (true);
				}
			}

			internal static void ClearByTag(string tag)
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
					it.Release (true);
				}
			}

			internal static void ClearByReside(ResideType riside)
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
					it.Release (true);
				}
			}
			#endregion

			internal void Release(bool force=false)
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
					InnerRelease ();
					Recyle (this);
				}
			}

			void InnerRelease()
			{
				if (mDepends != null) 
				{
					AssetRef.DecRef (mDepends);
					mDepends  = null;
				}

				if (mAssetBundle != null) 
				{
                    mAssetBundle.Unload(mReside == ResideType.Ref);
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
				TraceUnload(this);
				#endif
			}

			#region 资源加载
			void LoadAssetBundle()
			{
				if (mAssetBundle != null)return;
				if(mVer>0)
				{
					IEnumerator e = WWWLoad();
					while(e.MoveNext());
				}
				else
				{
                    string path = resPath + mKey + ext;
                    if (!File.Exists(path))return;
                    mAssetBundle = AssetBundle.LoadFromFile(resPath + mKey + ext);
				}
            }

			void LoadDepends()
			{
				//图片应该都打成依赖包，这样就可以使用引用计数管理
				mDepends = mManifest.GetDirectDependencies (mKey + ext);
				for (int i = 0, max = mDepends.Length; i < max; ++i) 
				{
					mDepends[i] = mDepends[i].Remove (mDepends[i].Length - 5);
					CachItem ci = CachItem.getFromCach (mDepends[i], ResideType.Ref);
					++ci.mRef;
                    if (ci.GetAssetBundle() == null)Log.E("load ab failed path=" + mKey, Log.Tag.RES);
                    //有的assetbundle是全局预加载的使用的非ref模式，如shader
                    if (ci.mReside != ResideType.Ref)mDepends[i] = null;
				}
			}

			void SetAssetRef(GameObject prefab)
			{
				if (prefab == null)return;
				mIsPrefab = true;
				if (mAssetRef != null)return;
				mAssetRef = prefab.GetComponent<AssetRef> ();
				if (mIsAB)
				{
					if (null == mAssetRef)mAssetRef = prefab.AddComponent<AssetRef> ();
					mAssetRef.SetDepends (mDepends);
				}
			}

			internal AssetBundle GetAssetBundle()
			{
				LoadAssetBundle();
				return mAssetBundle;
			}

			internal T Asset<T>(string assetName) where T:Object{
				if(mAsset==null)
				{
					if(File.Exists(resPath+mKey+ext))
					{
						mIsAB = true;
						LoadDepends ();
						LoadAssetBundle();
						string mainAssetName = mAssetBundle.GetAllAssetNames ()[0];
						mAsset = assetName==null?mAssetBundle.LoadAsset(mainAssetName):mAssetBundle.LoadAsset(mainAssetName,typeof(T));
					}
					else
					{
						mAsset = assetName==null?Resources.Load(mKey):Resources.Load(mKey,typeof(T));
					}
				}

                if(mAsset==null)
                {
                    Log.E("load asset failed path="+mKey, Log.Tag.RES);
                    return null;
                }
				//添加依赖引用计数管理
				SetAssetRef(mAsset as GameObject);
				return mAsset as T;
			}

			internal GameObject GetGameObject()
			{
				Object o = Asset<GameObject> (null);
				return o==null?null:GameObject.Instantiate(o) as GameObject;
			}
			#endregion

			#region 异步加载 
			internal void AsyncLoad<T>(ResLoad loader, OnLoadFinish onLoadFinish) where T : UnityEngine.Object
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
					mMono.StartCoroutine (YieldLoad (typeof(T)));
				}
				else
				{
					CallData cd = new CallData(loader, onLoadFinish);
					cd.next = mCallData;
					mCallData = cd;
				}
			}


			IEnumerator WWWLoad()
			{
				WWW w = WWW.LoadFromCacheOrDownload ("file:///"+resPath+mKey+ext, mVer);
				yield return w;
				mAssetBundle = w.assetBundle;
			}

			IEnumerator YieldLoad(System.Type type)
			{
				//float t = Time.realtimeSinceStartup;
				string path = resPath + mKey + ext;
				if(File.Exists(path))
				{
					mIsAB = true;
					LoadDepends ();
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
				Release ();
			}
			#endregion


			#region 资源调试(请勿用)
			internal static List<string> GetCachAssets()
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
			internal static void SetTraceAsset(string path)
			{
				traceAsset = path;
			}

			static void TraceLoad(CachItem rl)
			{
				if (rl.mKey != traceAsset)
					return;
				Debug.Log ("load reside="+rl.mReside+" ref="+rl.mRef);
			}

			static void TraceUnload(CachItem rl)
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
        uint mUniqueID;
        CachItem mItem;
		public object mParam1;
		public object mParam2;
		public object mParam3;
		//---------------
        static void LoadManifest()
        {
            string manifestPath = resPath + "ABData" + ext;
            if (!File.Exists(manifestPath)) return;
            AssetBundle ab = AssetBundle.LoadFromFile(manifestPath);
            if (ab != null)
            {
                mManifest = ab.LoadAsset(ab.GetAllAssetNames()[0], typeof(AssetBundleManifest)) as AssetBundleManifest;
                ab.Unload(false);
            }
        }

		public static void Init(MonoBehaviour mono)
		{
			#if CHECK_PATH
			initCheckMap();
			#endif
			resPath = Application.persistentDataPath+"/Res/";
			ext = ".data";
			mMono = mono;
            LoadManifest();
			InitVersionPart();
            Log.I ("res path=" + ResLoad.resPath, Log.Tag.RES);
		}

		public static void Deinit()
		{
			mMono = null;
		}

		//-----------------
		const string ResVerKey  = "Res.Ver";
		const string ResPartKey = "Res.part";
        public static int   version{ get; protected set;}
        public static int[] part{ get; protected set;}
        public static void SetVersionPart(int ver, int[] part, bool additive=true)
		{
			ResLoad.version = ver;
            List<int> ls = additive?new List<int>(ResLoad.part):new List<int>();
            for (int i = 0; i < part.Length; ++i)
            {
                int v = part[i];
                if (!ls.Contains(v))ls.Add(v);
            }
            ls.Sort(delegate(int x, int y){return x-y;});
            ResLoad.part = ls.ToArray();

            string partStr = intArray2Str(ResLoad.part);
			UnityEngine.PlayerPrefs.SetInt (ResVerKey,ver);
            UnityEngine.PlayerPrefs.SetString (ResPartKey, partStr);
			UnityEngine.PlayerPrefs.Save();
            Log.I ("SetVersionPart:" + ResLoad.version + ":" + partStr, Log.Tag.Update);
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
            string partStr  = "0,1,2,3,4,5,6,7,8,9";
		    #else
			ResLoad.version = UnityEngine.PlayerPrefs.GetInt(ResVerKey, 0);
            string partStr  = UnityEngine.PlayerPrefs.GetString(ResPartKey, "");
			#endif
            ResLoad.part    = string.IsNullOrEmpty(partStr)?new int[]{0}:str2IntArray(partStr);
            Log.I ("InitVersionPart:" + ResLoad.version + ":" + partStr, Log.Tag.Update);
		}

        public static bool IsPartOK(int partCode)
		{
            return System.Array.IndexOf<int>(part, partCode) >= 0;
		}

        public static bool IsPartOK(int[] partCodes)
        {
            for (int i = 0; i<partCodes.Length; ++i)
            {
                if (!IsPartOK(partCodes[i]))return false;
            }
            return true;
        }

        public static string intArray2Str(int[] val)
        {
            int max = val.Length;
            if (max < 1)return "";
            string str=val[0].ToString();
            for(int i=1;i<max;++i)str+=","+val[i];
            return str;
        }

        public static int[] str2IntArray(string str)
        {
            string[] ss = str.Split(new char[]{','}, System.StringSplitOptions.RemoveEmptyEntries);
            int[] val = new int[ss.Length];
            for(int i=0;i<ss.Length;++i)val[i] = int.Parse(ss[i]);
            return val;
        }
		//-----------------

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

		public static ResLoad Get(string path, ResideType reside=ResideType.None, int v=0)
		{
			path = path.ToLower ();
			#if CHECK_PATH
			checkPathCase(path);
			#endif
			CachItem it = CachItem.getFromCach (path, reside);
			it.mVer = v;
			return new ResLoad(it);
		}

		public static ResLoad Get(string path, string tag, int v=0)
		{
			path = path.ToLower ();
			#if CHECK_PATH
			checkPathCase(path);
			#endif
			CachItem it = CachItem.getFromCach (path, ResideType.InGame);
			it.mVer = v;
			it.mTag = tag;
			return new ResLoad(it);
		}

		public void Release()
		{
            if (mItem == null)
				return;
            if (mItem.mUniqueID != mUniqueID)
                return;
            mItem.Release ();
			mItem = null;
			mParam1 = null;
			mParam2 = null;
			mParam3 = null;
		}

		internal AssetRef Depends()
		{
            if (mItem.mUniqueID != mUniqueID)
            {
                Log.E("ResLoad expired", Log.Tag.RES);
                return null;
            }
			return mItem.mAssetRef;
		}

		public AssetBundle GetAssetBundle()
		{
            if (mItem.mUniqueID != mUniqueID)
            {
                Log.E("ResLoad expired", Log.Tag.RES);
                return null;
            }
            return mItem.GetAssetBundle ();
		}

		public T Asset<T>(string assetName = null) where T:Object
		{
            if (mItem.mUniqueID != mUniqueID)
            {
                Log.E("ResLoad expired", Log.Tag.RES);
                return null;
            }
            return mItem.Asset<T> (assetName);
		}

		public GameObject GetGameObject()
		{
            if (mItem.mUniqueID != mUniqueID)
            {
                Log.E("ResLoad expired", Log.Tag.RES);
                return null;
            }
            GameObject go = mItem.GetGameObject();
			Release ();
			return go;
		}

		public void AsyncLoad(OnLoadFinish onLoadFinish, object param1=null, object param2=null, object param3=null)
		{
            if (mItem.mUniqueID != mUniqueID)
            {
                Log.E("ResLoad expired", Log.Tag.RES);
                return;
            }
            mParam1 = param1;
			mParam2 = param2;
			mParam3 = param3;
			mItem.AsyncLoad<GameObject>(this, onLoadFinish);
		}

		public void AsyncLoadT<T>(OnLoadFinish onLoadFinish, object param1=null, object param2=null, object param3=null) where T : UnityEngine.Object
		{
            if (mItem.mUniqueID != mUniqueID)
            {
                Log.E("ResLoad expired", Log.Tag.RES);
                return;
            }
            mParam1 = param1;
			mParam2 = param2;
			mParam3 = param3;
			mItem.AsyncLoad<T>(this, onLoadFinish);
		}

		internal static void AddAssetRef (string path)
		{
			CachItem.AddAssetRef (path);
		}

		internal static void DecAssetRef (string path)
		{
			CachItem.DecAssetRef (path);
		}

		public static bool IsInCach(string path)
		{
			path = path.ToLower ();
			return CachItem.IsInCach (path);
		}

		public static void ClearCach()
		{
			CachItem.ClearCach ();
		}

		public static void ClearByPath(string path)
		{
			CachItem.ClearByPath (path);
		}

		public static void ClearByTag(string tag)
		{
			CachItem.ClearByTag (tag);
		}

		public static void ClearByReside(ResideType riside)
		{
			CachItem.ClearByReside (riside);
		}

		public static List<string> GetCachAssets()
		{
			return CachItem.GetCachAssets ();
		}

		public static void SetTraceAsset(string path)
		{
			CachItem.SetTraceAsset (path);
		}
		#endregion


		#region IDisposable
		public void Dispose ()
		{
			Release ();
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
		Log.E("!!!path case error ,must be corrected:path=" + path, Log.Tag.RES);
		}
		}
		#endif
	}

}