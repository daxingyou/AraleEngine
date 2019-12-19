using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using Arale.Engine;


#region Skill基类
public class Skill
{
    //指向类型
    public enum PointType
	{
        None,  //直接施法(无指向)
		Pos,   //位置施法
		Dir,   //方向施法
		Target,//目标施法
	}

    public Skill(int skillID)
    {
        GS = GameSkill.get(skillID); 
    }

    public int   mLV;         //技能等级
    public float mRCD;        //技能RCD
    public GameSkill GS{get;protected set;}
	protected virtual void play(Unit self, bool aiCall)
	{
        if (mRCD > 0)return;
		Vector3 tPos = self.skill.targetPos;
		Unit    tUnit= self.skill.targetUnit;
        switch (GS.pointType)
        {
        case PointType.Pos://位置技能
			if (Vector3.Distance(self.pos, tPos) > GS.distance)
            {
                self.move.nav(tPos, GS.distance, delegate(bool isAI){if(isAI)playSkill(self);});
                return;
            }
            break;

        case PointType.Dir://方向技能
			if (!aiCall)break;
			if (Vector3.Distance(self.pos, tUnit.pos) > GS.distance)
			{
                self.move.nav(tUnit, GS.distance, delegate(bool isAI){if(isAI)playSkill(self);});
				return;
			}
            break;

        case PointType.Target://目标技能
			if (tUnit == null)
			{
				Debug.LogError ("目标技能未指定目标");
				return;
			}
			if (Vector3.Distance(self.pos, tUnit.pos) > GS.distance)
            {
                self.move.nav(tUnit, GS.distance, delegate(bool isArray){if(isArray)playSkill(self);});
                return;
            }
            break;

        default:
            break;
        }
        playSkill(self);
    }

    void playSkill(Unit unit)
    {
        if (!unit.isState (UnitState.Skill))return;
        unit.move.stop ();
        if (unit.isServer)
        {
            unit.buff.addSkill(GS.id);
        }
        else
        {
            //请求服务器释放技能
            MsgSkill msg = new MsgSkill();
            msg.guid      = unit.guid;
            msg.skillTID  = GS.id;
			msg.targetPos = unit.skill.targetPos;
			msg.targetGUID= unit.skill.targetGUID;
			unit.sendMsg((short)MyMsgId.Skill, msg);
        }
    }

	#region 插件
	public class Plug : Plugin
	{
		public Vector3 targetPos; //当前技能目标位置
        public Vector3 targetDir{get{return(targetPos - mUnit.pos).normalized;}}
		public Unit targetUnit;   //当前技能目标单位
		public uint targetGUID{get{return targetUnit == null ? 0 : targetUnit.guid;}}
		List<Skill> mSkills = new List<Skill>();
		public List<Skill> skills{get{return mSkills;}} 
		public Plug(Unit unit):base(unit){}

        IndicatorMesh mIndicator;//技能指示器
		public override void reset ()
		{
			targetPos = Vector3.zero;
			targetUnit = null;
            GameObject.DestroyObject(mIndicator);
            mIndicator = null;
			mSkills.Clear ();
		}

		public void addSkills(int[] skillTIDs)
		{
			for (int i = 0; i < skillTIDs.Length; ++i)addSkill (skillTIDs [i]);
		}

		public Skill addSkill(int skillTID)
		{
            Skill skill = new Skill (skillTID);
            mSkills.Add(skill);
            return skill;
		}

		public void delKill(int skillTID)
		{
            Skill skill = mSkills.Find(delegate(Skill s){return s.GS.id == skillTID;});
			if (skill != null)mSkills.Remove (skill);
		}

		public void play(int skillTID, bool aiCall=false)
		{
            Skill skill = mSkills.Find(delegate(Skill s){return s.GS.id == skillTID;});
			if (skill == null)
			{
				Log.i("skill tid not exit, tid="+skillTID, Log.Tag.Skill);
				return;
			}
            playSkill(skill, aiCall);
		}

		public void playIndex(int idx, bool aiCall=false)
		{
			if (mSkills.Count <= idx)
			{
				Log.i("skill index not exit, idx="+idx, Log.Tag.Skill);
				return;
			}
            playSkill(mSkills [idx], aiCall);
		}

        void playSkill(Skill skill, bool aiCall)
        {
            if (!mUnit.isState (UnitState.Skill))return;
            Log.i("playIndex skill="+skill.GS.id, Log.Tag.Skill);
            skill.play (mUnit, aiCall);
        }

		public virtual bool onEvent (int evt, object param, object sender)
		{
			if (evt == (int)UnitEvent.SkillBegin)
            {
			    
			}
            else if (evt == (int)UnitEvent.SkillEnd)
            {
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
            DebugLine.drawCircle(targetPos, 0.7f, Color.yellow);
			if (targetUnit != null)DebugLine.drawCircle(targetUnit.pos, 0.7f, Color.magenta);
		}


        public void showIndicator(Skill skill, Vector2 dir, float disPercent, bool end)
        {
            if (mIndicator == null)mIndicator = mUnit.gameObject.AddComponent<IndicatorMesh>();
            mIndicator.Show(skill.GS, dir, disPercent, !end);
            if (end)
            {
                Vector3 d = new Vector3(dir.x, 0, dir.y);
                targetPos = mUnit.pos + skill.GS.distance * disPercent*d;
                playSkill(skill,false);
            }
        }
	}
	#endregion
}
#endregion
