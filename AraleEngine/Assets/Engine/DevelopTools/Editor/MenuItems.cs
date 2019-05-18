using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using UnityEngine.UI;
using Arale.Engine;
using System.Collections.Generic;

public class MenuItems{
    #region lua
	[MenuItem("开发工具/Lua/加密")]
	public static void encrydLua()
	{
        string outPath = dataOutPath()+"/lua";
		if (!Directory.Exists (outPath))Directory.CreateDirectory(outPath);
        string srcPath = Application.streamingAssetsPath + "Llua";
		Crypt.DoDirectoryCrypt (new DirectoryInfo(srcPath), new DirectoryInfo(outPath), "wanghuan", "*.lua");
        Debug.Log("lua加密完成");
	}

	[MenuItem("开发工具/Lua/解密")]
	public static void decryLua()
	{
        string outPath = dataOutPath()+"/lua1";
		if (!Directory.Exists (outPath))Directory.CreateDirectory(outPath);
        string srcPath = dataOutPath()+"/lua";
		Crypt.UnDirectoryCrypt (new DirectoryInfo(srcPath), new DirectoryInfo(outPath), "wanghuan");
	}
    #endregion

    #region protobuf
    [MenuItem("开发工具/Proto/生成CS")]
    public static void genProtoCS()
    {//导出cs协议
        string outPath = Application.dataPath + "/Demo/Script/Proto/";
        System.Diagnostics.Process p = new System.Diagnostics.Process();
        p.StartInfo.WorkingDirectory = Application.dataPath + "/../../Tools/ProtoGen";
        p.StartInfo.FileName = "proto-net.exe";
        p.StartInfo.Arguments = "-i:Proto/test.proto -o:"+outPath+"Test.cs";
        p.Start();
    }

    [MenuItem("开发工具/Proto/生成Java")]
    public static void genProtoJava()
    {//导出java协议
        System.Diagnostics.Process p = new System.Diagnostics.Process();
        p.StartInfo.WorkingDirectory = Application.dataPath + "/../../Tools/ProtoGen";
        p.StartInfo.FileName = "protoc.exe";
        p.StartInfo.Arguments = "--java_out=Proto Proto/test.proto";
        p.Start();
    }
    #endregion

	#region 资源打包
    static string dataOutPath()
    {
        string platform = "PC/";
        #if UNITY_IOS
        platform = "IOS/";
        #elif UNITY_ANDROID
        platform = "Android/";
        #endif
        return Application.dataPath + "/../Data/" + platform;
    }

    [MenuItem("开发工具/资源/标记资源")]
    public static void OneKeySetting()
    {
        //AssetAB.oneKeySetting ();
    }

