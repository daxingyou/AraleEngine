using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Arale.Engine;


public abstract class AIPlugin : Plugin
{
	public AIPlugin(Unit unit):base(unit){}
    public virtual  bool onEvent (int evt, object param, object sender){return false;}
	public abstract bool isPlay{ get; }
	public abstract bool startAI (string btPath);
	public abstract void stopAI ();

	public Unit target{ get; set;}
    public bool busy{ get; set;}
	protected List<Vector3> mPatrolPoint;
	protected int           mPatrolIndex;
	protected Vector3       mPatrolCenter;
	protected float         mPatrolArea;
	protected Vector3       mFlee;
	public void setPatrolPoint(List<Vector3> points)
	{
		mPatrolPoint = points;
		mPatrolIndex = 0;
	}
	public void setPatrolArea(Vector3 center, float r)
	{
		mPatrolCenter = center;
        if (mPatrolCenter == Vector3.zero)mPatrolCenter = mUnit.pos;
		mPatrolArea = r;
	}
	public void setFlee(Vector3 v)
	{
		mFlee = v;
	}

	//巡逻
	public bool doPatrol(int type)
	{
        busy = true;
		switch (type)
		{
    		case 1://顺序点巡逻
    			if (mPatrolPoint == null || mPatrolPoint.Count < 1)break;
                mUnit.move.nav (mPatrolPoint [mPatrolIndex]);
    			mPatrolIndex = ++mPatrolIndex % mPatrolPoint.Count;
    			return true;
    		case 2://随机点巡逻
    			if (mPatrolPoint == null || mPatrolPoint.Count < 1)break;
    			int idx = Random.Range (0, mPatrolPoint.Count);
    			if (idx == mPatrolIndex)break;
                mUnit.move.nav (mPatrolPoint [mPatrolIndex = idx]);
    			return true;
            case 3://范围随机巡逻
                float r   = Randoms.rang(0, mPatrolArea);
                float ang = Randoms.rang(0, 2*Mathf.PI);
                Vector3 pos = mPatrolCenter + new Vector3(Mathf.Cos(ang), 0, Mathf.Sin(ang)) * r;
                mUnit.move.nav (pos);
			    return true;
    		default:
    			break;
		}
        busy = false;
		return false;
	}

	//查找目标
	public bool doTarget(int unitType)
	{
		List<Unit> ls = null;
		ls = mUnit.mgr.getEnemy (mUnit, unitType, 10f, 1);
		if (ls.Count < 1)return false;
		target = ls [0];
		return true;
	}

	//释放技能
	public bool doSkill(int idx)
	{
		if (!target.isState (UnitState.Alive))
		{
			target = null;
			mUnit.skill.targetUnit = null;
			return false;
		}
		mUnit.skill.targetPos  = target.pos;
		mUnit.skill.targetUnit = target;
		mUnit.skill.playIndex (idx, true);
        busy = true;
		return true;
	}

	//逃离
	public bool doFlee(int type)
	{
        busy = true;
		switch (type)
		{
            case 0://目标方向逃离
                mUnit.move.nav(mUnit.pos + mFlee * 5);
    			return true;
    		case 1://反向逃离
    			if (target == null)break;
    			Vector3 dir = mUnit.pos - target.pos;
                mUnit.move.nav (mUnit.pos + dir * 3);
    			return true;
    		case 2://目标点逃离
                mUnit.move.nav (mFlee);
    			return true;
		}
        busy = false;
		return false;
	}
}
