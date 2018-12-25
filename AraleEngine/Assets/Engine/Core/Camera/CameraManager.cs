using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Arale.Engine
{
    
    public class CameraMgr : MgrBase<CameraMgr>
    {
    	List<Camera> mCams = new List<Camera> ();
    	public Camera CreateCamera(string name, Vector3 pos, Vector3 lookpos)
    	{
    		GameObject go = new GameObject (name);
    		Camera cam = go.AddComponent<Camera> ();
    		cam.transform.position = pos;
    		cam.transform.LookAt (lookpos);
    		mCams.Add (cam);
    		return cam;
    	}

    	public Camera GetCamera(string name)
    	{
    		return mCams.Find ((Camera cam) => {return cam.name == name;});
    	}

    	public void RemoveCamera(string name)
    	{
    		for (int i = mCams.Count - 1; i >= 0; --i)
    		{
    			if (mCams [i].name == name)mCams.RemoveAt (i);
    		}
    	}

        public void AddCamera(Camera cam)
        {
            if (null == mCams.Find(delegate(Camera c)
                {
                    return object.ReferenceEquals(c, cam);
                }))
                mCams.Add(cam);
        }

        public void RemoveCamera(Camera cam)
        {
            mCams.Remove(cam);
        }
    }

}
