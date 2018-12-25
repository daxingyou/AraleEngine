using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace Arale.Engine
{

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
    	public static AnimationCurve get(string assetName, string acName)
    	{
    		AnimationCurve ac=null;
            string key = assetName + acName;
    		if (mCach.TryGetValue (key, out ac))
    		{
    			return ac;
    		}

            GameObject go = ResLoad.get ("AC/"+assetName).gameObject ();
    		AC acMono = go.GetComponent<AC> ();
    		for (int i = 0; i < acMono._acs.Count; ++i)
    		{
    			ACItem aci = acMono._acs[i];
                key = assetName + aci.name;
    			mCach [key] = aci.ac;
    		}
    		ac = mCach [key];
    		DestroyObject (go);
    		return ac;
    	}

    	public static void clear()
    	{
    		mCach.Clear ();
    	}
    }

}
