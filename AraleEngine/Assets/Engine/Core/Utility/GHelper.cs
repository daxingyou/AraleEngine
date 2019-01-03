#define MOBILE_MOVIE_TEXTURE
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;

namespace Arale.Engine
{
        
    public class GHelper{
    	static long utcTicks = new System.DateTime (1970, 1, 1, 0, 0, 0).Ticks;
    	static string[] color = new string[]{"[ffffff]","[ffffff]","[32e9ff]","[feeb4d]","[fc7a12]","[f3586e]"};
    	static Transform GetTransform(Transform t,string name)
    	{
    		Transform tt = t.FindChild (name);
    		if (null != tt)return tt;
    		for (int i=0; i<t.childCount; ++i) 
    		{
    			tt  = GetTransform(t.GetChild(i),name);
    			if(tt!=null)return tt;
    		}
    		return null;
    	}

    	public static GameObject GetSubGameObjectByPath(GameObject go, string path)
    	{
    		if(path[0] == '/')
    		{
    			string[] name = path.Split('/');
    			if(name[1].Length<1)return go;
    			Transform t = go.transform;
    			for(int i=1; i<name.Length; i++)
    			{
    				t = t.FindChild(name[i]);
                    if (null == t)
                    {
    					Log.e("在 " + go.name + " 中找不到以下节点: " + path);
                        return null;
                    }
    			}
    			return t.gameObject;
    		}
    		else
    		{
                Transform trans = GetTransform(go.transform, path);
                if (null == trans)
                {
    				Log.e("在 " + go.name + " 中找不到以下节点: " + path);
                    return null;
                }
                return trans.gameObject;
    		}
    	}

    	public static GameObject[] GetGameObjectByName(string[] name)
    	{
    		List<GameObject> ls = new List<GameObject> ();
    		GameObject go;
    		for(int i=0,max=name.Length; i<max; ++i)
    		{
    			go = GameObject.Find(name[i]);
    			if(go!=null)ls.Add(go);
    		}
    		return ls.Count>0?ls.ToArray ():null;
    	}

    	//将字典id转为字符串/
    	public static string id2Str(int dictId)
    	{
    		return null;
    	}

    	public static string ToString<T>(T[] array)
    	{
    		string s = "";
    		if (array == null)return s;
    		int len = array.Length;
    		if (len < 1)return s;
    		for (int i = 1; i < len; ++i)s += ","+array [i].ToString ();
    		s = array [0]+s;
    		return s;
    	}

    	//从"1.0,1.0,1.0"格式化为float[]/
    	public static float[] toFloatArray(string s)
    	{
    		string[] ss = s.Split(',');
    		float[] f = new float[ss.Length];
    		for(int i=0,max = ss.Length; i<max; ++i)
    		{
    			f[i] = float.Parse(ss[i]);
    		}
    		return f;
    	}

    	//从"1,1,1"格式化为int[]/
    	public static int[] toIntArray(string s)
    	{
    		string[] ss = s.Split(',');
    		int[] f = new int[ss.Length];
    		for(int i=0,max = ss.Length; i<max; ++i)
    		{
    			f[i] = int.Parse(ss[i]);
    		}
    		return f;
    	}

        public static int[] toIntArray(string s, int len)
        {
            string[] ss = s.Split(',');
            if (len > ss.Length)
                len = ss.Length;

            if (0 == len)
                return null;

            int[] f = new int[len];
            for (int i = 0; i < len; ++i)
            {
                f[i] = int.Parse(ss[i]);
            }
            return f;
        }

        //从"1,1,1"格式化为bool[]/
        public static bool[] toBoolArray(string s)
        {
            string[] ss = s.Split(',');
            bool[] f = new bool[ss.Length];
            for (int i = 0, max = ss.Length; i < max; ++i)
            {
                f[i] = bool.Parse(ss[i]);
            }
            return f;
        }

    	public static T[] reverse<T>(T[] sz)
    	{
    		for(int i=0,max=sz.Length;i<max/2;++i)
    		{
    			T t = sz[i];
    			sz[i] = sz[max-i-1];
    			sz[max-i-1]=t;
    		}
    		return sz;
    	}

    	//将ScrollView重的item居中显示,idx为item的索引
    	void ScrollToIdx(Transform contentRoot, int idx)//idx begin at 0
    	{
    		int count = contentRoot.childCount;
    		if (count < 1||idx>=count)return;
    		GridLayoutGroup glg = contentRoot.GetComponent<GridLayoutGroup> ();

    		Mask mask = contentRoot.GetComponentInParent<Mask> ();
    		Vector3[] corner = new Vector3[4];
    		mask.rectTransform.GetLocalCorners (corner);
    		Debug.LogError (corner [0].ToString ());
    		float vcenter = (corner [3].x - corner [0].x) / 2;
    		float idxPos = glg.padding.left + (idx + 0.5f) * glg.cellSize.x + idx * glg.spacing.x;
    		float left = 0;
    		float right = glg.padding.left + glg.padding.right + count * glg.cellSize.x + (count-1) * glg.spacing.x;
    		float targetPos = 0;
    		if (idxPos - left <= vcenter)
    			return;
    		else if (right - idxPos <= vcenter)
    			targetPos = right - vcenter;
    		else
    			targetPos = idxPos;
    		Vector3 target = contentRoot.localPosition;
    		target.x = -targetPos;
    		contentRoot.localPosition = target;
    	}