    [MenuItem("开发工具/资源/创建资源")]
	public static void buildAB()
	{
        string outPath = dataOutPath();
        if (!Directory.Exists (dataOutPath()))Directory.CreateDirectory(outPath);
        BuildPipeline.BuildAssetBundles (outPath, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
        string manifestPath = outPath+"Data";
		string manifestDataPath = manifestPath + ".data";
		if (File.Exists (manifestDataPath))File.Delete (manifestDataPath);
		File.Move (manifestPath, manifestDataPath);
	}

    [MenuItem("开发工具/资源/创建版本")]
    public static void makeVersion()
    {
        string versionPath = dataOutPath() + "version.xml";
        XmlPatch patch = new XmlPatch(versionPath);
        patch.make(1);
    }

    [MenuItem("开发工具/资源/构建全资源包")]
    public static void makeFullZip()
    {
    }

    [MenuItem("开发工具/资源/构建精简资源包")]
    public static void makeMiniZip()
    {
    }

    [MenuItem("开发工具/资源/浏览资源")]
    public static void showAB()
    {
        Debug.LogError(dataOutPath());
        System.Diagnostics.Process.Start("Explorer.exe", dataOutPath());
    }

    [MenuItem("开发工具/资源/分包配置")]
    public static void configPart()
    {
        string path = Application.dataPath+"/ResPart.xml";
        string mono = EditorApplication.applicationContentsPath + "/../../MonoDevelop/bin/MonoDevelop.exe";
        System.Diagnostics.Process.Start(mono, path);
    }

    [MenuItem("开发工具/资源/清除测试资源")]
	public static void clearAB()
	{
		string resPath = Application.persistentDataPath+"/Res/";
		if (Directory.Exists (resPath))FileUtils.delFolder (resPath, true);
	}

    [MenuItem("开发工具/资源/清除测试缓存")]
    public static void clearResVer()
    {
        //UnityEngine.PlayerPrefs.DeleteKey(ResLoad.ResVerKey);
        UnityEngine.PlayerPrefs.Save();
    }

    [MenuItem("开发工具/资源/更新测试资源")]
	public static void refreshAB()
	{
		string resPath = Application.persistentDataPath+"/Res";
		if (Directory.Exists (resPath))FileUtils.delFolder (resPath, true);
        FileUtils.copy (dataOutPath(), resPath);
	}

    [MenuItem("开发工具/资源/校验测试更新")]
    public static void checkMD5()
    {
        string resPath = Application.persistentDataPath+"/Res/";
        XmlPatch patch = new XmlPatch(resPath);
        bool bcancel = false;
        List<XmlPatch.DFileInfo> fs = patch.listDownFiles(onCheckFileProgress,ref bcancel,null);
        for (int i = 0, max = fs.Count; i < max; ++i)
        {
            XmlPatch.DFileInfo df = fs [i];
            Debug.LogError (df.path);
        }
        Debug.Log ("check ok");
    }
    static void onCheckFileProgress(float percent){}
	#endregion

	#region atlas
	[MenuItem("DevelopTools/Atlas/Update")]
	public static void updateAtlas()
	{
		string atlasPath = Application.dataPath+"/Atlas/";
		Debug.Log ("atlas path="+atlasPath);
		string[] fs = Directory.GetFiles (atlasPath,"*.*",SearchOption.AllDirectories);
		for (int i = 0, max = fs.Length; i < max; ++i) 
		{
			if (fs [i].EndsWith (".meta"))
				continue;
			string atlasName = new DirectoryInfo(Path.GetDirectoryName (fs[i])).Name;
			string assetPath = FileUtils.toAssetsPath (fs [i]);
			TextureImporter ti = TextureImporter.GetAtPath(assetPath) as TextureImporter;
			if (ti.spritePackingTag == atlasName)
				continue;
			ti.textureType = TextureImporterType.Sprite;
			ti.spritePackingTag = atlasName;
			ti.mipmapEnabled = false;
			ti.SaveAndReimport ();
		}
	}

	[MenuItem("DevelopTools/Atlas/Move")]
	public static void moveAtlas()
	{
        string atlasPath = Application.dataPath+"/Atlas/";
        if (!Directory.Exists(atlasPath)) AssetDatabase.CreateFolder("Assets", "Atlas");
        string selPath = NGUIEditorTools.GetSelectionFolder ();
        string[] fs = Directory.GetFiles (selPath,"*.*",SearchOption.AllDirectories);
        AssetDatabase.StartAssetEditing ();
        for (int i = 0, max = fs.Length; i < max; ++i) 
        {
            if (fs [i].EndsWith (".meta"))
                continue;
            string assetPath = FileUtils.toAssetsPath (fs [i]);
            string assetName = Path.GetFileName (assetPath);
            TextureImporter ti = TextureImporter.GetAtPath(assetPath) as TextureImporter;
            if (ti==null||ti.textureType != TextureImporterType.Sprite)
                continue;
            string atlasName = ti.spritePackingTag;
            if (string.IsNullOrEmpty (atlasName)) 
            {//没有设置tag的不改变目录，等使用者正确设置tag后再进行move操作
                continue;
            }
            string subPath = atlasPath + atlasName + "/";
            if (!Directory.Exists(subPath)) AssetDatabase.CreateFolder("Assets/Atlas", atlasName);
            string newPath = FileUtils.toAssetsPath(subPath);
            //必须做延迟移动，因为前面建立的父目录还没建库
            EditorApplication.delayCall += () =>
            {
                string r = AssetDatabase.MoveAsset(assetPath, newPath + assetName);
                if (!string.IsNullOrEmpty(r)) Debug.LogError(r);
            };
        }
        AssetDatabase.StopAssetEditing ();
	}

    [MenuItem("DevelopTools/Atlas/Create")]
    public static void createAtlas()
    {
        string selPath = NGUIEditorTools.GetSelectionFolder ();
        DirectoryInfo dir = new DirectoryInfo (selPath);
        string atlasPath = selPath+"/"+dir.Name + ".asset";
        Atlas atlas = AssetDatabase.LoadAssetAtPath (atlasPath, typeof(Atlas)) as Atlas;
        if (atlas == null)
        {
            atlas = ScriptableObject.CreateInstance<Atlas>();
            List<Sprite> sps = new List<Sprite> ();
            FileInfo[] fis = dir.GetFiles ();
            foreach (FileInfo fi in fis)
            {
                Object[] os = AssetDatabase.LoadAllAssetsAtPath (selPath+fi.Name);
                for (int i = 0; i < os.Length; ++i)
                {
                    Sprite sp = os [i] as Sprite;
                    if (sp == null)continue;
                    sps.Add (sp);
                }
            }
            atlas._sprites = sps.ToArray ();

            AssetDatabase.CreateAsset (atlas, atlasPath);
        }
    }
    #endregion

	[MenuItem("DevelopTools/ExportSprite")]
	static void exportSrite()
	{
		string exportDir = "Assets/Resources/Sprite/";
		Sprite o = Selection.activeObject as Sprite;
		string path = AssetDatabase.GetAssetPath (o);
		if (!File.Exists(path))
		{
			Debug.LogError ("select the export sprite,sprite is texture sub node");
			return;
		}

		string dir = Application.dataPath+"/Resources/Sprite/";
		if (!Directory.Exists (dir))Directory.CreateDirectory (dir);

		path = exportDir+System.IO.Path.GetFileNameWithoutExtension (path)+".prefab";
		GameObject go = new GameObject (o.name);
		AssetRef ar = go.AddComponent<AssetRef> ();
		ar.mAsset = o;
		PrefabUtility.CreatePrefab (path, go);
		AssetDatabase.SaveAssets ();
		GameObject.DestroyImmediate (go);
	}

	[MenuItem("DevelopTools/Create/Window")]
	static void createWindow()
	{
		GameObject pa = Selection.activeObject as GameObject;
		GameObject go = new GameObject ("Window", typeof(RectTransform));
		RectTransform rt = go.transform as RectTransform;
		rt.SetParent (pa.transform, false);
		rt.anchorMin = Vector2.zero;
		rt.anchorMax = Vector2.one;
		rt.offsetMin = new Vector2(0,0);
		rt.offsetMax = new Vector2(0,0);
		go.AddComponent<CanvasRenderer> ();
		go.AddComponent<Image> ().color = new Color(0,0,0,180.0f/255);
		Canvas c = go.AddComponent<Canvas> ();
		c.overrideSorting = true;
		go.AddComponent<GraphicRaycaster> ();
	}
        
    [MenuItem("DevelopTools/CopyCode")]
    static void CopyCode()
    {
        FileUtils.copy("E:/project/Demo/Assets/Engine/Game", "F:/Demo/Assets/Engine/Game");
    }

    [MenuItem("DevelopTools/UnloadEditorAsset")]
    static void unloadEditorAsset()
    {
        EditorUtility.UnloadUnusedAssetsImmediate();
    }

    [MenuItem("DevelopTools/UpdateIncludeShader")]
    static void setIncludeShader()
    {
        //BatchMode.setPlayerIncludeShader ();
    }
}
