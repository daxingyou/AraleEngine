using UnityEngine;
using System.Collections;

namespace Arale.Engine
{

    public class ACMovement{
    	public enum MoveType{
    		Centripetence,
    		Yaw,
    		Dir,
    		Position,
    	};
    	public Transform trans;
    	public Transform target;
    	public float duration;
    	float t;
    	float k;
    	AnimationCurve acS;
    	AnimationCurve acX;
    	AnimationCurve acY;
    	AnimationCurve acZ;
    	delegate void UpdateFunc();
    	UpdateFunc _updateFunc;
    	public ACMovement(MoveType mt, string acName)
    	{
    		t = 0;
    		k = 0.3f;
            acS = AC.Get(acName, "s");
            acX = AC.Get(acName, "x");
            acY = AC.Get(acName, "y");
            acZ = AC.Get(acName, "z");
    		switch(mt)
    		{
    		case MoveType.Centripetence:
    			_updateFunc=CentripetenceUpdate;
    			break;
    		case MoveType.Yaw:
    			_updateFunc=YawUpdate;
    			break;
    		case MoveType.Dir:
    			_updateFunc=DirUpdate;
    			break;
    		case MoveType.Position:
    			_updateFunc=PosUpdate;
    			break;
    		default:
    			break;
    		}
    	}

    	public void Play()
    	{
    		if(_updateFunc==null)return;
    		trans.LookAt (target);
    	}

    	// Update is called once per frame
    	public bool Update () {
    		if(t>=duration)return false;
    		float  dt = Time.deltaTime;
    		_updateFunc ();
    		t += dt;
    		return true;
    	}

    	//向心力运动/
    	void CentripetenceUpdate()
    	{
    		Vector3 vz = (target.position - trans.position).normalized; //前进速度/
    		Vector3 vx = Vector3.Cross (vz, Vector3.up).normalized;//切线速度/
    		Vector3 vy = Vector3.Cross (vx, vz).normalized;        //提升速度/
    		
    		Vector3 p = trans.position;
    		p += vx * k * acX.Evaluate (t / duration);
    		p += vy * k * acY.Evaluate (t / duration);
    		Vector3 d = vz*k*acZ.Evaluate(t/duration);
    		if (d.magnitude >= (target.position - trans.position).magnitude)
    		{
    			t = duration;
    			p = trans.position;
    		}
    		else
    		{
    			p+=d;
    		}
    		trans.position = p;
    	}

    	//旋转，偏航/
    	float mx = 0;
    	float my = 0;
    	float mz = 0;
    	void YawUpdate()
    	{
    		//mx,my,mz表示绕x,y,z轴的旋转角度/
    		if(null!=acX)mx += acX.Evaluate (t / duration)*360;
    		if(null!=acY)my += acY.Evaluate (t / duration)*360;
    		if(null!=acZ)mz += acZ.Evaluate (t / duration)*360;
    		if(mx>=360)mx-=360;
    		if(my>=360)my-=360;
    		if(mz>=360)mz-=360;
    		trans.localRotation = Quaternion.Euler (mx, my, mz);
    		Vector3 p = trans.position;
    		p+=trans.forward.normalized*k*acS.Evaluate(t/duration);
    		trans.position = p;
    	}

    	//方向/
    	void DirUpdate()
    	{
    		//mx,my,mz表示绕x,y,z轴的旋转角度/
    		if(null!=acX)mx = acX.Evaluate (t / duration)*360;
    		if(null!=acY)my = acY.Evaluate (t / duration)*360;
    		if(null!=acZ)mz = acZ.Evaluate (t / duration)*360;
    		if(mx>=360)mx-=360;
    		if(my>=360)my-=360;
    		if(mz>=360)mz-=360;
    		trans.localRotation = Quaternion.Euler (mx, my, mz);
    		Vector3 p = trans.position;
    		p+=trans.forward.normalized*k*acS.Evaluate(t/duration);
    		trans.position = p;
    	}

    	//位置/
    	void PosUpdate()
    	{
    		//mx,my,mz表示本地坐标系位置/
    		Vector3 v = trans.localPosition;
    		v.x = mx + k * acX.Evaluate (t / duration);
    		v.y = my + k * acY.Evaluate (t / duration);
    		v.z = mz + k * acZ.Evaluate (t / duration);
    		trans.localPosition = v;
    	}
    }

}