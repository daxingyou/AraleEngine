using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using Arale.Engine;


#region Skill基类
public class Skill
{
	public enum TargetType
	{
		Pos,   //位置施法
		Dir,   //方向施法
		Target,//目标施法
	}

    public Skill(int tid)
    {
        mTID = tid;
    }

    public int   mTID;        //技能配表ID
    public int   mLV;         //技能等级
    public float mRCD;        //技能RCD

	public virtual void play(Unit self, bool aiCall)
	{
        if (mRCD > 0)return;
		Vector3 tPos = self.skill.targetPos;
		Unit    tUnit= self.skill.targetUnit;
        TBSkill tb = TableMgr.single.GetData<TBSkill>(mTID);
        switch (tb.type)
        {
        case (int)TargetType.Pos://位置技能
			if (Vector3.Distance(self.pos, tPos) > tb.distance)
            {
				self.nav.startNav(tPos, tb.distance, delegate(bool isArray){if(isArray)playSkill(self);});
                return;
            }
            break;

		case (int)TargetType.Dir://方向技能
			if (!aiCall)break;
			if (Vector3.Distance(self.pos, tUnit.pos) > tb.distance)
			{
				self.nav.startNav(tUnit, tb.distance, delegate(bool isArray){if(isArray)playSkill(self);});
				return;
			}
            break;

		case (int)TargetType.Target://目标技能
			if (tUnit == null)
			{
				Debug.LogError ("目标技能未指定目标");
				return;
			}
			if (Vector3.Distance(self.pos, tUnit.pos) > tb.distance)
            {
				self.nav.startNav(tUnit, tb.distance, delegate(bool isArray){if(isArray)playSkill(self);});
                return;
            }
            break;

        default:
            return;
        }
        playSkill(self);
    }

    void playSkill(Unit unit)
    {
		if (unit.isState (UnitState.Skill))return;
		unit.nav.stopNav (false);
        if (unit.isServer)
        {
            TBSkill tb = TableMgr.single.GetData<TBSkill>(mTID);
            unit.buff.addBuff(tb.skillBuff);
        }
        else
        {
            //请求服务器释放技能
            MsgSkill msg = new MsgSkill();
            msg.guid      = unit.guid;
            msg.skillTID  = mTID;
			msg.targetPos = unit.skill.targetPos;
			msg.targetGUID= unit.skill.targetGUID;
			unit.sendMsg((short)MyMsgId.Skill, msg);
        }
    }

	#region 插件
	public class Plug : Plugin
	{
		public Skill skill;       //当前正在释放的技能
		public Vector3 targetPos; //当前技能目标位置
		public Unit targetUnit;   //当前技能目标单位
		public uint targetGUID{get{return targetUnit == null ? 0 : targetUnit.guid;}}
		List<Skill> mSkills = new List<Skill>();
		public List<Skill> skills{get{return mSkills;}} 
		public Plug(Unit unit):base(unit){}

		public override void reset ()
		{
			skill = null;
			targetPos = Vector3.zero;
			targetUnit = null;
			mSkills.Clear ();
		}

		public void addSkills(int[] skillTIDs)
		{
			for (int i = 0; i < skillTIDs.Length; ++i)addSkill (skillTIDs [i]);
		}

		public Skill addSkill(int skillTID)
		{
			TBSkill tb = TableMgr.single.GetData<TBSkill>(skillTID);
			Skill s = new Skill (skillTID);
			mSkills.Add(s);
			return s;
		}

		public void delKill(int skillTID)
		{
			Skill skill = mSkills.Find(delegate(Skill s){return s.mTID == skillTID;});
			if (skill != null)mSkills.Remove (skill);
		}

		public void play(int skillTID, bool aiCall=false)
		{
			if (this.skill != null)return;
			if (mUnit.isState (UnitState.Skill))return;
			Skill skill = mSkills.Find(delegate(Skill s){return s.mTID == skillTID;});
			if (skill == null)
			{
				Log.i("skill tid not exit, tid="+skillTID, Log.Tag.Skill);
				return;
			}
			Log.i("play skill="+skillTID, Log.Tag.Skill);
			skill.play(mUnit, aiCall);
			this.skill = skill;
		}

		public void playIndex(int idx, bool aiCall=false)
		{
			if (this.skill != null)return;
			if (mUnit.isState (UnitState.Skill))return;
			if (mSkills.Count <= idx)
			{
				Log.i("skill index not exit, idx="+idx, Log.Tag.Skill);
				return;
			}
			Skill skill = mSkills [idx];
			Log.i("playIndex skill="+skill.mTID, Log.Tag.Skill);
			skill.play (mUnit, aiCall);
			this.skill = skill;
		}

		public virtual bool onEvent (int evt, object param, object sender)
		{
			if (evt == (int)UnitEvent.SkillBegin){
					
			} else if (evt == (int)UnitEvent.SkillEnd) {
				skill = null;
			}
			return true;
		}

		public override void onSync(MessageBase msg)
		{
			MsgSkill m = msg as MsgSkill;
			targetPos  = m.targetPos;
			targetUnit = mUnit.mgr.getUnit (m.targetGUID);
			play (m.skillTID);
		}

		public void drawDebug()
		{
			DebugLine.drawCircle(targetPos, 0.7f, Color.red);
			if (targetUnit != null)DebugLine.drawCircle(targetUnit.pos, 0.7f, Color.magenta);
		}
	}
	#endregion
}
#endregion
