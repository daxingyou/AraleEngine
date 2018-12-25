using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using Arale.Engine;

public class TestRes : MonoBehaviour {
	public string abPath = null;//相对资源根目录/
	public int times;

	void OnGUI() 
	{
		float y = 0;
		if (GUI.Button (new Rect (200, y, 200, 50), "ClearUnusedAsset")) {
			Resources.UnloadUnusedAssets ();
		}
		if (GUI.Button (new Rect (0, y, 200, 50), "LoadFromMemory")) {
			Debug.Log ("----------------------------LoadFromMemory");
			LoadFromMemory(abPath);
		}

		if (GUI.Button (new Rect (0, y+=50, 200, 50), "LoadFromMemoryAsync")) {
			Debug.Log ("----------------------------LoadFromMemoryAsync");
			StartCoroutine(loadFromMemoryAsync(abPath));
		}

		if (GUI.Button (new Rect (0, y+=50, 200, 50), "loadFromFile")) {
			Debug.Log ("----------------------------loadFromFile");
			loadFromFile(abPath);
		}

		if (GUI.Button (new Rect (0, y+=50, 200, 50), "loadFromFileAsync")) {
			Debug.Log ("----------------------------loadFromFileAsync");
			loadFromFileAsync(abPath);
		}

		if (GUI.Button (new Rect (0, y+=50, 200, 50), "LoadFromCacheOrDownload")) {
			Debug.Log ("----------------------------LoadFromCacheOrDownload");
			StartCoroutine(createAssetByLoadFromCacheOrDownload(abPath));
		}

		if (GUI.Button (new Rect (0, y+=50, 200, 50), "WWW")) {
			Debug.Log ("----------------------------WWW");
			StartCoroutine(createAssetByWWW(abPath));
		}

		if (GUI.Button (new Rect (0, y+=50, 200, 50), "createAssetByResLoadSync")) {
			Debug.Log ("----------------------------createAssetByResLoadSync");
			createAssetByResLoadSync(abPath);
		}

		if (GUI.Button (new Rect (0, y+=50, 200, 50), "createAssetByResLoadAsync")) {
			Debug.Log ("----------------------------createAssetByResLoadAsync");
			createAssetByResLoadAsync(abPath);
		}

		if (GUI.Button (new Rect (0, y+=50, 200, 50), "Test Resource Atlas ")) {
			Object  o = Resources.Load ("UI/TestRes");
			GameObject go = GameObject.Instantiate (o) as GameObject;
			go.transform.parent = transform;
			go.transform.localPosition = Vector3.zero;
			go.transform.localRotation = Quaternion.identity;
		}

		if (GUI.Button (new Rect (0, y+=50, 200, 50), "Test Assetbundle Atlas ")) {
			AssetBundle ab = ResLoad.get ("atlas/genera",ResideType.InGame).assetBundle();
			GameObject go = ResLoad.get ("UI/TestRes").gameObject ();
			go.transform.parent = transform;
			go.transform.localPosition = Vector3.zero;
			go.transform.localRotation = Quaternion.identity;
			Image[] i = go.GetComponentsInChildren<Image>(true);
			i [3].sprite = ab.LoadAsset<Sprite> ("Btn_Blue");
			ResLoad.clearByPath("atlas/genera");
		}
	}

	void LoadFromMemory(string path)
	{
		Profiler.BeginSample("LoadFromMemory");
		path = ResLoad.resPath + path + ".data";
		byte[] buf = File.ReadAllBytes(path);
		float t1 = Time.realtimeSinceStartup;
		for(int i=0;i<times;++i)
		{
			float t = Time.realtimeSinceStartup;
			AssetBundle ab = AssetBundle.LoadFromMemory(buf);
			ab.LoadAllAssets();
			ab.Unload (false);
			Debug.Log ("use time="+(Time.realtimeSinceStartup-t));
		}
		Debug.Log ("use time="+(Time.realtimeSinceStartup-t1));
		Profiler.EndSample();
	}

	IEnumerator loadFromMemoryAsync(string path)
	{	
		path = ResLoad.resPath + path + ".data";
		byte[] buf = File.ReadAllBytes(path);
		for(int i=0;i<times;++i)
		{
			float t = Time.realtimeSinceStartup;
			AssetBundleCreateRequest cr = AssetBundle.LoadFromMemoryAsync(buf);
			yield return cr;
			AssetBundle ab = cr.assetBundle;
			ab.LoadAllAssets();
			ab.Unload(false);
			Debug.Log ("use time="+(Time.realtimeSinceStartup-t));
		}
	}

	void loadFromFile(string path)
	{
		Profiler.BeginSample("LoadFromFile");
		path = ResLoad.resPath + path + ".data";
		for(int i=0;i<times;++i)
		{
			float t = Time.realtimeSinceStartup;
			AssetBundle ab = AssetBundle.LoadFromFile(path);
			ab.LoadAllAssets();
			ab.Unload (false);
			Debug.Log ("use time="+(Time.realtimeSinceStartup-t));
		}
		Profiler.EndSample();
	}

	IEnumerator loadFromFileAsync(string path)
	{
		Profiler.BeginSample("LoadFromFileAsync");
		path = ResLoad.resPath + path + ".data";
		for(int i=0;i<times;++i)
		{
			float t = Time.realtimeSinceStartup;
			AssetBundleCreateRequest cr = AssetBundle.LoadFromFileAsync(path);
			yield return cr;
			AssetBundle ab = cr.assetBundle;
			ab.LoadAllAssets();
			ab.Unload (false);
			Debug.Log ("use time="+(Time.realtimeSinceStartup-t));
		}
		Profiler.EndSample();
	}

	IEnumerator createAssetByLoadFromCacheOrDownload(string path)
	{
		Caching.CleanCache ();
		path = "file:///"+ResLoad.resPath + path + ".data";
		for(int i=0;i<times;++i)
		{
			float t = Time.realtimeSinceStartup;
			WWW w = WWW.LoadFromCacheOrDownload(path, 1);
			yield return w;
			AssetBundle ab = w.assetBundle;
			ab.LoadAllAssets();
			ab.Unload(false);
			Debug.Log ("use time="+(Time.realtimeSinceStartup-t));
		}
	}

	IEnumerator createAssetByWWW(string path)
	{
		path = "file:///"+ResLoad.resPath + path + ".data";
		for(int i=0;i<times;++i)
		{
			float t = Time.realtimeSinceStartup;
			WWW w = new WWW(path);
			yield return w;
			AssetBundle ab = w.assetBundle;
			ab.LoadAllAssets();
			ab.Unload(false);
			Debug.Log ("use time="+(Time.realtimeSinceStartup-t));
		}

	}
	
	void createAssetByResLoadSync(string path)
	{
		Caching.CleanCache ();
		float t1 = Time.realtimeSinceStartup;
		for(int i=0;i<times;++i)
		{
			float t = Time.realtimeSinceStartup;
			Object o = ResLoad.get(path,ResideType.InGame).asset<Object>();
			Debug.Log ("use time="+(Time.realtimeSinceStartup-t));
		}
		Debug.Log ("use time="+(Time.realtimeSinceStartup-t1));
	}

	float tResLoad;
    void onLoadFinish(ResLoad resLoad)
	{
		Debug.Log ("use time="+(Time.realtimeSinceStartup-tResLoad));
	}
	void createAssetByResLoadAsync(string path)
	{
		Caching.CleanCache ();
		tResLoad = Time.realtimeSinceStartup;
		for(int i=0;i<times;++i)
		{
			ResLoad.get(path).asyncLoad(onLoadFinish);
		}
	}
}
