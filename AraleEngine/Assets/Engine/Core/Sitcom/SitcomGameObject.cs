using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using DG.Tweening;

namespace Arale.Engine
{
    
public class SitcomGameObject : SitcomSystem.Action
{
    public void OnEvent (TimeMgr.Action a)
	{
		Log.i ("gameobject act="+act+","+id, Log.Tag.Sitcom);
		switch(act)
		{
		case "create":
			Create();
			break;
		case "destory":
			Destory();
			break;
		case "show":
			Show();
			break;
		case "hide":
			Hide();
			break;
		case "move":
			Move();
			break;
		case "transform":
			Transform();
			break;
		}
	}

	void Create()
	{
		JObject data = JsonConvert.DeserializeObject (param) as JObject;
		string res = data["path"].ToString();
		GameObject go = ResLoad.get (res).gameObject();
		OnLoadFinish (go, data);
	}

	void OnLoadFinish(GameObject go, object param1=null, object param2=null, object param3=null)
	{
		JObject data = param1 as JObject;
		go.tag="Sitcom";
		go.name = actor;
		go.transform.parent = SitcomSystem.single.mount;
		float[] val = GHelper.toFloatArray(data ["position"].ToString ());
		go.transform.position = new Vector3(val[0],val[1], val[2]);
		val = GHelper.toFloatArray(data ["rotation"].ToString ());
		go.transform.rotation = Quaternion.Euler(val[0],val[1], val[2]);
		if(null!=data.Property("scale"))
		{
			val = GHelper.toFloatArray(data ["scale"].ToString ());
			go.transform.localScale = new Vector3(val[0],val[1], val[2]);
		}
		else
		{
			go.transform.localScale = Vector3.one;
		}
		RunNextAction ();
	}

	void Destory()
	{
		GameObject go = GameObject.Find (actor);
		GameObject.Destroy (go);
		RunNextAction ();
	}

	void Show()
	{
		GameObject go = GameObject.Find (actor);
		go.SetActive (true);
		RunNextAction ();
	}

	void Hide()
	{
		GameObject go = GameObject.Find (actor);
		go.SetActive (false);
		RunNextAction ();
	}

	void Move()
	{
		GameObject go = GameObject.Find (actor);
		JObject data = JsonConvert.DeserializeObject (param) as JObject;
		string path = data ["path"].ToString ();
		float duration = 0;
		if(null!=data.Property("duration"))
		{
			duration = data ["duration"].ToObject<float>();
		}
		go.transform.DOPath(iTweenPath.GetPath(path), duration).OnComplete(delegate() {RunNextAction();});
	}

	public void OnMovePathComplete(object param)
	{
		RunNextAction ();
	}

	void Transform()
	{
		JObject data = JsonConvert.DeserializeObject (param) as JObject;
		GameObject go = GameObject.Find (actor);
		Vector3 pos=go.transform.position;
		Quaternion rotation=go.transform.rotation;
		Vector3 scale=go.transform.localScale;
		float time = 0;
		if(null!=data.Property("positon"))
		{
			float[] val = GHelper.toFloatArray(data ["positon"].ToString ());
			pos = new Vector3(val[0],val[1], val[2]);
		}
		if(null!=data.Property("rotation"))
		{
			float[] val = GHelper.toFloatArray(data ["rotation"].ToString ());
			rotation = Quaternion.Euler(val[0],val[1], val[2]);
		}
		if(null!=data.Property("scale"))
		{
			float[] val = GHelper.toFloatArray(data ["scale"].ToString ());
			scale = new Vector3(val[0],val[1], val[2]);
		}
		if(null!=data.Property("duration"))
		{
			time = data ["duration"].ToObject<float>();
		}
		GRoot.single.StartCoroutine (TransformAnimation(go, pos, rotation, scale, time));  
	}

	IEnumerator TransformAnimation(GameObject go, Vector3 pos, Quaternion rotation, Vector3 scale, float time)
	{
		Transform trans = go.transform;
		Vector3 cPos = trans.position;
		Quaternion cRot = trans.rotation;
		Vector3 cSca = trans.localScale;
		float t=0;
		float k;
		WaitForSeconds w = new WaitForSeconds (0.02f);//25fps
		while(t<time)
		{
			k = t/time;
			trans.position   = Vector3.Lerp(cPos,pos,k);
			trans.rotation   = Quaternion.Lerp(cRot,rotation,k);
			trans.localScale = Vector3.Lerp(cSca,scale,k);
			t+=Time.deltaTime;
			yield return w;
		}
		trans.position = pos;
		trans.rotation = rotation;
		trans.localScale = scale;
		RunNextAction ();
	}
}

}