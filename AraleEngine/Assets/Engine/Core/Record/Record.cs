using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Arale.Engine
{
	public class Record
	{
		string mFileCryptPassword;
		bool dirty;
		
		public Record(string cryptPassword)
		{
			mFileCryptPassword = cryptPassword;
		}
		
		public void Load(string identifier)
		{
			m_fullPath = UnityEngine.Application.persistentDataPath + "/" + StringUtils.ToMD5(identifier);
			Log.i ("读取信息:" + identifier + "        路径:" + m_fullPath);
			if (!File.Exists(m_fullPath))
			{
				return;
			}
			
			// Check file's MD5
			if (!FileUtil.GetFileMD5(m_fullPath).Equals(UnityEngine.PlayerPrefs.GetString(m_fullPath)))
			{
				File.Delete(m_fullPath);
				return;
			}
			
			//Read bytes to buffer
			FileStream fs = new FileStream(m_fullPath, FileMode.Open, FileAccess.Read);
			byte[] buffers = new byte[fs.Length];
			fs.Read(buffers, 0, buffers.Length);
			fs.Close();
			
			//Uncrypt and parse
			if(!string.IsNullOrEmpty(mFileCryptPassword))Crypt.UnCrypt(buffers, mFileCryptPassword);
			string content = Encoding.UTF8.GetString(buffers);
			m_dic = StringUtils.ReadDictionary(content);
			dirty = false;
		}
		
		public void Save()
		{
			if(!dirty)return;
			if (string.IsNullOrEmpty(m_fullPath))
			{
				Log.w("Save record failed as the path is empty.");
				return;
			}
			
			if (0 == m_dic.Count)
			{
				return;
			}
			
			if (File.Exists(m_fullPath))
			{
				File.Delete(m_fullPath);
			}
			
			string content = StringUtils.ParseDictionary(m_dic);
			byte[] buffers = Encoding.UTF8.GetBytes(content);
			if(!string.IsNullOrEmpty(mFileCryptPassword))Crypt.DoCrypt(buffers, mFileCryptPassword);
			FileStream fs = new FileStream(m_fullPath, FileMode.CreateNew);
			fs.Write(buffers, 0, buffers.Length);
			fs.Flush();
			fs.Close();

			//Save File's MD5
			UnityEngine.PlayerPrefs.SetString(m_fullPath, FileUtil.GetFileMD5(m_fullPath));
			UnityEngine.PlayerPrefs.Save();
			dirty = false;
            Log.i("Record saved in " + m_fullPath);
		}

        public void Reset(string identifier)
        {
            m_fullPath = UnityEngine.Application.persistentDataPath + "/" + StringUtils.ToMD5(identifier);
            Log.i("重置信息:" + identifier + "        路径:" + m_fullPath);
            if (File.Exists(m_fullPath))
            {
                File.Delete(m_fullPath);
                Save();
            }
        }
		
		public void DeleteAll()
		{
			m_dic.Clear();
			dirty = true;
		}
		
		public void DeleteKey(string key)
		{
			m_dic.Remove(key);
			dirty = true;
		}
		
		public bool HasKey(string key)
		{
			return m_dic.ContainsKey(key);
		}
		
		public float GetFloat(string key, float defaultValue = 0.0f)
		{
			if (m_dic.ContainsKey(key))
			{
				try
				{
					return float.Parse(m_dic[key]);
				}
				catch (Exception e)
				{
                   	Log.e(e,Log.Tag.Default,e);
					return defaultValue;
				}
			}
			
			return defaultValue;
		}
		
		public int GetInt(string key, int defaultValue = 0)
		{
			if (m_dic.ContainsKey(key))
			{
				try
				{
					return int.Parse(m_dic[key]);
				}
				catch (Exception e)
				{
					Log.e(e,Log.Tag.Default,e);
					return defaultValue;
				}
			}
			
			return defaultValue;
		}
		
		public bool GetBoolean(string key, bool defaultValue = false)
		{
			if (m_dic.ContainsKey(key))
			{
				try
				{
					return bool.Parse(m_dic[key]);
				}
				catch (Exception e)
				{
					Log.e(e,Log.Tag.Default,e);
					return defaultValue;
				}
			}
			
			return defaultValue;
		}
		
		public string GetString(string key, string defaultValue = "")
		{
			if (m_dic.ContainsKey(key))
			{
				return m_dic[key];
			}
			
			return defaultValue;
		}
		
		public void SetFloat(string key, float v)
		{
			SetString(key, v.ToString());
		}
		
		public void SetInt(string key, int v)
		{
			SetString(key, v.ToString());
		}
		
		public void SetBoolean(string key, bool v)
		{
			SetString(key, v.ToString());
		}
		
		public void SetString(string key, string v)
		{
			if (m_dic.ContainsKey(key))
			{
				m_dic[key] = v;
			}
			else
			{
				m_dic.Add(key, v);
			}
			dirty = true;
		}
		
		string m_fullPath;
		Dictionary<string, string> m_dic = new Dictionary<string, string>();
	}
}
