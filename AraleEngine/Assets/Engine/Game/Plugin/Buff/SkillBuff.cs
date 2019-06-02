using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using Arale.Engine;
using System.Collections.Generic;
using System.Xml;

public class SkillBuff : Buff
{
	Unit mUnit;
	public class Root
	{
		public Skill[] skill;
	}
	[System.Serializable]//必须加标签
	public class Skill
	{
		public Action[] action;
	}
	[System.Serializable]
	public class Action
	{
		public float  delay;
		public int    state;
		public string anim;
		public string area;
		public int    harm;
		public int    bullet;
		public int    buff;
		public int    move;
	}
	Skill skill;
	protected void buildActions(string skillCfg, int cfgId)
	{
		string txt = ResLoad.get (skillCfg).asset<TextAsset> ().text;
		Root r = JsonUtility.FromJson<Root> (txt);
		skill = r.skill [cfgId];
	}

	protected override void onInit(Unit unit)
	{
		buildActions (table.lua, table.param);
		mUnit  = unit;
        decUnitState (mUnit, UnitState.Move, true);
		mUnit.sendUnitEvent ((int)UnitEvent.SkillBegin,null,true);
		mUnit.forward (mUnit.skill.targetPos);
		for (int i = 0; i < skill.action.Length; ++i)
		{
			Action skillAction = skill.action [i];
			TimeMgr.Action a = timer.AddAction (new TimeMgr.Action ());
			a.doTime   = skillAction.delay;
			a.onAction = onAction;
			a.userData = i;
		}
	}

	void onAction(TimeMgr.Action self)
	{
		int idx = (int)self.userData;
		Action a = skill.action[idx];
		if (a.state != 0) {//掩码最高位不用
			if (a.state > 0)
                addUnitState(mUnit,a.state);
			else
                decUnitState(mUnit,-a.state);
		}

		if (a.anim != null) {
			mUnit.anim.sendEvent(AnimPlugin.PlayAnim, a.anim);
		}
			
		if (a.harm != 0 && a.bullet==0) {
			if (a.area != null)
			{
				IArea garea = GameArea.fromString (a.area);
				Matrix4x4 mt = Matrix4x4.TRS (mUnit.pos, Quaternion.LookRotation (mUnit.dir), Vector3.one).inverse;
				List<Unit> units = mUnit.mgr.getUnitInArea (UnitType.Monster|UnitType.Player, garea, mt);
				for (int i = 0; i < units.Count; ++i)
				{
					Unit u = units [i];
					if (u.guid == mUnit.guid)continue;
					u.dir = (mUnit.pos - u.pos).normalized;
					u.anim.sendEvent (AnimPlugin.Hit);
					AttrPlugin ap = u.attr;
					ap.HP -= a.harm;
					ap.sync ();
                    u.sendUnitEvent((int)UnitEvent.BeHit,mUnit.guid,true);
				}
			}
			else
			{
				Unit u = mUnit.skill.targetUnit;
				if (u == null)return;
				u.dir = (mUnit.pos - u.pos).normalized;
				u.anim.sendEvent (AnimPlugin.Hit);
				AttrPlugin ap = u.attr;
				ap.HP -= a.harm;
				ap.sync (); 
                u.sendUnitEvent((int)UnitEvent.BeHit,mUnit.guid,true);
			}
		}

		if (a.bullet != 0) {
			Bullet bt = mUnit.mgr.getUnit (0, UnitType.Bullet, Mathf.Abs(a.bullet)) as Bullet;
			bt.setParam (mUnit.pos, mUnit.dir, mUnit);
			bt.mHarm = a.harm;
			bt.mOwner = mUnit.guid;
			if (a.buff != 0)
			{//子弹上绑定buff
				bt.buff.addBuff (a.buff);
				a.buff = 0;//避免后面的buff绑定
			}
			Vector3 v = mUnit.skill.targetPos;
			v.y = bt.pos.y;
			if (a.bullet > 0)
			{//第一个参数为位置
				bt.play (mUnit.skill.targetPos, mUnit.skill.targetGUID);
			}
			else
			{//第一个参数为方向
				bt.play ((v - bt.pos).normalized, mUnit.skill.targetGUID);
			}
		}

		if (a.buff != 0) {
			mUnit.buff.addBuff (a.buff);
		}

        if (a.move != 0){
			if (a.area != null)
			{
				IArea garea = GameArea.fromString (a.area);
				Matrix4x4 mt = Matrix4x4.TRS (mUnit.pos, Quaternion.LookRotation (mUnit.dir), Vector3.one).inverse;
				List<Unit> units = mUnit.mgr.getUnitInArea (UnitType.Monster|UnitType.Player, garea, mt);
				for (int i = 0; i < units.Count; ++i)
				{
					Unit u = units [i];
					if (u.guid == mUnit.guid)continue;
					u.dir = (mUnit.pos - u.pos).normalized;
					if(a.move>0)
						u.move.play(a.move,mUnit.pos-garea.R*u.dir,null,true);//击退到攻击范围的最远点
					else
                        u.move.play(Mathf.Abs(a.move),-u.dir,null,true);//击退指定距离
				}
			}
			else
			{
				Unit u = mUnit.skill.targetUnit;
				if (u == null)return;
				u.dir = (mUnit.pos - u.pos).normalized;
                u.move.play(a.move,-u.dir,null,true);//击退指定距离
			}
		}

		if (idx+1 >= skill.action.Length)
		{
			mState = 0;
		}
	}

	protected override void onDeinit()
	{
        addUnitState (mUnit, UnitState.Move, true);
		mUnit.sendUnitEvent ((int)UnitEvent.SkillEnd, null, true);
		mUnit = null;
	}
}
