using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RedPoint : MonoBehaviour {
	public string _key="";
	public Text   _count;
    bool _multiKey;
	// Use this for initialization
	void Awake () {
        _multiKey = _key.StartsWith("|");
        int count = _multiKey?getMultiKeyCount():RedPointManager.Single.get (_key);
		gameObject.SetActive (count>0);
		if (_count != null)_count.text = count.ToString ();
		RedPointManager.Single.onDataChanged += OnDataChange;
	}

	void OnDestroy()
	{
		RedPointManager.Single.onDataChanged -= OnDataChange;
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

        int count = _multiKey?getMultiKeyCount():RedPointManager.Single.get (_key);
		gameObject.SetActive (count>0);
		if (_count != null)_count.text = count.ToString ();
	}

    int getMultiKeyCount()
    {
        int count = 0;
        string[] keys = _key.Split(new char[]{'|'}, System.StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < keys.Length; ++i)
        {
            count += RedPointManager.Single.get (keys[i]);
        }
        return count;
    }
}
