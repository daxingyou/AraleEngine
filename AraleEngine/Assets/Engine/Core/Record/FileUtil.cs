using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Arale.Engine
{
    
	public class FileUtil
	{
		public static string GetFileMD5(string path)
		{
			if(!File.Exists(path))
			{
				return "null";
			}
			
			try
			{
				FileStream get_file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
				System.Security.Cryptography.MD5CryptoServiceProvider get_md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
				byte[] hash_byte = get_md5.ComputeHash(get_file);
				string resule = System.BitConverter.ToString(hash_byte).Replace("-", "");
				get_file.Dispose();
				return resule.ToLower();
			}
			catch (Exception e)
			{
				Log.e(e,Log.Tag.Default,e);
				return "null";
			}
		}
		
		public static Dictionary<string, string> ReadDictionary(string path)
		{
			Dictionary<string, string> dic = new Dictionary<string, string>();
			
			try
			{
				StreamReader sr = File.OpenText(path);
				string line = null;
				while (null != (line = sr.ReadLine()))
				{
					line = line.Trim();
					if (string.IsNullOrEmpty(line) || line.StartsWith("#") || line.StartsWith("//"))
					{
						continue;
					}
					
					string[] splites = line.Split(new char[] { '=' }, 2);
					if (2 == splites.Length)
					{
						try
						{
							dic.Add(splites[0].Trim(), splites[1].Trim());
						}
						catch (System.Exception ex)
						{
							Log.e(ex,Log.Tag.Default,ex);
						}
					}
					else
					{
                        Log.e("Parse Error: " + line);
					}
				}
			}
			catch (Exception e)
			{
				Log.e(e,Log.Tag.Default,e);
			}
			
			return dic;
			
		}
		
		public static void CreateTextFileWithDic(string path, Dictionary<string, string> contentDic)
		{
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			
			try
			{
				StreamWriter sw = File.CreateText(path);
				foreach (string key in contentDic.Keys)
				{
					sw.WriteLine(key + "=" + contentDic[key]);
				}
				sw.Flush();
				sw.Close();
			}
			catch (Exception e)
			{
				Log.e(e,Log.Tag.Default,e);
			}
		}
	}

}