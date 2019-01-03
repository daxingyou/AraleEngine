using UnityEngine;
using System.Collections;
using DG.Tweening;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Arale.Engine
{
    
public class SitcomCamera : SitcomSystem.Action
{
	GameObject[] mCam;
    public void onEvent (TimeMgr.Action a)
	{
		Log.i ("camera act="+act+","+id, Log.Tag.Sitcom);
		if(actor!=null)mCam = GHelper.GetGameObjectByName(actor.Split(','));
		switch (act)
		{
		case "shake":
			Shake ();
			break;
		case "blur":
			Blur ();
			break;
		case "radiaBlur":
			RadiaBlur ();
			break;
		case "move":
			Move ();
			break;
		default:
			Log.e("SitcomCaram not support act=" + act);
			break;
		}
	}

	void Shake()
	{
		JObject data = JsonConvert.DeserializeObject (param) as JObject;
		float duration = data["duration"].ToObject<float>();
		float strength = data["strength"].ToObject<float>();
		float vibrate  = data["vibrate"].ToObject<int>();
		mCam[0].transform.DOShakePosition(1.0f,1.0f,10).OnComplete (delegate(){RunNextAction();});
	}

	void Blur()
	{
	}

	void RadiaBlur()
	{
	}

	void Move()
	{
	}
}

}
