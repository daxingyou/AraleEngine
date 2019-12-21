using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Arale.Engine;


public class AIPlugin : Plugin
{
    public const int StateCmd = 0xFF;
    public enum Cmd
    {
        Idle  =0,//空闲
        Attack=1,//攻击敌人
        Patrol=2,//巡逻
        Trace =3,//跟随
        Flee  =5,//逃离
        Detect=0x100,//侦测敌人
    }

	public AIPlugin(Unit unit):base(unit){}
    public bool isPlaying{ get; protected set;}
    public virtual bool startAI (string btPath)
    {
        warnningHP = 30;
        startCmd(Cmd.Detect);
        isPlaying = true;
        return true;
    }

    public virtual void stopAI()
    {
        if (!isPlaying)return;
        isPlaying = false;
        mUnit.unbindLua();
    }

    public override void reset ()
    {
        stopAI();
        trigCmd = 0;
        stateCmd= 0;
    }

    public int   warnningHP; //警戒血量
    public float sleepTime;  //休眠时间
    public float minDistance;//跟随最近距离
    public float maxDistance;//跟随最大距离
    public Vector3 targetPos;//导航目标点
    public Unit   targetUnit;//导航目标
    int trigCmd;
    int stateCmd;
    public void startCmd(Cmd cmd)
    {
        if ((int)cmd < StateCmd)
        {//触发型
            trigCmd = (int)cmd;
            doCmd(cmd);
        }
        else
        {//状态型
            stateCmd|=(int)cmd;   
        }
    }

    public void stopCmd(Cmd cmd)
    {
        if ((int)cmd < StateCmd)
        {//触发型
            if((int)cmd!=trigCmd)return;
            trigCmd = 0;
        }
        else
        {//状态型
            stateCmd&=~(int)cmd;   
        }
    }

    void doCmd(Cmd cmd)
    {
        switch (cmd)
        {
            case Cmd.Attack:
                mUnit.skill.targetUnit = targetUnit;
                mUnit.skill.playIndex(0, true);
                break;
            case Cmd.Patrol:
                mUnit.move.nav(targetPos);
                break;
            case Cmd.Trace:
                mUnit.move.nav(targetUnit, minDistance);
                break;
            case Cmd.Flee:
                mUnit.move.nav(targetPos);
                break;
            default:
                break;
        }
    }
        
    bool isStateCmd(Cmd cmd)
    {
        return (stateCmd & (int)cmd) != 0;
    }

    public override void update()
    {
        if (!isPlaying)return;
        if (isStateCmd(Cmd.Detect))
        {
            int ut = (mUnit is Player) ? UnitType.Monster : UnitType.Player;
            List<Unit> ls = mUnit.mgr.getEnemy(mUnit, ut, 3, 1);
            if (ls.Count > 0)
            {
                onEvent((int)UnitEvent.AIEnemyFound, ls[0]);
            }
        }

        if (trigCmd == (int)Cmd.Idle && (sleepTime -= Time.deltaTime)<0)
        {
            onEvent((int)UnitEvent.AIWakeUp, 0);
        }
    }

    public override void onEvent(int evt, object param)
    {
        if (!isPlaying)return;
        switch (evt)
        {
            case (int)UnitEvent.NavEnd:
                break;
            case (int)UnitEvent.AIEnemyFound:
                targetUnit = param as Unit;
                stopCmd(Cmd.Detect);
                startCmd(Cmd.Attack);
                break;
            case (int)UnitEvent.AIEnemyDied:
                targetUnit = null;
                sleepTime = Random.Range(3, 8);
                startCmd(Cmd.Detect);
                break;
            case (int)UnitEvent.AIWakeUp:
                startCmd(Cmd.Patrol);
                break;
            case (int)UnitEvent.BeHit:
                Unit attacker = mUnit.mgr.getUnit((uint)param);
                if (attacker == null)break;
                if (mUnit.attr.HP > warnningHP && warnningHP > 0)
                {
                    targetUnit = attacker;
                    startCmd(Cmd.Attack);
                }
                else
                {
                    Vector3 dir = mUnit.pos - attacker.pos;
                    targetPos = mUnit.pos + dir.normalized * 5;
                    startCmd(Cmd.Flee);
                }
                break;
            case (int)UnitEvent.SkillEnd:
                if (trigCmd == (int)Cmd.Attack)startCmd(Cmd.Attack);
                break;
        }
    }
}
