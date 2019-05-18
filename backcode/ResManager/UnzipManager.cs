using UnityEngine;
using System.Collections;
using Scripts.CoreScripts.Core;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using Scripts.BehaviourScripts.UI.Login;
using System.Threading;
using Scripts.CoreScripts.Core.Notice;
using System.Collections.Generic;
using System;
using Scripts.CoreScripts.Core.Platform;

public class UnzipManager : MonoBehaviour {
	#if UNITY_ANDROID && !UNITY_EDITOR
	private const string SDK_JAVA_CLASS = "com.baina.goldshark.update.Helper";
	[System.Serializable]
	public class UnzipParemJson
	{
		public string zip_path;
		public string tmp_path;
		public string out_path;
		public string gameobject;
		public string callback;

	}
	void callAndroidUnzip(string zipFile, string tmpPath, string targetPath)
	{
		using (AndroidJavaClass cls = new AndroidJavaClass(SDK_JAVA_CLASS))
		{
			UnzipParemJson o = new UnzipParemJson ();
			o.zip_path = zipFile;
			o.tmp_path = tmpPath;
			o.out_path = targetPath;
			o.gameobject = gameObject.name;
			o.callback = "onAndroidCallback";
			cls.CallStatic("doUnzipAssetFile","",JsonUtility.ToJson(o));
		}
	}
	#endif

	public class UnzipEventValue
	{
		public float progress;
		public int code;
		public UnzipEventValue(float progress, int code)
		{
			this.progress = progress;
			this.code = code;
		}
	}

	public static void StartUnzip(System.Action action=null)
	{
		AppAnalysis.recordAction ("action_zip_begin");
		Debug.Log("action_zip_begin");

		Log.I("start unzip res.zip", Log.Tag.RES);
		if (_show) 
		{
			Log.W("call StartUnzip more times", Log.Tag.Update);
			return;
		}

		do 
		{
			#if UNITY_ANDROID && !UNITY_EDITOR
			WWW www = new WWW ("jar:file://"+Application.dataPath+"!/assets/resinfo.txt");
			while(!www.isDone)
			{
				Thread.Sleep(100);
				continue;
			}
			string resinfo = www.text;
			#else
			string resinfo = null;
			string filePath = Application.streamingAssetsPath + "/resinfo.txt";
			if(File.Exists(filePath))resinfo = File.ReadAllText (filePath);
			#endif
			if (string.IsNullOrEmpty(resinfo))break;
			string[] ss = resinfo.Split ('|');
			int resZipVersion = int.Parse (ss [0]);
			int resZipSize = int.Parse (ss [1]);
			int resPart = int.Parse(ss[2]);
			string unzipTagFile = ResLoad.resPath + resZipVersion;
			if (File.Exists (unzipTagFile))break;
			if (Directory.Exists (ResLoad.resPath))Directory.Delete (ResLoad.resPath, true);
			GameObject go = GameObject.Find ("LoginUIRoot");
			go.AddComponent<UnzipManager> ().Init(resZipVersion, resZipSize, resPart, unzipTagFile, action);
			return;
		} while(false);
		if (action != null)action ();
	}

	public void Init(int resZipVersion, int resZipSize, int resPart, string unzipTagFile, System.Action callback)
	{
		_resZipVersion = resZipVersion;
		_resZipSize = resZipSize;
		_resPart = resPart;
		_unzipTagFile = unzipTagFile;
		_action = callback;
	}

	LoginRoot _loginRoot;
	string    _unzipTagFile;
	int       _resZipVersion;
	int       _resZipSize;
	int       _resPart;
	System.Action _action;
	static	bool _show;
	// Use this for initialization
	void Awake()
	{
		_show = true;
		_loginRoot = GetComponent<LoginRoot>();
		ShowUnzip (true);	
	}

