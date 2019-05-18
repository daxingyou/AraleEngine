using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Scripts.BehaviourScripts;
using Arale.Engine;

public class ResMgr : MonoBehaviour
{
    public static ResMgr Single;
	Dictionary<string, Shader> _shaders = new Dictionary<string, Shader>();
    void Awake()
    {
        Single = this;
        ResLoad.init(this);
        LoadCommonAB();
    }

    void LoadCommonAB()
    {
        ResLoad.get("common/font", ResideType.InGame).assetBundle();
        AssetBundle ab = ResLoad.get("common/shader", ResideType.InGame).assetBundle();
		if (ab != null) 
		{
			Object[] objs = ab.LoadAllAssets ();
			for (int i = 0, max = objs.Length; i < max; ++i) 
			{
				Shader sd = objs [i] as Shader;
				_shaders [sd.name] = sd;
			}
		}
    }

    public void Reset()
    {
        Log.i("ResMgr Reset!!!", Log.Tag.RES);
		_shaders.Clear ();
        ResLoad.clearCach();
        ResLoad.init(this);
        LoadCommonAB();
		LuaRoot.dirty = true;
    }

	public Shader FindShader(string name)
	{
		Shader sd = Shader.Find(name);
		if (sd != null)return sd;
		if (_shaders.TryGetValue (name, out sd))return sd;
		return null;
	}
}