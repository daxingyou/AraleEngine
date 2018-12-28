using UnityEngine;
using System.Collections;

namespace Arale.Engine
{

    public class CameraController : MonoBehaviour
    {
    	[System.NonSerialized]public Camera mCam;
    	[System.NonSerialized]public Transform mTarget;
		[System.NonSerialized]public float mDistance;
		[System.NonSerialized]public float mSmooth=3;//平滑系数
		Transform mTrans;
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
			FollowPos();
    	}

		void FollowPos()
		{
			Vector3 targetPos = mTarget.position + new Vector3 (0, 9, -10);
			mTrans.position = Vector3.Lerp (mTrans.position, targetPos, mSmooth * Time.deltaTime);
		}

		void FollowPosDir()
		{
			Vector3 targetPos = mTarget.position + new Vector3 (0, 9, -10);
			mTrans.position = Vector3.Lerp (mTrans.position, targetPos, mSmooth * Time.deltaTime);
			Quaternion angle = Quaternion.LookRotation (mTarget.position - mTrans.position);
			mTrans.rotation = Quaternion.Slerp (mTrans.rotation, angle, mSmooth * Time.deltaTime);
		}
    }

}
