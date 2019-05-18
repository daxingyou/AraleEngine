using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Scripts.CoreScripts.Core;
using Scripts.CoreScripts.GameLogic.Battle.BattleObject;
using Scripts.BehaviourScripts;

public class ResMgr : MonoBehaviour
{
    public static ResMgr Single;
	Dictionary<string, Shader> _shaders = new Dictionary<string, Shader>();
    void Awake()
    {
        Single = this;
        ResLoad.Init(this);
        LoadCommonAB();
    }

    void LoadCommonAB()
    {
        ResLoad.Get("common/font", ResideType.InGame).GetAssetBundle();
        AssetBundle ab = ResLoad.Get("common/shader", ResideType.InGame).GetAssetBundle();
		if (ab != null) 
		{
			Object[] objs = ab.LoadAllAssets ();
			for (int i = 0, max = objs.Length; i < max; ++i) 
			{
				Shader sd = objs [i] as Shader;
				_shaders [sd.name] = sd;
			}
		}
		IObj.RimHighlightShader = FindShader ("Shader/RimHighLight");
    }

    public void Reset()
    {
        Log.I("ResMgr Reset!!!", Log.Tag.RES);
		_shaders.Clear ();
        ResLoad.ClearCach();
        ResLoad.Init(this);
        LoadCommonAB();
		LuaRoot.dirty = true;
		GameDataCfg.Instance._dirty = true;
    }

	public Shader FindShader(string name)
	{
		Shader sd = Shader.Find(name);
		if (sd != null)return sd;
		if (_shaders.TryGetValue (name, out sd))
			return sd;
		return null;
	}
}