    	//销毁所有子节点
    	public static void DestroyChilds(Transform root)
    	{
    		for(int i=0,max=root.childCount;i<max;++i)
    		{
    			GameObject.Destroy(root.GetChild(i).gameObject);
    		}
    	}

    	public static void SetLayer(Transform root, LayerMask lm)
    	{
    		root.gameObject.layer = lm;
    		for (int i=0; i<root.childCount; ++i) 
    		{
    			SetLayer(root.GetChild(i), lm);
    		}
    	}

    	public static long RemoteTick2Local(long tick)
    	{
    		return tick * 10000 + utcTicks;
    	}

    	public static long LocalTick2Remote(long tick)
    	{
    		return (tick - utcTicks) / 10000;
    	}

    	public static long Tick2minute(long tick)
    	{
    		return tick/600000000;
    	}

    	public static IEnumerator WaitForRealSeconds(float time)
    	{
    		float start = Time.realtimeSinceStartup;
    		while (Time.realtimeSinceStartup < start + time)
    		{
    			yield return null;
    		}
    	}

    	public static void PlayAnim(GameObject go, string clip, bool unscaleTime=false, AnimationEx.OnComplite onComplite=null)
    	{
    		AnimationEx ae = go.AddComponent<AnimationEx>();
    		ae.Play(clip,onComplite,unscaleTime);
    	}

    	public static string GetLoadPathFromAssetObject(Object asset)
    	{
    #if UNITY_EDITOR
    		string path = UnityEditor.AssetDatabase.GetAssetPath(asset);
    		int idx = path.LastIndexOf("Resources/");
    		path = path.Substring(idx+10);
    		int i = path.LastIndexOf(".");
    		return path.Remove(i);
    #else
            return null;
    #endif
        }

        public static byte[] Object2Bytes(object obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                return ms.GetBuffer();
            }
        }

        public static object Bytes2Object(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                IFormatter formatter = new BinaryFormatter();
                return formatter.Deserialize(ms);
            }
        }

        #region 解析屏蔽字使用
        /**通过源字符串以及敏感字符串 替换源字符串中敏感字符串，并保留其余全部字符信息
        *
        * @param[string] src	     源字符串
        * @param[string] filterStr	 敏感字符串
        * @return string: 
        *
        * @note 将src中的filterStr转换为"**",保留src中filterStr以外的所有字符信息
        *
        * 作者 日期 问题单号 修订说明
        *
        * shiyuanbo 2015-12-18
        *
        */

        public static string GetRemainStr(string src, string filterStr)
        {
            string trimStr = src.Replace(" ", "").ToUpper();
            filterStr = filterStr.ToUpper();
            int filterIdx = trimStr.IndexOf(filterStr);
            string value = GetSrc(src, trimStr, filterStr, filterIdx) + "**" + GetSrc(src, trimStr, filterStr, filterIdx, false);

            return value;
        }

        /**通过源字符串以及敏感字符串，得到源字符串中位于敏感字符串左边(右边)的子字符串
        *
        * @param[string] src	         源字符串
        * @param[string] trimStr	     源字符串中踢出空白字符后的字符串(字母全部为大写)
        * @param[string] filterStr	     敏感字符串(字母全部为大写)
        * @param[int]    filterIdx       敏感字符串在filterStr在trimStr的初始位置索引
        * @param[bool]   isHead          true:表示查找源字符串src中位于敏感字符串filterStr左边的子字符串；false:表示查找源字符串src中位于敏感字符串filterStr右边的子字符串
        * @return string: 
        *
        * @note 将src中的filterStr转换为"**",保留src中filterStr以外的所有字符信息
        *
        * 作者 日期 问题单号 修订说明
        *
        * shiyuanbo 2015-12-18
        *
        */
        static string GetSrc(string src, string trimStr, string filterStr, int filterIdx, bool isHead = true)
        {
            int find_count = 0;
            int pos = -1;

            if (isHead && filterIdx == 0)
                return "";

            if (!isHead && filterIdx + filterStr.Length >= trimStr.Length)
                return "";
            int count = 0;
            int deadLine = isHead ? filterIdx : filterIdx + filterStr.Length + 1;
            char target = isHead ? trimStr[filterIdx - 1] : trimStr[filterIdx + filterStr.Length];
            for (int i = 0; i < deadLine; ++i)
            {
                if (trimStr[i] == target)
                {
                    count++;
                }
            }

            string tmp = src.ToUpper();
            while (find_count < count)
            {
                pos = tmp.IndexOf(target, pos+1);
                if (-1 != pos)
                {
                    find_count++;
                }
            }

            if (isHead)
            {
                while (pos + 1 < src.Length && src[pos + 1] == ' ')
                {
                    ++pos;
                }
            }

            if (isHead)
            {
                string value = src.Substring(0, pos+1);
                return value;
            }
            else
            {
                string value = src.Substring(pos);
                return value;
            }
        }

        #endregion
    }

}