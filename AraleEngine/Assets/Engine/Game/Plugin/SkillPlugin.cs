using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using Arale.Engine;


#region Skill基类
public class Skill
{
    public const int NoTarget = 1;
    public const int OutRange = 2;
    //指向类型
    public enum PointType
	{
        None,  //直接施法(无指向)
		Pos,   //位置施法
		Dir,   //方向施法
		Target,//目标施法
	}

    public enum FuncType
    {
        Attack=0x01,        //攻击
        Recover=0x02,       //恢复
        Control=0x04,       //控制
        DeControl=0x08,     //解控
        HarmStrengthen=0x10,//伤害加强
        HarmReduce=0x20,    //伤害减免
        Summon=0x40,        //召唤
        Move=0x80,          //位移
    }

    public Skill(int skillID)
    {
        GS = GameSkill.get(skillID); 
    }

    public int   mLV;         //技能等级
    public float mRCD;        //技能RCD
    public GameSkill GS{get;protected set;}
	protected virtual void play(Unit self)
	{
        if (mRCD > 0)return;
        Unit    tUnit= self.skill.targetUnit;
        Vector3 tPos = self.skill.targetPos;
        switch (GS.pointType)
        {//服务端技能不在距离内也施法，但不一定造成伤害.如果和客户端一样做距离跟踪处理，则会因为客户端和服务端通信延迟导致距离差距不一致，客户端人可以可以释放技能，服务端认为需要继续跟踪，从而导致动画节奏不一致
            case PointType.Pos://位置技能
                if (self.isServer || Vector3.Distance(self.pos, tPos) <= GS.distance)break;
                self.move.nav(tPos, GS.distance, delegate(bool arrive){if(arrive)playSkill(self);});
                return;
            case PointType.Target://目标技能
                if (tUnit == null)
                {
                    Debug.LogError("未指定技能目标");
                    self.sendEvent((int)UnitEvent.SkillBegin, NoTarget);
                    return;
                }
                if (self.isServer || Vector3.Distance(self.pos, tUnit.pos) <= GS.distance)break;
                self.move.nav(tUnit, GS.distance, delegate(bool arrive){if (arrive)playSkill(self);});
                return;
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

		public void play(int skillTID)
		{
            Skill skill = mSkills.Find(delegate(Skill s){return s.GS.id == skillTID;});
			if (skill == null)
			{
				Log.i("skill tid not exit, tid="+skillTID, Log.Tag.Skill);
				return;
			}
            playSkill(skill);
		}

		public void playIndex(int idx)
		{
			if (mSkills.Count <= idx)
			{
				Log.i("skill index not exit, idx="+idx, Log.Tag.Skill);
				return;
			}
            playSkill(mSkills [idx]);
		}

        void playSkill(Skill skill)
        {
            if (!mUnit.isState (UnitState.Skill))return;
            Log.i("playIndex skill="+skill.GS.id, Log.Tag.Skill);
            skill.play (mUnit);
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

        public void showIndicator(Skill skill, Vector2 dir, float disPercent, bool end)
        {
            if (mIndicator == null)mIndicator = mUnit.gameObject.AddComponent<IndicatorMesh>();
            mIndicator.Show(skill.GS, dir, disPercent, !end);
            if (end)
            {
                Vector3 d = new Vector3(dir.x, 0, dir.y);
                targetPos = mUnit.pos + skill.GS.distance * disPercent*d;
                playSkill(skill);
            }
        }

        public Skill getSkill(Skill.FuncType type, bool playAble=false)
        {
            for (int i = 0; i < skills.Count; ++i)
            {
                Skill sk = skills[i];
                if ((sk.GS.funcType & type)==0)continue;
                return sk;
            }
            return null;
        }

        #if UNITY_EDITOR
        public void drawDebug()
        {
            DebugLine.drawCircle(targetPos, 0.7f, Color.yellow);
            if (targetUnit != null)DebugLine.drawCircle(targetUnit.pos, 0.7f, Color.magenta);
        }
        #endif
	}
	#endregion
}
#endregion
