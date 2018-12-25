using UnityEngine;
using System.Collections;
using System.Xml;
using System.Collections.Generic;
using System;


/*情景系统*/
namespace Arale.Engine
{
	public class SitcomSystem
	{
		static SitcomSystem mThis;
		public static SitcomSystem single{
			get{
				if(null!=mThis)return mThis;
				return mThis=new SitcomSystem();
			}
		}

		//------------------
		int mSitcomId;
		XmlDocument xmlDoc;
		GameObject sitcom;
		public Transform mount{get{return sitcom.transform;}}
		public delegate void OnSitcomComplete ();
		OnSitcomComplete onSitcomComplete;
		Dictionary<int,Action> actionMap = new Dictionary<int,Action>();
		TimeMgr.TimeAxis mSitcomAxis;
		//------------------

		SitcomSystem()
		{
			sitcom = new GameObject("Sitcom");
		}

		public bool Play(string file, OnSitcomComplete onComplete)
		{
			if (isPlaying)return false;
			mSitcomId = -1;
			onSitcomComplete = onComplete;
			mSitcomAxis = TimeMgr.single.GetTimeAxis ("Sitcom", true);
			mSitcomAxis.SetParent (TimeMgr.single.realTimeAxis, 0);
			RunScript (file);
			return true;
		}

		public void Stop()
		{//给一个不存在的NEXTID就可以结束剧情/
			if(mSitcomId==0)return;
			ClearSitcom ();
			mSitcomId = 0;
			xmlDoc = null;
			actionMap.Clear ();
			mParamMap.Clear ();
			mSitcomAxis.Remove ();
			if(null!=onSitcomComplete)onSitcomComplete();
			onSitcomComplete = null;
		}

		public bool isPlaying
		{
			get{return mSitcomId != 0;}
		}

		void ClearSitcom()
		{//清除情景中动态创建的对象/
			GameObject[] go = GameObject.FindGameObjectsWithTag ("Sitcom");
			for(int i=0,max=go.Length;i<max;++i)
			{
				GameObject.Destroy(go[i]);
			}
		}

		public void RunAction(int id)
		{
			Action a;
			if (!actionMap.TryGetValue (id, out a))
			{
				Log.e("not find action=" + id);
				Stop ();
				return;
			}
			RunAction(a);
		}

		void RunAction(Action a)
		{
			Log.i ("push " + a.id, Log.Tag.Sitcom);
			float t = mSitcomAxis.time;
			a.time = t + a.delay;
			mSitcomAxis.AddAction (a);
		}

		#region 剧情Action
		public abstract class Action : TimeMgr.Action
		{
			public float   time
			{
                set{doTime = value;}
			}
			public XmlNode node;
			public int     id;
			public float   delay;
			public string  next;
			public string  act;
			public string  actor;
			public string  param;
			public string  waitEvent;
			public string  sendEvetn;
			void OnWaitEvent(EventMgr.EventData eb)
			{
                EventMgr.single.UnAddListener ("Sitcom."+waitEvent, OnWaitEvent);
				waitEvent = null;
				RunNextAction ();
			}

			protected void RunNextAction()
			{
				if (waitEvent!=null)
				{
                    EventMgr.single.AddListener ("Sitcom."+waitEvent, OnWaitEvent);
					return;//等待事件
				}

				if (null == next)return;
				string[] strId = next.Split(',');
				for(int i=0,max=strId.Length;i<max;++i)
				{
					int id = int.Parse (strId [i]);
					SitcomSystem.single.RunAction (id);
				}
			}
		}
		#endregion


		#region 全局配置参数
		Dictionary<string,object> mParamMap = new Dictionary<string,object>();
		public object GetParam(string key)
		{
			if (!mParamMap.ContainsKey (key))
				return null;
			return mParamMap [key];
		}

		public void SetParam(string key, object val)
		{
			mParamMap [key] = val;
		}
		#endregion


		#region sitcom脚本解析
		void RunScript(string file)
		{
			Log.i("runScript path=" + file, Log.Tag.Sitcom);
			if(string.IsNullOrEmpty(file))
			{
				Log.e("sitcom table error sitcomid=" + mSitcomId, Log.Tag.Sitcom);
				return;
			}

			using (ResLoad ld = ResLoad.get (file))
			{
				RunScript (ld.asset<TextAsset> ());
			}
		}

		void RunScript(TextAsset asset)
		{
			xmlDoc = new XmlDocument();
			xmlDoc.LoadXml (asset.text);

			XmlNodeList play = xmlDoc.SelectNodes ("/SITCOM/ACTION");
			foreach (XmlNode n in play)
			{
				string name = n.Attributes["TYPE"].Value;
				Action action = CreateAction(name);
				if(null==action)continue;
				
				action.node = n;
				XmlNode an=n.Attributes["ID"];
				if(null!=an)
				{
					action.id = int.Parse(an.Value);
					actionMap[action.id]=action;
				}
				an=n.Attributes["ACT"];
				action.act = an.Value;

				an=n.Attributes["ACTOR"];
				if(null!=an)
				{
					action.actor = an.Value;
				}

				an=n.Attributes["TIME"];
				if(null!=an)
				{
					action.delay = float.Parse(an.Value);
					RunAction (action);
				}
				an=n.Attributes["DELAY"];
				if(null!=an)
				{
					action.delay = float.Parse(an.Value);
				}
				an=n.Attributes["NEXTID"];
				if(null!=an)
				{
					action.next= an.Value;
				}
				an=n.Attributes["WAIT"];
				if(null!=an)
				{
					action.waitEvent = an.Value;
				}
				an=n.Attributes["SEND"];
				if(null!=an)
				{
					action.sendEvetn = an.Value;
				}
				an=n.Attributes["PARAM"];
				if(null!=an)
				{
					action.param = an.Value;
				}
			}
		}

		Action CreateAction(string name)
		{
			switch(name)
			{
			case "gameobject":
				return new SitcomGameObject();
			case "camera":
				return new SitcomCamera();
			default:
				Log.e("sitcom not support action type=" + name);
				return null;
			}
		}
		#endregion
	}
}