	void Start () 
	{
		EventManager.single.registEventListener ("UnzipEvent", OnUnzipEvent);
		//解压资源/
		string tmpPath = Application.persistentDataPath + "/ResTmp/";
		Log.I("unzip start", Log.Tag.RES);
		#if UNITY_ANDROID && !UNITY_EDITOR
		callAndroidUnzip("res.zip", tmpPath, ResLoad.resPath);
		#else
		FileStream fs = new FileStream (Application.streamingAssetsPath + "/res.zip", FileMode.Open, FileAccess.Read);
		UnzipCach.unzipFile (fs, tmpPath, ResLoad.resPath, OnThreadCallback);
		#endif
	}

	void ShowUnzip(bool show)
	{
		_loginRoot.setLoginBtnEnable (!show);
		#if GAME_THIRD_SHILED
		_loginRoot._changeAccBtn.SetActive(false);
		#else
		_loginRoot._changeAccBtn.SetActive(!show);
		#endif
		_loginRoot._processBar.gameObject.SetActive (show);

		_loginRoot._processNum.gameObject.SetActive (show);
		_loginRoot._introduceInfo.gameObject.SetActive (show);
		_loginRoot.updateUnzipProgress (0);
		_loginRoot._introduceInfo.text = "正在解压资源...";
	}

	void OnDestroy()
	{
		_show = false;
		_action = null;
		EventManager.single.unregistEventListener ("UnzipEvent", OnUnzipEvent);
	}

	public void onAndroidCallback(string param)
	{
		if (param == "ok") {
			OnUnzipProgress (1, 0);
		} else if (param == "error") {
			OnUnzipProgress (1, 1);
		} else {
			int i = int.Parse(param);
			OnUnzipProgress (0.99f*i/_resZipSize, 0);
		}
	}

	void OnThreadCallback(float prog, int code)
	{
		EventManager.single.pushEvent("UnzipEvent",new UnzipEventValue(prog, code));
	}

	void OnUnzipEvent(EventManager.EventBase eb)
	{
		UnzipEventValue v = eb.eventValue as UnzipEventValue;
		OnUnzipProgress (v.progress, v.code);
	}

	void onCheckFileProgress(float percent)
	{
	}

	void OnUnzipProgress(float progress, int code)
	{
		if (code == 0) 
		{
			_loginRoot.updateUnzipProgress (progress);
			if (progress >= 1) 
			{//unzip ok
				List<XmlPatch.DFileInfo> fs=null;
				try
				{
					XmlPatch patch = new XmlPatch(Application.persistentDataPath+"/Res/");
					bool bcancel = false;
					fs = patch.listDownFiles(onCheckFileProgress,ref bcancel,_resPart);
				}
				catch(Exception e) 
				{
					Log.E(e);
					fs = null;
				}

				if(fs==null || fs.Count>0)
				{
					Log.E("unzip failed", Log.Tag.RES);
					MessageBox.showComfirm ("解压资源失败,请确保有足够的存储空间(>"+ToSize(_resZipSize)+")后重启游戏再试.", () => {Application.Quit ();});
					return;
				}

				File.WriteAllText(_unzipTagFile,"ok");
				ResLoad.SetVersionPart (_resZipVersion,_resPart);
				Log.I("^_^ unzip ok", Log.Tag.RES);
				ShowUnzip(false);
				ResMgr.Single.Reset();
				if (null!=_action)_action ();
				Destroy (this);
			}
		}
		else
		{//unzip failed
			Log.E("unzip failed", Log.Tag.RES);
			MessageBox.showComfirm ("解压资源失败,请确保有足够的存储空间(>"+ToSize(_resZipSize)+")后重启游戏再试", () => {
				Application.Quit ();
			});
		}	
	}

	string ToSize(int bytes)
	{
		if (bytes > 1024 * 1024 * 1024)
		{
			return (1.0f * bytes / (1024 * 1024 * 1024)).ToString("f2") + "GB";
		}
		else if (bytes > 1024 * 1024)
		{
			return (1.0f * bytes / (1024 * 1024)).ToString("f2") + "MB";
		}
		else if (bytes > 1024)
		{
			return (1.0f * bytes / 1024).ToString("f2") + "KB";
		}
		else
		{
			return (float)bytes + "B";
		}
	}
}
