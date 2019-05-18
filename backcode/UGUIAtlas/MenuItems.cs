using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using Scripts.CoreScripts.Core;
using ICSharpCode.SharpZipLib.Zip;
using System.Collections.Generic;

public class MenuItems{
	#region assetbundle
	[MenuItem("DevelopTools/AB/OneKeySetting")]
	public static void OneKeySetting()
	{
		AssetAB.oneKeySetting ();
	}

	[MenuItem("DevelopTools/AB/Build")]
	public static void buildAB()
	{
        BatchMode.buildAB();
    }

    [MenuItem("DevelopTools/AB/Show")]
    public static void showAB()
    {
        System.Diagnostics.Process.Start("Explorer.exe", BatchMode.mTargetPath);
    }

	[MenuItem("DevelopTools/AB/ClearRes")]
	public static void clearAB()
	{
		string resPath = Application.persistentDataPath+"/Res/";
		if (Directory.Exists (resPath))FileUtils.delFolder (resPath, true);
	}

    [MenuItem("DevelopTools/AB/ClearVer")]
    public static void clearVer()
    {
		UnityEngine.PlayerPrefs.DeleteKey(ResLoad.ResVerKey);
        UnityEngine.PlayerPrefs.Save();
    }

    [MenuItem("DevelopTools/AB/Refresh")]
	public static void refreshAB()
	{
		string resPath = Application.persistentDataPath+"/Res";
		if (Directory.Exists (resPath))FileUtils.delFolder (resPath, true);
		FileUtils.copy (BatchMode.mTargetPath, resPath);
	}

    [MenuItem("DevelopTools/AB/MakeVersion")]
    public static void makeVersion()
    {
        BatchMode.makeVersion(1);
    }

    [MenuItem("DevelopTools/AB/MakeDiff")]
    public static void makeDiff()
    {
        BatchMode.makeDiff();
    }

	[MenuItem("DevelopTools/AB/MakeZip")]
	public static void makeZip()
	{
		BatchMode.makeZip ();
	}

	[MenuItem("DevelopTools/AB/checkMD5")]
	public static void checkMD5()
	{
		string resPath = Application.persistentDataPath+"/Res/";
		XmlPatch patch = new XmlPatch(resPath);
		bool bcancel = false;
		List<XmlPatch.DFileInfo> fs = patch.listDownFiles(onCheckFileProgress,ref bcancel);
		for (int i = 0, max = fs.Count; i < max; ++i)
		{
			XmlPatch.DFileInfo df = fs [i];
			Debug.LogError (df.path);
		}
		Debug.Log ("check ok");
	}
	static void onCheckFileProgress(float percent)
	{
	}
    #endregion

	#region lua
	[MenuItem("DevelopTools/Lua/build")]
	public static void buildLua()
	{
		BatchMode.buildLua ();
	}

	[MenuItem("DevelopTools/Lua/Encrypt")]
	public static void encryptLua()
	{
		string outPath = BatchMode.mTargetPath + "/lua/";
		DirectoryInfo din = new DirectoryInfo (Application.streamingAssetsPath + "/Lua/");
		DirectoryInfo dout = new DirectoryInfo (BatchMode.mTargetPath + "/lua/");
		if(!dout.Exists)Directory.CreateDirectory (dout.FullName);
		Core.Utils.Crypt.DoDirectoryCrypt(din, dout, LuaRoot.ENCODESTRING, "*.lua");
		UnityEngine.Debug.Log ("encode lua ok!");
	}
	#endregion

    #region publish
    [MenuItem("DevelopTools/Publish/App")]
    public static void publishApp()
    {
        BatchMode.publishApp();
    }

    [MenuItem("DevelopTools/Publish/Res")]
    public static void publishRes()
    {
		BuildWindow.showBuildWindow ();
    }
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
        Sprite o = Selection.activeObject as Sprite;
        string path = AssetDatabase.GetAssetPath(o);
        if (!File.Exists(path))
        {
            Debug.LogError("select the export sprite,sprite is texture sub node");
            return;
        }

        path = "Assets/Resources/" + System.IO.Path.GetFileNameWithoutExtension(path) + ".prefab";
        GameObject go = new GameObject(o.name);
        AssetRef ar = go.AddComponent<AssetRef>();
        ar._asset = o;
        PrefabUtility.CreatePrefab(path, go);
        AssetDatabase.SaveAssets();
        GameObject.DestroyImmediate(go);
    }

    [MenuItem("DevelopTools/UnloadEditorAsset")]
    static void unloadEditorAsset()
    {
        EditorUtility.UnloadUnusedAssetsImmediate();
    }

	[MenuItem("DevelopTools/UpdateIncludeShader")]
	static void setIncludeShader()
	{
		
		BatchMode.setPlayerIncludeShader ();
	}
}
