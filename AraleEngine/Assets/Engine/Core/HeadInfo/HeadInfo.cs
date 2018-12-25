using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;


namespace Arale.Engine
{

    public class HeadInfo : MonoBehaviour
    {
        #region HeadInfo管理
        public delegate void OnHeadInfoInit(HeadInfo hi);
        static List<HeadInfo> mHeadInfos;
        static Transform mMount;
        public static Transform root{get{return mMount;}}
        public static void Create(Camera cam, bool enableEvent=false)
        {
            mHeadInfos = new List<HeadInfo>();
            GameObject go = new GameObject("HeadInfo");
            go.transform.SetParent(cam.transform,false);
            Canvas canvas = go.AddComponent<Canvas>();
            canvas.worldCamera = cam;
            if (enableEvent)
            {
                go.AddComponent<GraphicRaycaster>();
            }
            //必须放在添加Canvas之后，因为Add之后原Transform被销毁变成了RectTransform,导致mMount为空
            mMount = go.transform;
        }

        public static void Destroy()
        {
            mHeadInfos = null;
            GameObject.Destroy(mMount.gameObject);
            mMount = null;
        }

        public static HeadInfo Bind(Transform trans, object data)
        {
            HeadInfo hi = null;
            if (mHeadInfos.Count > 0 && mHeadInfos[0].target == null)
            {
                hi = mHeadInfos[0];
                hi.gameObject.SetActive(true);
            }
            else
            {
                GameObject go = ResLoad.get("UI/Main/HeadInfo", ResideType.InScene).gameObject();
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
            mHeadInfos.Remove(hi);
            mHeadInfos.Insert(0, hi);
        }
        #endregion


        public float mYOffset = 1;
        Transform mTarget;
        public Transform target
        {
            set
            {
                mTarget = value;
                Update();
            }
            get{ return mTarget; }
        }

        public void Unbind()
        {
            if (mTarget == null)return;
            mTarget = null;
            Unbind();
            Recycle(this);
        }

    	// Update is called once per frame
    	void Update ()
        {
            if (mTarget == null)return;
            Vector3 v = mTarget.position;
            v.y += mYOffset;
            transform.position = v; 
    	}
            
        protected virtual void Init(object data){}
        protected virtual void Deinit(){}
    }

}
