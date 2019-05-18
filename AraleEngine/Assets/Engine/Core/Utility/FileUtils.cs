using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using System;

namespace Arale.Engine
{
        
    public class FileUtils{
    	#if UNITY_ANDROID
    	public const string platform = "android";
    	public const string appExt = ".apk";
    	#elif UNITY_IPHONE
    	public const string platform = "iphone";
    	public const string appExt = ".ipa";
    	#else
    	public const string platform = "standalone";
    	public const string appExt = ".exe";
    	#endif

    	public static string resZipName
    	{
    		get{return platform+".zip";}
    	}

    	public static string resPartZipName
    	{
    		get{return platform+".part.zip";}
    	}

    	public static string resPartCfgName
    	{
    		get{return platform+".part.txt";}
    	}

    	public static string resBaseCfgName
    	{
    		get{return platform+".base.txt";}
    	}

    	public static void createDirectory(string path)
    	{
    		if(Directory.Exists(path))return;
    		Directory.CreateDirectory(path);//支持递归创建/
    	}

    	public static void copy(string from, string to)
    	{
            from = from.TrimEnd('\\');
    		if(File.Exists(from))
    		{
    			string dir = System.IO.Path.GetDirectoryName(to);
    			createDirectory(dir);
    			File.Copy(from,to);
    		}
    		else if(Directory.Exists(from))
    		{
    			copyFolder(from, to);
    		}
    		else
    		{
    			throw new IOException("source path not exit:source="+from);
    		}
    	}

    	static void copyFolder(string dir, string target)
    	{
    		if (!Directory.Exists(target))
    		{
    			Directory.CreateDirectory(target);
    		} 
    		
    		string[] fileName = Directory.GetFiles(dir);
    		foreach(string fi in fileName)
    		{
    			string filePathTemp = target + "/" + fi.Substring(dir.Length + 1);
    			File.Copy(fi, filePathTemp, true); 
    		}
    		
    		string[] directionName = Directory.GetDirectories(dir);
    		foreach (string di in directionName)
    		{
    			string directionPathTemp = target + "/" + di.Substring(dir.Length + 1);
    			copyFolder(di, directionPathTemp); 
    		}
    	}

    	public static void move(string from, string to, bool ignoreSourceError=false)
    	{
    		if(File.Exists(from))
    		{
    			string dir = System.IO.Path.GetDirectoryName(to);
    			createDirectory(dir);
    			File.Move(from,to);
    		}
    		else if(Directory.Exists(from))
    		{
    			moveFolder(from, to);
    		}
    		else
    		{
    			if(!ignoreSourceError)throw new IOException("source path not exit:source="+from);
    #if UNITY_EDITOR
    			Debug.Log("source path not exit:source="+from);
    #endif
    		}
    	}

    	static void moveFolder(string dir, string target)
    	{
    		if (!Directory.Exists(target))
    		{
    			Directory.CreateDirectory(target);
    		}

    		string[] fileName = Directory.GetFiles(dir);
    		foreach(string fi in fileName)
    		{
    			string filePathTemp = target + "/" + fi.Substring(dir.Length + 1);
    			File.Move(fi, filePathTemp); 
    		}
    		
    		string[] directionName = Directory.GetDirectories(dir);
    		foreach (string di in directionName)
    		{
    			string directionPathTemp = target + "/" + di.Substring(dir.Length + 1);
    			moveFolder(di, directionPathTemp); 
    		}
    	}

    	public delegate bool FilterFunc (string name, bool isfile);//返回真删除
    	public static void delFolder(string path, bool recursive, FilterFunc filterFunc=null)
    	{
    		if (filterFunc == null)
            {
                Directory.Delete(path, recursive);
            }
            else
            {
                bool delRoot = true;
                DirectoryInfo di = new DirectoryInfo(path);
                FileInfo[] fis = di.GetFiles();
                for (int i = 0, max = fis.Length; i < max; ++i)
                {
                    if (filterFunc (fis [i].Name, true))
                        fis [i].Delete ();
                    else
                        delRoot = false;
                }

                if (recursive)
                {
                    DirectoryInfo[] dis = di.GetDirectories();
                    for (int i = 0, max = dis.Length; i < max; ++i)
                    {
                        if (filterFunc (dis [i].Name, false))
                            delFolder (dis [i].FullName, true, filterFunc);
                        else
                            delRoot = false;
                    }
                }
                else
                {
                    if (di.GetDirectories().Length > 0)delRoot = false;
                }
                if(delRoot)di.Delete();
            }
    	}

