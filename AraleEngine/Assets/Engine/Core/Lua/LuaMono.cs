using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using XLua;
using DG.Tweening;

namespace Arale.Engine
{

    public class LuaMono : MonoBehaviour
    {
    	#if UNITY_EDITOR
    	[ContextMenu("AutoBind")]
    	#endif
    	public void AutoBind()
        {
    		List<GameObject> ls = new List<GameObject> ();
    		for (int i = 0; i < transform.childCount; ++i)AutoBind (ls, transform.GetChild (i));
    		mBinds = ls.ToArray ();
    	}
    	void AutoBind(List<GameObject> ls, Transform t)
    	{
    		if (t.name.StartsWith ("lua"))ls.Add (t.gameObject);
            if (t.GetComponent<LuaMono> () != null)return;
    		for (int i = 0; i < t.childCount; ++i)AutoBind (ls, t.GetChild (i));
    	}
        void AutoBind(Transform t)
        {
            if (t.name.StartsWith ("lua"))mLO.mLT[t.name] = t.gameObject;
            if (t.GetComponent<LuaMono> () != null)return;
            for (int i = 0; i < t.childCount; ++i)AutoBind (t.GetChild (i));
        }


    	#region bindlua
        public void bindLua(string luaClassName=null)
        {
            if (luaClassName != null)mLuaClassName = luaClassName;
            mLO = LuaObject.newObject (mLuaClassName, this);
            if (mLO == null)return;
            if (mBinds != null && mBinds.Length > 0)
            {
                for (int i = 0; i < mBinds.Length; ++i)
                {
                    mLO.mLT[mBinds[i].name] = mBinds[i];
                }
            }
            else
            {
                for (int i = 0; i < transform.childCount; ++i)
                {
                    AutoBind(transform.GetChild(i));
                }
            }
    	}

    	public void unbindLua()
    	{
            if (mLO != null)mLO.Dispose ();
            mLO = null;
    	}

    	public GameObject[] mBinds;
    	public string mLuaClassName;
    	[NonSerialized]public LuaObject mLO;
    	[NonSerialized]public object mUserData;
		[NonSerialized]public Action<LuaTable> luaOnAwake;
        [NonSerialized]public Action<LuaTable> luaOnEnable;
        [NonSerialized]public Action<LuaTable> luaOnStart;
        [NonSerialized]public Action<LuaTable> luaOnUpdate;
        [NonSerialized]public Action<LuaTable> luaOnDisable;
        [NonSerialized]public Action<LuaTable> luaOnDestroy;
		[NonSerialized]public Func<LuaTable,int,object,bool> luaOnEvent;
    	#endregion


    	void Awake(){
    		bindLua ();
            if (luaOnAwake != null)luaOnAwake(mLO.mLT);
            onAwake();
    	}

    	void OnEnable(){
            if (luaOnEnable != null)luaOnEnable(mLO.mLT);
            onEnable();
    	}

        void Start () {
            if (luaOnStart != null)luaOnStart(mLO.mLT);
            onStart();
        }

        void Update () {
            if (luaOnUpdate != null)luaOnUpdate(mLO.mLT);
            onUpdate();
        }

    	void OnDisable(){
            if (luaOnDisable != null)luaOnDisable(mLO.mLT);
            onDisable();
    	}

    	void OnDestroy(){
            if (luaOnDestroy != null)luaOnDestroy(mLO.mLT);
            onDestroy();
            unbindLua ();
    		mUserData = null;
    	}

        protected virtual void onAwake(){}
        protected virtual void onStart(){}
        protected virtual void onUpdate(){}
        protected virtual void onDestroy(){}
        protected virtual void onEnable(){}
        protected virtual void onDisable(){}

		protected virtual bool onEvent(int evt, object param, object sender)
		{
			if (luaOnEvent != null)return luaOnEvent (mLO.mLT, evt, param);
			return false;
		}
		public void sendEvent(int evt, object param)
		{
			onEvent (evt,param,this);
		}

		public Coroutine startLuaCoroutine(LuaTable lt)
		{
			return this.StartCoroutine(luaCoroutine(lt));
		}

		IEnumerator luaCoroutine(LuaTable lt)
		{
			LuaFunction f = (LuaFunction)lt[0];
			object[] o = null;
			do
			{
				o = f.Call(lt);
				if(o==null)break;
				f = (LuaFunction)o[0];
				yield return o[1];
			}while(true);
		}
    }

}
