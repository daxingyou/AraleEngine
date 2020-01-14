using UnityEngine;
using System.Collections;

namespace Arale.Engine
{

    public class CameraController : MonoBehaviour
    {
    	[System.NonSerialized]public Camera mCam;
    	[System.NonSerialized]public Transform mTarget;
		[System.NonSerialized]public float mSmooth=3;//平滑系数
		protected Transform mTrans;
    	void Awake ()
    	{
            if (GRoot.single == null)
            {
                Debug.LogError("Goto Launch");
                UnityEngine.SceneManagement.SceneManager.LoadScene("Launch");
                return;
            }
    		mCam = GetComponent<Camera>();
            CameraMgr.single.AddCamera(mCam);
    		mTrans = transform;
    	}

    	void OnDestroy()
    	{
    		mTarget = null;
            CameraMgr.single.RemoveCamera(mCam);
    	}
    }

}
