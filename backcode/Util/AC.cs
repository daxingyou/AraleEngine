using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Scripts.CoreScripts.Core;

public class AC : MonoBehaviour
{
	[System.Serializable]
	public class ACItem
	{
		public string name;
		public AnimationCurve ac;
	}
	public List<ACItem> _acs;
	//=======
	static Dictionary<string, AnimationCurve> mCach = new Dictionary<string, AnimationCurve>();
	public static AnimationCurve get(string path, string name)
	{
		AnimationCurve ac=null;
		if (mCach.TryGetValue (name, out ac))
		{
			return ac;
		}
		GameObject go = ResLoad.Get (path).GetGameObject ();
		AC acMono = go.GetComponent<AC> ();
		for (int i = 0; i < acMono._acs.Count; ++i)
		{
			ACItem aci = acMono._acs[i];
			if (mCach.ContainsKey (aci.name))continue;
			mCach [name] = aci.ac;
		}
		ac = mCach [name];
		DestroyObject (go);
		return ac;
	}

	public static void clear()
	{
		mCach.Clear ();
	}
}
