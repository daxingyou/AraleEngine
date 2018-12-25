using UnityEngine;
using System.Collections;
using UnityEngine.UI;

//绑小红点节点上,填入关联key值
namespace Arale.Engine
{

    public class RedPoint : MonoBehaviour {
    	public string _key="";
    	public Text   _count;
        bool _multiKey;
    	// Use this for initialization
    	void Awake () {
            _multiKey = _key.StartsWith("|");
            int count = _multiKey?GetMultiKeyCount():RedPointMgr.single.Get (_key);
    		gameObject.SetActive (count>0);
    		if (_count != null)_count.text = count.ToString ();
            RedPointMgr.single.onDataChanged += OnDataChange;
    	}

    	void OnDestroy()
    	{
            RedPointMgr.single.onDataChanged -= OnDataChange;
    	}

    	public void OnDataChange(int mask, object val)
    	{
            string key = val as string;
            if (_multiKey)
            {
                if (!_key.Contains(key))
                    return;
            }
            else
            {
                if (_key != key)
                    return;
            }

            int count = _multiKey?GetMultiKeyCount():RedPointMgr.single.Get (_key);
    		gameObject.SetActive (count>0);
    		if (_count != null)_count.text = count.ToString ();
    	}

        int GetMultiKeyCount()
        {
            int count = 0;
            string[] keys = _key.Split(new char[]{'|'}, System.StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < keys.Length; ++i)
            {
                count += RedPointMgr.single.Get (keys[i]);
            }
            return count;
        }
    }

}
