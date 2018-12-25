using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Arale.Engine
{
	public class StringUtils
	{
		private const string INVALID_CHAR_SET = ",<.>/?;:'\"[{]}\\|`~!@#$%^&*()-=+ \r\n\t";
		public static string ToSBC(string text)
		{
			char[] c = text.ToCharArray();
			for (int i=0; i<c.Length; i++)
			{
				if (INVALID_CHAR_SET.IndexOf(c[i]) > -1)
				{
					if (32 == c[i])
					{
						c[i] = (char)12288;
					}
					else if (c[i] < 127)
					{
						c[i] = (char)(c[i] + 65248);
					}
				}
			}
			
			return new string(c);
		}
		
		public static bool ContainInvalidChar(string text)
		{
			char[] c = text.ToCharArray();
			for (int i=0; i<c.Length; i++)
			{
				if (INVALID_CHAR_SET.IndexOf(c[i]) > -1)
				{
					return true;
				}
			}
			
			return false;
		}
		
		public static int LengthOfUTF8(string str)
		{
			int length = 0;
			char[] characters = str.ToCharArray();
			foreach (char c in characters)
			{
				int cInt = (int)c;
				if (cInt < 256)
				{
					length++;
				}
				else
				{
					length += 2;
				}
			}
			return length;
		}
		
		public static string ToMD5(string str)
		{
			if(string.IsNullOrEmpty(str))
			{
				return null;
			}
			
			try
			{
				MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
				byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
				return System.BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
			}
			catch (System.Exception e)
			{
				Log.e(e,Log.Tag.Default,e);
				return null;
			}
		}
		
		public static Dictionary<string, string> ReadDictionary(string str)
		{
			Dictionary<string, string> dic = new Dictionary<string, string>();
			string[] lines = str.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
			for (int i=0; i<lines.Length; i++)
			{
				string line = lines[i].Trim();
				if (string.IsNullOrEmpty(line) || line.StartsWith("#") || line.StartsWith("//"))
				{
					continue;
				}

				string key = null;
				int idx = line.IndexOf('=');
				if (idx>=0)
				{
					try
					{
						key = line.Substring(0,idx).Trim();
						dic.Add(key, line.Substring(idx+1));
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
			
			return dic;
		}
		
		public static string ParseDictionary(Dictionary<string, string> dic)
		{
			string str = "";
			foreach (string key in dic.Keys)
			{
				str += key + "=" + dic[key] + "\n";
			}
			return str;
		}
	}
}