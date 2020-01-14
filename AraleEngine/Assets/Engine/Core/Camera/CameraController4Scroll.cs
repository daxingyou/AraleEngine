using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Arale.Engine
{

    //卷屏相机控制器
    //2d游戏卷屏,mBackMove=false 街机93快打模式,mBackMove=true 雷电模式(打飞机模式，实际背景在动，飞机不动)
    public class CameraController4Scroll : CameraController
    {
        public Bounds mWinBox;//滑动窗口，限制camera视口在区域内移动
        public bool  mBackMove;//背景移动
        // Update is called once per frame
        void LateUpdate()
        {
            if (mTarget == null) return;
            Vector3 targetPos = mTarget.position + new Vector3(0, 0, -10);
            mTrans.position = mBackMove ? Vector3.Lerp(mTrans.position, targetPos, mSmooth * Time.deltaTime) : targetPos;
        }

        void ClampInWindow()
        {//如果有不期望的结果考虑mWinBox.z没设置的原因
            if (mWinBox == null || mWinBox.min == mWinBox.max) return;
            Vector3 v = mTrans.position;
            Vector3 vmin = mCam.ViewportToWorldPoint(new Vector3(0, 0, 0));
            Vector3 vmax = mCam.ViewportToWorldPoint(new Vector3(1, 1, 1));
            Vector3 wmin = mWinBox.min;
            Vector3 wmax = mWinBox.max;
            if (vmin.x < wmin.x) v.x = wmin.x + 0.5f * (vmax.x - vmin.x);
            if (vmax.x > wmax.x) v.x = wmax.x - 0.5f * (vmax.x - vmin.x);
            if (vmin.y < wmin.y) v.y = wmin.y + 0.5f * (vmax.y - vmin.y);
            if (vmax.y > wmax.y) v.y = wmax.y - 0.5f * (vmax.y - vmin.y);
            if (vmin.z < wmin.z) v.z = wmin.z + 0.5f * (vmax.z - vmin.z);
            if (vmax.z > wmax.z) v.z = wmax.z - 0.5f * (vmax.z - vmin.z);
            mTrans.position = v;
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {//对应的脚本在inspector必须为展开状态，否则不会被调用
            if (mWinBox == null) return;
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(mWinBox.center, mWinBox.size);
        }
#endif
    }

}
