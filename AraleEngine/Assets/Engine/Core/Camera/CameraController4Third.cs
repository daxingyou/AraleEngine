using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Arale.Engine
{

    //第三人称相机控制器(RPG)
    public class CameraController4Third : CameraController
    {
        public Vector2 mOffset = new Vector2(9f, -10f);//相机相对主角的水平偏移
        // Update is called once per frame
        void LateUpdate()
        {
            if (mTarget == null) return;
            Vector3 targetPos = mTarget.position + new Vector3(0, mOffset.x, mOffset.y);
            mTrans.position = Vector3.Lerp(mTrans.position, targetPos, mSmooth * Time.deltaTime);
        }
    }

}
