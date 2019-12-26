using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Arale.Engine;
using System;


public class AIPlugin : Plugin
{
    public enum Cmd
    {
        None  =0,
        Idle  =1,//空闲
        Attack=2,//攻击敌人
        Patrol=3,//巡逻
        Trace =4,//跟随
        Flee  =5,//逃离
    }
    public const int TimerDectect = 1;
    public const int TimerWakeup  = 2;
    public const int TimerRedo    = 4;
    public const int TimerTrace   = 5;

    public int     warnningHP; //警戒血量
    public float   minDistance;//跟随最近距离
    public float   maxDistance;//跟随最大距离
    public Vector3 bornPos;    //出生点 
    public float   patrolArea; //巡逻范围
    public Vector3 targetPos;  //导航目标点
    public Unit    targetUnit; //导航目标
    public  Cmd     cmd{get;protected set;}//当前触发命令
    Cmd     next;       //下一个命令

	public AIPlugin(Unit unit):base(unit){}
    public bool isPlaying{ get; protected set;}
    protected virtual bool startAI (string btPath)
    {
        warnningHP = 30;
        bornPos    = mUnit.pos;
        patrolArea = 10f;
        isPlaying = true;
        next = Cmd.Idle;
        return true;
    }

    protected virtual void stopAI()
    {
        if (!isPlaying)return;
        isPlaying = false;
    }

    public override void reset ()
    {
        stopAI();
        cmd = Cmd.None;
    }

    void failed(Cmd cmd)
    {
        if (this.cmd != cmd)return;
        next = cmd;
    }

    void doCmd(Cmd cmd)
    {
        this.cmd = cmd;
        switch (cmd)
        {
            case Cmd.Attack:
                attack();
                break;
            case Cmd.Flee:
                flee();
                break;
            case Cmd.Patrol:
                patrol();
                break;
            case Cmd.Idle:
                idle();
                break;
            case Cmd.Trace:
                trace();
                break;
        }
    }

    public override void update()
    {
        if (next != Cmd.None)
        {
            Cmd cmd = next;
            next = Cmd.None;
            doCmd(cmd);
        }
    }

    public override void onEvent(int evt, object param)
    {
        if (evt == (int)UnitEvent.AIStart)
        {
            startAI(param as string);
            return;
        }
        else if(evt == (int)UnitEvent.AIStop)
        {
            stopAI();
            return;
        }

        if (!isPlaying)return;
        if (evt == (int)UnitEvent.Timer)
        {
            mOnTimer(param as Timer.Node);
        }
        else
        {
            mOnEvent(evt, param);
        }
    }

    Action<int,object> mOnEvent;
    Timer.OnTimer mOnTimer;

    #region 查找目标
    bool findTarget()
    {
        int ut = (mUnit is Player) ? UnitType.Monster : UnitType.Player;
        List<Unit> ls = mUnit.mgr.getEnemy(mUnit, ut, 3, 1);
        if (ls.Count > 0)targetUnit = ls[0];
        return ls.Count > 0;
    }
    #endregion


    #region 被击
    void onBeHit(uint guid)
    {
        Unit attacker = mUnit.mgr.getUnit(guid);
        if (attacker == null)return;
        if (mUnit.attr.HP < warnningHP && warnningHP > 0)
        {//逃跑
            Vector3 dir = mUnit.pos - attacker.pos;
            targetPos = mUnit.pos + dir.normalized * 5;
            next = Cmd.Flee;
        }
        else
        {//反击
            targetUnit = attacker;
            next = Cmd.Attack;
        }
    }
    #endregion

    #region 空闲
    void idle()
    {
        mOnEvent = onIdleEvent;
        mOnTimer = onIdleTimer;
        mUnit.AddTimer(TimerDectect, 0.1f);
        mUnit.AddTimer(TimerWakeup, Randoms.rang(3, 8));
    }