        public delegate void DealFunc(FileInfo fi);
        public static void enumFiles(string dirPath, bool recursive, DealFunc dealFunc)
        {
            DirectoryInfo di = new DirectoryInfo(dirPath);
            FileInfo[] fis = di.GetFiles();
            for (int i = 0, max = fis.Length; i < max; ++i)
            {
                dealFunc (fis [i]);
            }

            if (recursive)
            {
                DirectoryInfo[] dis = di.GetDirectories();
                for (int i = 0, max = dis.Length; i < max; ++i)
                {
                    enumFiles (dis [i].FullName, true, dealFunc);
                }
            }
        }

    	const int CHECK_BUF_SIZE = 4*1024*1024;
    	public delegate void OnMd5CheckProgress(float progress);
    	public static bool checkBigFileMd5(string path, string md5, OnMd5CheckProgress onProgress)
    	{
    		char[] cmd5 = md5.ToCharArray ();
    		int cmd5offset = 0;
    		int cmd5Len = cmd5.Length;
    		FileStream f = null;
    		try
    		{
    			System.Security.Cryptography.MD5CryptoServiceProvider oMD5Hasher = new System.Security.Cryptography.MD5CryptoServiceProvider();
    			f = File.OpenRead (path);
    			int len = (int)f.Length;
    			byte[] buf = new byte[CHECK_BUF_SIZE];
    			int n = f.Read (buf, 0, CHECK_BUF_SIZE);
    			while(n>0)
    			{
    				byte[] m = oMD5Hasher.ComputeHash(buf, 0, n);
    				for(int i=0; i<m.Length; ++i)
    				{
    					char[] tm = m[i].ToString("x2").ToCharArray();
    					if(cmd5[cmd5offset++]!=tm[0])throw new Exception("md5 not match offset="+(cmd5offset-1));
    					if(cmd5[cmd5offset++]!=tm[1])throw new Exception("md5 not match offset="+(cmd5offset-1));
    				}
    				onProgress(1.0f*cmd5offset/cmd5Len);
    				n = f.Read (buf, 0, CHECK_BUF_SIZE);
    			}
    			f.Close();
    			return cmd5offset==cmd5Len;
    		}
    		catch(Exception e)
    		{
    #if UNITY_EDITOR
    			Debug.LogException(e);
    #endif
    			if(f!=null)f.Close ();
    			return false;
    		}
    	}

    	public static string createBigFileMd5(string path)
    	{
    		StringBuilder md5 = new StringBuilder();
    		System.Security.Cryptography.MD5CryptoServiceProvider oMD5Hasher = new System.Security.Cryptography.MD5CryptoServiceProvider();
    		FileStream f = File.OpenRead (path);
    		byte[] buf = new byte[CHECK_BUF_SIZE];
    		int n = f.Read (buf, 0, CHECK_BUF_SIZE);
    		while(n>0)
    		{
    			byte[] m = oMD5Hasher.ComputeHash(buf, 0, n);
    			for (int i = 0; i < m.Length; i++)
    			{  
    				md5.Append(m[i].ToString("x2"));  
    			}    
    			n = f.Read (buf, 0, CHECK_BUF_SIZE);
    		}
    		f.Close();
    		return md5.ToString ();
    	}

    	//取文件md5值/
    	public static string GetMd5Hash(string pathName)
    	{
    		string strHashData = "";
    		System.Security.Cryptography.MD5CryptoServiceProvider oMD5Hasher = new System.Security.Cryptography.MD5CryptoServiceProvider();
    		try
    		{
    			FileStream file = new FileStream(pathName, FileMode.Open);
    			byte[] retVal = oMD5Hasher.ComputeHash(file);
    			file.Close();
    			strHashData = System.BitConverter.ToString(retVal);
    			strHashData = strHashData.Replace("-", "");
    		}
    		catch (System.Exception ex)
    		{
    			Log.e (ex);
    		}
    		return strHashData;
    	}

    	public static string toAssetsPath(string absolutePath)
    	{
    		string path = absolutePath.Replace ('\\', '/');
    		int i = path.IndexOf ("Assets/");
    		return path.Substring (i);
    	}

    	public static string toResourcesPath(string absolutePath)
    	{
    		string path = absolutePath.Replace ('\\', '/');
    		int i = path.LastIndexOf ("Resources/")+10;
    		return path.Substring (i);
    	}
    }

}
