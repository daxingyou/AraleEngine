using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Reflection;
using System.Text.RegularExpressions;
using System.IO;
using System;

public class DllReplace
{
	string   mDllGuid;
	Assembly mAssembly;
	public DllReplace(string dllPath)
	{
		mDllGuid = AssetDatabase.AssetPathToGUID(dllPath);
		if (string.IsNullOrEmpty (mDllGuid))
		{
			Debug.LogError (dllPath+"不存在");
			return;
		}
		string assemblyName = Path.GetFileNameWithoutExtension(dllPath);
		Assembly assembly = Assembly.Load (assemblyName);
	}

	public void replacePrefabs(string path)
	{
		DirectoryInfo dir = new DirectoryInfo(path);
		FileInfo[] fis = dir.GetFiles("*.prefab",SearchOption.AllDirectories);
		for (int i = 0; i < fis.Length; ++i)
		{
			FileInfo fi = fis [i];
			replacePrefab (fi.FullName.Substring (Application.dataPath.Length-6));
		}
	}

	public void replacePrefab(string prefabPath)
	{
		//获取prefab上绑定的mono脚本
		GameObject go = AssetDatabase.LoadAssetAtPath<GameObject> (prefabPath);
		if (go == null)return;
		MonoBehaviour[] mbs = go.GetComponentsInChildren<MonoBehaviour> (true);
		//替换非dll脚本
		try
		{
			string txt = File.ReadAllText (prefabPath);
			string newTXT = new string (txt.ToCharArray ());
			bool dirty=false;
			int i = 0;
			do
			{
				i = txt.IndexOf("m_Script:", i+1);
				if(i<0)break;
				int b = i;
				int e = txt.IndexOf("}", b);
				string item = txt.Substring(b, e-b+1);
				//通过正则表达式解析出fileID和guid
				Regex r = new Regex(@"m_Script: {fileID: (?<fid>[\s\S]*?), guid: (?<gid>[\s\S]*?), type: 3}");
				Match m = r.Match(item);
				if(m.Success)
				{
					string fid = m.Groups["fid"].Value;
					string gid = m.Groups["gid"].Value;
					if(fid!="11500000")continue;//已经是dll脚本无需替换
					string typeName = Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(gid));
					//获取文件名找到脚本class类型
					for (int j = 0; j < mbs.Length; ++j)
					{
						System.Type t = mbs [j].GetType ();
						if(t.Name == typeName && null!=mAssembly.GetType(t.FullName))
						{//dll中有该类型才替换
							string newFID = genAssetFileID(t).ToString();
							string newItem = string.Format("m_Script: {0}fileID: {1}, guid: {2}, type: 3{3}", '{',newFID, mDllGuid,'}');
							newTXT = newTXT.Replace(item, newItem);
							dirty=true;
						}
					}
				}
			} while (i > 0);
			if(dirty)File.WriteAllText(prefabPath, newTXT);
		}
		catch(System.Exception e)
		{
			Debug.LogError (e.Message);
			Debug.LogError ("脚本替换失败 path="+prefabPath);
		}
	}
		
	public static int genAssetFileID(Type t) 
	{  
		string toBeHashed = "s\0\0\0" + t.Namespace + t.Name; 
		byte[] hashed = Blood.COM.Security.MD4.GetByteHashFromBytes(System.Text.Encoding.UTF8.GetBytes(toBeHashed)); 

		int result = 0; 

		for (int i = 3; i >= 0; --i) 
		{  
			result <<= 8; 
			result |= hashed[i]; 
		}  

		return result; 
	}

    [MenuItem("开发工具/Dll/替换为Dll脚本",false,3)]
    static void replaceDllScript()
    {
        UnityEngine.Object o = Selection.activeObject;
        if (o != null)
        {
            DllReplace dr = new DllReplace ("Assets/Plugins/AraleEngine.dll");
            string path = AssetDatabase.GetAssetPath (o);
            if (File.Exists (Application.dataPath+path.Substring(6)))
            {
                dr.replacePrefab (path);
            }
            else
            {
                dr.replacePrefabs (path);
            }
        }
    }

}