    void onIdleTimer(Timer.Node tn)
    {
        switch (tn.timerID)
        {
            case TimerDectect:
                if (mUnit.attr.HP < warnningHP && warnningHP > 0)
                {
                    tn.loop(1f);
                    break;
                }

                if (findTarget())
                {
                    next = Cmd.Attack;
                }
                else
                {
                    tn.loop(0.1f);
                }
                break;
            case TimerWakeup:
                next = Cmd.Patrol;
                break;
        }
    }

    void onIdleEvent(int evt, object param)
    {
        switch (evt)
        {
            case (int)UnitEvent.BeHit:
                onBeHit((uint)param);
                break;
        }
    }
    #endregion


        
    #region 攻击
    void attack()
    {
        mOnEvent = onAttackEvent;
        mOnTimer = onAttackTimer;
        if (targetUnit == null || !targetUnit.isState(UnitState.Alive))
        {
            next = Cmd.Idle;
            return;
        }
 
        mUnit.skill.targetUnit = targetUnit;
        Skill sk = mUnit.skill.getSkill(Skill.FuncType.Attack);
        if (Vector3.Distance(mUnit.pos, targetUnit.pos) <= sk.GS.distance)
        {
            mUnit.skill.targetPos = targetUnit.pos;
            mUnit.skill.play(sk.GS.id);
        }
        else
        {
            mUnit.move.nav(targetUnit, sk.GS.distance,delegate(bool arrive)
                {
                    if (arrive)
                    {
                        mUnit.skill.targetPos = targetUnit.pos;
                        mUnit.skill.play(sk.GS.id);
                    }
                    else
                    {
                        failed(Cmd.Attack);
                    }
                });
        }
    }

    void onAttackTimer(Timer.Node tn)
    {
    }

    void onAttackEvent(int evt, object param)
    {
        switch (evt)
        {
            case (int)UnitEvent.BeHit:
                onBeHit((uint)param);
                break;
            case (int)UnitEvent.SkillEnd:
                failed(Cmd.Attack);
                break;
        }
    }
    #endregion

    #region 巡航
    void patrol()
    {
        float r = Randoms.rang(3f, this.patrolArea);
        float ang=Randoms.rang(0f, 2*Mathf.PI);
        targetPos = bornPos+new Vector3(Mathf.Cos(ang),0,Mathf.Sin(ang))*r;

        mOnEvent = onPatrolEvent;
        mOnTimer = onPatrolTimer;
        mUnit.move.nav(targetPos, 0, delegate(bool arrive)
            {
                if (arrive)
                {
                    next=Cmd.Idle;
                }
                else
                {
                    failed(Cmd.Patrol);
                }
            });
    }
    void onPatrolTimer(Timer.Node tn)
    {
        switch (tn.timerID)
        {
            case TimerDectect:
                if (mUnit.attr.HP < warnningHP && warnningHP > 0)
                {
                    tn.loop(1f);
                    break;
                }

                if (findTarget())
                {
                    next = Cmd.Attack;
                }
                else
                {
                    tn.loop(0.1f);
                }
                break;
        }
    }
    void onPatrolEvent(int evt, object param)
    {
        switch (evt)
        {
            case (int)UnitEvent.BeHit:
                onBeHit((uint)param);
                break;
        }
    }
    #endregion

    #region 跟随
    void trace()
    {
        mOnEvent = onTraceEvent;
        mOnTimer = onTraceTimer;
        mUnit.move.nav(targetUnit, minDistance, delegate(bool arrive)
            {
                if (arrive)
                {
                    next = Cmd.Idle;
                }
                else
                {
                    failed(Cmd.Trace);
                }
            });
    }
    void onTraceTimer(Timer.Node tn)
    {
    }
    void onTraceEvent(int evt, object param)
    {
    }
    #endregion

    #region 逃离
    void flee()
    {
        mOnEvent = onFleeEvent;
        mOnTimer = onFleeTimer;
        mUnit.move.nav(targetPos, 0, delegate(bool arrive)
            {
                if (arrive)
                {
                    next = Cmd.Idle;
                }
                else
                {
                    failed(Cmd.Flee);
                }
            });
    }
    void onFleeTimer(Timer.Node tn)
    {
    }
    void onFleeEvent(int evt, object param)
    {
        
    }
    #endregion
}
