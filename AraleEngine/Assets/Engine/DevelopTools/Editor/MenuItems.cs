using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using UnityEngine.UI;
using Arale.Engine;

public class MenuItems{
	[MenuItem("DevelopTools/Lua/Build")]
	public static void buildLua()
	{
		string outPath = Application.dataPath+"/../Data/lua";
		if (!Directory.Exists (outPath))Directory.CreateDirectory(outPath);
		string srcPath = Application.dataPath + "/lua";
		Crypt.DoDirectoryCrypt (new DirectoryInfo(srcPath), new DirectoryInfo(outPath), "wanghuan");
	}

	[MenuItem("DevelopTools/Lua/Decry")]
	public static void decryLua()
	{
		string outPath = Application.dataPath+"/../Data/lua1";
		if (!Directory.Exists (outPath))Directory.CreateDirectory(outPath);
		string srcPath = Application.dataPath + "/../Data/lua";
		Crypt.UnDirectoryCrypt (new DirectoryInfo(srcPath), new DirectoryInfo(outPath), "wanghuan");
	}

	#region assetbundle
	[MenuItem("DevelopTools/AB/Build")]
	public static void buildAB()
	{
		string outPath = Application.dataPath+"/../Data/";
		if (!Directory.Exists (outPath))
			Directory.CreateDirectory(outPath);
		Debug.Log ("ab outpath="+outPath);
		BuildPipeline.BuildAssetBundles (outPath, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
		string manifestPath = Application.dataPath + "/../Data/Data";
		string manifestDataPath = manifestPath + ".data";
		if (File.Exists (manifestDataPath))File.Delete (manifestDataPath);
		File.Move (manifestPath, manifestDataPath);
	}

	[MenuItem("DevelopTools/AB/Clear")]
	public static void clearAB()
	{
		string resPath = Application.persistentDataPath+"/Res/";
		if (Directory.Exists (resPath))FileUtils.delFolder (resPath, true);
	}

	[MenuItem("DevelopTools/AB/Refresh")]
	public static void refreshAB()
	{
		string resPath = Application.persistentDataPath+"/Res";
		string outPath = Application.dataPath+"/../Data";
		if (Directory.Exists (resPath))FileUtils.delFolder (resPath, true);
		FileUtils.copy (outPath, resPath);
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
		if (!Directory.Exists (atlasPath))Directory.CreateDirectory (atlasPath);

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
			{
				AssetDatabase.MoveAsset (assetPath, atlasPath);
				continue;
			}
			string subPath = atlasPath + atlasName + "/";
			if (!Directory.Exists (subPath))
				Directory.CreateDirectory (subPath);
			string newPath = FileUtils.toAssetsPath(subPath);
			string r = AssetDatabase.MoveAsset (assetPath, newPath+assetName);
			if (r != null)Debug.LogError (r);
		}
		AssetDatabase.StopAssetEditing ();
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
}
