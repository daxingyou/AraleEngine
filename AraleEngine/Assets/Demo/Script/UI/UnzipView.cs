using UnityEngine;
using System.Collections;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using System.Threading;
using System;
using Arale.Engine;
using System.Collections.Generic;
using UnityEngine.UI;

public class UnzipView : MonoBehaviour {
	#if UNITY_ANDROID && !UNITY_EDITOR
	private const string SDK_JAVA_CLASS = "YourJavaClassFullName";
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

	bool StartUnzip()
	{
		Log.i("start unzip res.zip", Log.Tag.RES);
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
            int[] resPart = ResLoad.str2IntArray(ss[2]);
			string unzipTagFile = ResLoad.resPath + resZipVersion;
			if (File.Exists (unzipTagFile))break;
			if (Directory.Exists (ResLoad.resPath))Directory.Delete (ResLoad.resPath, true);
            _resZipVersion = resZipVersion;
            _resZipSize = resZipSize;
            _resPart = resPart;
            _unzipTagFile = unzipTagFile;
			return true;
		} while(false);
        EventMgr.single.SendEvent(GRoot.EventResUnzip, true);
        return false;
	}

    public Slider progressBar;
    public Text   unzipInfo;
    public Button quitBtn;

	string    _unzipTagFile;
	int       _resZipVersion;
	int       _resZipSize;
	int[]     _resPart;
	// Use this for initialization
	void Start () 
	{
        quitBtn.onClick.AddListener( delegate(){ Application.Quit ();});
        EventMgr.single.AddListener ("UnzipEvent", OnUnzipEvent);
        if (!StartUnzip())return;

        progressBar.value = 0;
        unzipInfo.text = "正在解压资源...";
		//解压资源/
		string tmpPath = Application.persistentDataPath + "/ResTmp/";
		#if UNITY_ANDROID && !UNITY_EDITOR
		callAndroidUnzip("res.zip", tmpPath, ResLoad.resPath);
		#else
		FileStream fs = new FileStream (Application.streamingAssetsPath + "/res.zip", FileMode.Open, FileAccess.Read);
		UnzipCach.unzipFile (fs, tmpPath, ResLoad.resPath, OnThreadCallback);
		#endif
	}

	void OnDestroy()
	{
        EventMgr.single.RemoveListener ("UnzipEvent", OnUnzipEvent);
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
        EventMgr.single.PostEvent("UnzipEvent",new UnzipEventValue(prog, code));
	}

	void OnUnzipEvent(EventMgr.EventData eb)
	{
		UnzipEventValue v = eb.data as UnzipEventValue;
		OnUnzipProgress (v.progress, v.code);
	}

	void onCheckFileProgress(float percent)
	{
	}

	void OnUnzipProgress(float progress, int code)
	{
		if (code == 0) 
		{
            progressBar.value = progress;
            unzipInfo.text = string.Format("正在解压资源...{0:F2}%",100f*progress);
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
					Log.e(e);
					fs = null;
				}

				if(fs==null || fs.Count>0)
				{
					Log.e("unzip failed", Log.Tag.RES);
                    unzipInfo.text = "解压资源失败,请确保有足够的存储空间(>" + ToSize(_resZipSize) + ")后重启游戏再试";
                    quitBtn.gameObject.SetActive(true);
					return;
				}

				File.WriteAllText(_unzipTagFile,"ok");
				ResLoad.setVersionPart (_resZipVersion,_resPart);
				Log.i("^_^ unzip ok", Log.Tag.RES);
                EventMgr.single.SendEvent(GRoot.EventResUnzip, true);
			}
		}
		else
		{//unzip failed
			Log.e("unzip failed", Log.Tag.RES);
            unzipInfo.text = "解压资源失败,请确保有足够的存储空间(>" + ToSize(_resZipSize) + ")后重启游戏再试";
            quitBtn.gameObject.SetActive(true);
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
