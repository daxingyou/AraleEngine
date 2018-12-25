using UnityEngine;
using System.Collections;

namespace Arale.Engine
{
    public class IMgr
    {
        public virtual void Init(){}
        public virtual void Deinit(){}
        public virtual void Update(){}
    }
    
    public class MgrBase<T> : IMgr where T:IMgr,new()
    {
        static T mThis;
        public static T single
        {
            get
            {
                if (null != mThis)return mThis;
                mThis = new T();
                GRoot.single.AddUpdate(mThis.Update);
                mThis.Init();
                return mThis;
            }
        }
    }
}

