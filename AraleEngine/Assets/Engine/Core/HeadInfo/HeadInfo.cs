using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;


namespace Arale.Engine
{

	public class HeadInfo : LuaMono
    {
        #region HeadInfo管理
        static List<HeadInfo> mHeadInfos;
        static Transform mMount;
        public static Transform root{get{return mMount;}}
        public static void Create(Camera cam, bool enableEvent=false)
        {
            mHeadInfos = new List<HeadInfo>();
            GameObject go = new GameObject("HeadInfo");
            Canvas canvas = go.AddComponent<Canvas>();
            canvas.worldCamera = cam;
			canvas.renderMode = RenderMode.ScreenSpaceCamera;
			canvas.planeDistance = 10;
            if (enableEvent)
            {
                go.AddComponent<GraphicRaycaster>();
            }
            //必须放在添加Canvas之后，因为Add之后原Transform被销毁变成了RectTransform,导致mMount为空
            mMount = go.transform;
            GameObject.DontDestroyOnLoad(go);
        }

        public static void Destroy()
        {
            mHeadInfos = null;
			if (mMount == null)return;
            GameObject.Destroy(mMount.gameObject);
            mMount = null;
        }

        public static HeadInfo Bind(Transform trans, object data)
		{
			if (mMount == null)return null;//for test
            HeadInfo hi = null;
            if (mHeadInfos.Count > 0 && mHeadInfos[0].target == null)
            {
                hi = mHeadInfos[0];
                hi.gameObject.SetActive(true);
            }
            else
            {
                GameObject go = ResLoad.get("UI/HeadInfo", ResideType.InScene).gameObject();
                go.transform.SetParent(mMount, false);
                hi = go.GetComponent<HeadInfo>();
                mHeadInfos.Add(hi);
            }
			hi.target = trans;
            hi.Init(data);
            return hi;
        }

        static void Recycle(HeadInfo hi)
        {
			hi.gameObject.SetActive (false);
            mHeadInfos.Remove(hi);
            mHeadInfos.Insert(0, hi);
        }
        #endregion


        public float mYOffset = 3;
		RectTransform mRT;
		Transform mTarget;
		public Transform target{get{return mTarget;} set{mTarget = value;Update ();}}

        public void Unbind()
        {
            if (mTarget == null)return;
            mTarget = null;
            Unbind();
			Deinit ();
            Recycle(this);
        }

		protected override void onAwake()
		{
			mRT = transform as RectTransform;
		}

    	void Update ()
        {
            if (mTarget == null)return;
            Vector3 v = mTarget.position;
            v.y += mYOffset;
			mRT.position = v;
    	}
            
        protected virtual void Init(object data)
		{
			sendEvent ((int)UnitEvent.HeadInfoInit, data);
		}
        protected virtual void Deinit()
		{
			sendEvent ((int)UnitEvent.HeadInfoDeinit, null);
		}
    }

}
