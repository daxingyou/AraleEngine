using UnityEngine;
using System.Collections;

namespace Arale.Engine
{

    public class CameraController : MonoBehaviour
    {
        public Bounds mWinBox;//滑动窗口，限制camera视口在区域内移动
        public Vector2 mOffset = new Vector2(9f,-10f);//相机相对主角的水平偏移
    	[System.NonSerialized]public Camera mCam;
    	[System.NonSerialized]public Transform mTarget;
		[System.NonSerialized]public float mDistance;
		[System.NonSerialized]public float mSmooth=3;//平滑系数
		Transform mTrans;
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

    	protected virtual void LateUpdate () {
    		if (mTarget == null)return;
			FollowPos();
    	}

        protected void ScrollPos(bool smooth=true)
        {//2d游戏卷屏,smooth=false 街机93快打模式,smooth=true 雷电模式(打飞机模式，实际背景在动，飞机不动)
            Vector3 targetPos = mTarget.position + new Vector3 (0, 0, -10);
            mTrans.position = smooth?Vector3.Lerp (mTrans.position, targetPos, mSmooth * Time.deltaTime):targetPos;
        }

        protected void FollowPos()
        {//相机平滑跟随(位置)
            Vector3 targetPos = mTarget.position + new Vector3 (0, mOffset.x, mOffset.y);
			mTrans.position = Vector3.Lerp (mTrans.position, targetPos, mSmooth * Time.deltaTime);
		}

        protected void FollowPosDir()
        {//相机平滑跟随(位置，角度)
            Vector3 targetPos = mTarget.position + new Vector3 (0, mOffset.x, mOffset.y);
			mTrans.position = Vector3.Lerp (mTrans.position, targetPos, mSmooth * Time.deltaTime);
			Quaternion angle = Quaternion.LookRotation (mTarget.position - mTrans.position);
			mTrans.rotation = Quaternion.Slerp (mTrans.rotation, angle, mSmooth * Time.deltaTime);
		}

        protected void ClampInWindow()
        {//如果有不期望的结果考虑mWinBox.z没设置的原因
            if (mWinBox == null||mWinBox.min == mWinBox.max)return;
            Vector3 v = mTrans.position;
            Vector3 vmin = mCam.ViewportToWorldPoint(new Vector3(0, 0, 0));
            Vector3 vmax = mCam.ViewportToWorldPoint(new Vector3(1, 1, 1));
            Vector3 wmin = mWinBox.min;
            Vector3 wmax = mWinBox.max;
            if (vmin.x < wmin.x)v.x = wmin.x + 0.5f * (vmax.x - vmin.x);
            if (vmax.x > wmax.x)v.x = wmax.x - 0.5f * (vmax.x - vmin.x);
            if (vmin.y < wmin.y)v.y = wmin.y + 0.5f * (vmax.y - vmin.y);
            if (vmax.y > wmax.y)v.y = wmax.y - 0.5f * (vmax.y - vmin.y);
            if (vmin.z < wmin.z)v.z = wmin.z + 0.5f * (vmax.z - vmin.z);
            if (vmax.z > wmax.z)v.z = wmax.z - 0.5f * (vmax.z - vmin.z);
            mTrans.position = v;
        }

        #if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {//对应的脚本在inspector必须为展开状态，否则不会被调用
            if(mWinBox==null)return;
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(mWinBox.center, mWinBox.size);
        }
        #endif
    }

}
