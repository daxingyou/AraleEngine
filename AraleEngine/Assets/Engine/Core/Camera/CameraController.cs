using UnityEngine;
using System.Collections;

namespace Arale.Engine
{

    public class CameraController : MonoBehaviour
    {
    	[System.NonSerialized]public Camera mCam;
    	[System.NonSerialized]public Transform mTrans;
    	[System.NonSerialized]public Transform mTarget;
    	void Awake ()
    	{
    		mCam = GetComponent<Camera>();
            CameraMgr.single.AddCamera(mCam);
    		mTrans = transform;
    	}

    	void OnDestroy()
    	{
    		mTarget = null;
            CameraMgr.single.RemoveCamera(mCam);
    	}

    	void LateUpdate () {
    		if (mTarget == null)return;
    		Vector3 v = mTarget.position;
    		Vector3 cv = mTrans.position;
    		cv.x = v.x;
    		cv.y = v.y;
    		mTrans.position = cv;
    	}
    }

}
