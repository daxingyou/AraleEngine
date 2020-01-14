using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Arale.Engine
{

    //第一人称相机控制器(ACT)
    public class CameraController4First : CameraController
    {
        public float mYOffset = 1;//视点y偏移
        public float mYPRRate = 90;//视角调整系数
        public float mDisRate = 100;//视距调整系数
        float mYaw;  //偏航角(Y轴旋转)
        float mPitch;//俯仰角(X轴旋转)
        float mDistance=10;//视距
        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            //mYaw   = mTrans.eulerAngles.y;
            //mPitch = -mTrans.eulerAngles.x;
            mPitch = -45;
        }

        void LateUpdate()
        {
            if (mTarget == null) return;
            //Quaternion q = Quaternion.Euler(-mPitch,mYaw,0);
            //mTrans.localRotation = q;//not well
            mTrans.localRotation = Quaternion.AngleAxis(mYaw, Vector3.up);
            mTrans.localRotation *= Quaternion.AngleAxis(-mPitch, Vector3.right);
            mTrans.position = mTarget.position - mTrans.forward * mDistance + Vector3.up*mYOffset;
        }

        void Update()
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                mYaw   += Input.GetAxis("Mouse X") * mYPRRate * Time.deltaTime;
                mPitch += Input.GetAxis("Mouse Y") * mYPRRate * Time.deltaTime;
                mPitch = Mathf.Clamp(mPitch, -90, 90);
                mDistance += mDisRate* Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime;
                mDistance = Mathf.Clamp(mDistance, 0, 30);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Cursor.lockState = (Cursor.lockState == CursorLockMode.None) ? CursorLockMode.Locked : CursorLockMode.None;
            }
        }
    }

}
