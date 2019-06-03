﻿#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using Arale.Engine;
using System;

public partial class GameSkill : AraleSerizlize
{
    static List<Node> selecteds = new List<Node>();
    static Color[] markClrs = new Color[]{Color.cyan,Color.blue,Color.magenta,Color.green,Color.red};
    public abstract partial class Node : AraleSerizlize
    {
        public Action action{ get; protected set;}
        Rect rc = new Rect(0,0,12,12);
        protected void drawNode(Vector3 pos, Color fillClr)
        {
            rc.center = pos;
            bool sel = selecteds.Contains(this);
            Vector3[] vs = new Vector3[]{ new Vector3(pos.x-6,pos.y,0), new Vector3(pos.x,pos.y+6,0), new Vector3(pos.x+6,pos.y,0), new Vector3(pos.x,pos.y-6,0)};
            Handles.color = sel ? Color.red : fillClr;
            Handles.DrawSolidRectangleWithOutline(vs, Handles.color, Color.black);
            if (Event.current == null || Event.current.rawType != EventType.MouseDown || !rc.Contains(Event.current.mousePosition))
                return;

            if (!Event.current.control)
            {
                selecteds.Clear();
            }

            if (inSameAction())
            {
                if (sel)
                    selecteds.Remove(this);
                else
                    selecteds.Add(this);
            }   
        }
        public virtual void setAction(Action a)
        {
            if (action != null)action.bullets.Remove(this as Bullet);
            action = a;
            a.bullets.Add(this as Bullet);
        }
        public virtual void draw(Vector3 pos){}
        public virtual bool drawGUI(){return true;}
        bool inSameAction()
        {
            if (selecteds.Count < 1)return true;
            Node n = selecteds[0];
            if (n.action == null)
            {
                return action == null ? object.ReferenceEquals(this,n) : object.ReferenceEquals(action,n);
            }
            else
            {
                return action == null ? object.ReferenceEquals(this,n.action) : object.ReferenceEquals(action,n.action);
            }
        }

        protected void drawMark(int idx, bool light)
        {
            if (!light)return;
            Vector3 pos = rc.center;
            pos.y += idx * 5+8;
            Vector3[] vs = new Vector3[]{ new Vector3(pos.x-2,pos.y-2,0), new Vector3(pos.x+2,pos.y-2,0), new Vector3(pos.x+2,pos.y+2,0), new Vector3(pos.x-2,pos.y+2,0)};
            Color clr = markClrs[idx];
            Handles.color = clr;
            Handles.DrawSolidRectangleWithOutline(vs, clr, Color.black);
        }
    }

    public partial class Action : Node
    {
        public virtual void setAction(GameSkill.Action a){}
        public void mergeTo(GameSkill.Action a)
        {
            for (int i = bullets.Count-1; i>=0; --i)bullets[i].setAction(a);
        }
        public override void draw(Vector3 pos)
        {
            drawNode(pos, Color.yellow);
            drawMark(0, loopTimes>0);
            drawMark(1, breakable);
            drawMark(2, !string.IsNullOrEmpty(anim));
            drawMark(3, state!=UnitState.ALL);
            pos.y += 35;
            for (int i = 0; i<bullets.Count; ++i)
            {
                bullets[i].draw(pos);pos.y += 20;
            }
        }

        public override bool drawGUI()
        {
            loopTimes = EditorGUILayout.IntField(loopTimes<0?"技能结束":"循环次数", loopTimes);
            if(loopTimes>0)loopInterval = EditorGUILayout.FloatField("循环间隔(s)", loopInterval);
            breakable = EditorGUILayout.Toggle("打断", breakable);
            anim = EditorGUILayout.TextField("动画", anim);
            state = EditorGUILayout.Toggle("生",(state & UnitState.Alive)!=0)?state|UnitState.Alive:state&(~UnitState.Alive);
            state = EditorGUILayout.Toggle("移",(state & UnitState.Move)!=0)?state|UnitState.Move:state&(~UnitState.Move);
            state = EditorGUILayout.Toggle("动",(state & UnitState.Anim)!=0)?state|UnitState.Anim:state&(~UnitState.Anim);
            state = EditorGUILayout.Toggle("技",(state & UnitState.Skill)!=0)?state|UnitState.Skill:state&(~UnitState.Skill);
            state = EditorGUILayout.Toggle("显",(state & UnitState.Show)!=0)?state|UnitState.Show:state&(~UnitState.Show);
            state = EditorGUILayout.Toggle("伤",(state & UnitState.Harm)!=0)?state|UnitState.Harm:state&(~UnitState.Harm);
            return true;
        }
    }

    public partial class Bullet : Node
    {
        public override void draw(Vector3 pos)
        {
            drawNode(pos, Color.green);
        }

        public override bool drawGUI()
        {
            id = EditorGUILayout.IntField("子弹ID", id);
            harm = EditorGUILayout.IntField("伤害", harm);
            buffId = EditorGUILayout.IntField("BuffID", buffId);
            moveId = EditorGUILayout.IntField("MoveID", moveId);
            mode = (Mode)EditorGUILayout.EnumPopup("模式", mode);
            for(int i=targets.Count-1;i>=0;--i)
            {
                EditorGUILayout.Space();
                if (!targets[i].drawGUI())targets.RemoveAt(i);
            }
            if(GUILayout.Button("添加目标"))
            {
                targets.Insert(0,new Target());
            }
            return true;
        }
    }

    public partial class Target : Node
    {
        public bool drawGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("目标:");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("X", GUI.skin.label))return false;
            GUILayout.EndHorizontal();
            unitTypeMask = EditorGUILayout.IntField("unit查找过滤器", unitTypeMask);
            maxTarget = EditorGUILayout.IntField("最大命中目标数", maxTarget);
            if (maxTarget < 1)maxTarget = 1;
            area = EditorGUILayout.TextField("子弹命中区域", area);
            return true;
        }
    }

    public static bool isDrag{get{return Event.current.alt;}}
    public void drawGUI(float tickLine)
    {
        string newName = EditorGUILayout.TextField("技能名称", name);
        if (!skills.ContainsKey(newName))
        {
            skills.Remove(name);
            name = newName;
            skills[name] = this;
            genNames();
        }

        switch (EditorGUILayout.Popup(0, new string[]{ "选定时间线处添加", "行为","子弹"}))
        {
            case 1:
                getAction(tickLine);
                break;
            case 2:
                new GameSkill.Bullet().setAction(getAction(tickLine));
                break;
        }

        if (selecteds.Count < 1)return;
        Node n = selecteds[selecteds.Count - 1];
        n.drawGUI();
    }

    Action createAction(float time)
    {
        Action act = new Action();
        actions.Add(act);
        act.time = time;
        return act;
    }

    public Action getAction(float time)
    {
        Action act = actions.Find(delegate(Action a){return a.time == time;});
        return act !=null?act as Action:createAction(time);
    }

    public void draw(Rect rc, float unitWidth)
    {
        Color clr = Handles.color;
        Vector3 v = new Vector3(rc.xMin, rc.center.y,0);
        for (int i = 0; i < actions.Count; ++i)
        {
            Action act = actions[i] as Action;
            act.draw(v + new Vector3(act.time * unitWidth, 0, 0));
        }
        Handles.color = clr;
    }

    Action dragAction;
    public void drag(float timeLine, bool draging)
    {
        if (selecteds.Count < 1)return;
        if (draging && isDrag)
        {
            if (dragAction == null)
            {
                Node n = selecteds[0];
                Action a = n as Action;
                dragAction = a == null ? n.action as Action : a;
                if (!selecteds.Contains(dragAction))
                {
                    dragAction = createAction(timeLine);
                    selecteds.Add(dragAction);
                    for (int i = 0; i < selecteds.Count; ++i)
                    {
                        n = selecteds[i];
                        if (n.action == null)continue;
                        n.setAction(dragAction);
                    }
                }
            }
            dragAction.time = timeLine;
        }
        else
        {
            if (dragAction == null)return;
            Action act = actions.Find(delegate(Action b){return b.time == dragAction.time&&!object.ReferenceEquals(b,dragAction);});
            if (act != null)
            {
                dragAction.mergeTo(act);
                actions.Remove(dragAction);
                selecteds.Remove(dragAction);
            }
            dragAction = null;
        }
    }

    public void deleteSelected()
    {
        for (int i = 0; i < selecteds.Count; ++i)
        {
            Node n = selecteds[i];
            if (n.action != null)
            {
                n.action.bullets.Remove(n as Bullet);
            }
            else
            {
                actions.Remove(n as Action);
            }
        }
        selecteds.Clear();
    }

    public static GameSkill create(string name)
    {
        if (skills.ContainsKey(name))return null;
        GameSkill gs = new GameSkill();
        gs.name = name;
        skills[name] = gs;
        reset(false);
        genNames();
        return gs;
    }

    public static void delete(string name)
    {
        skills.Remove(name);
        reset(false);
        genNames();
    }

    public static string[] names;
    public static void genNames()
    {
        List<string> ls = new List<string>();
        IDictionaryEnumerator e = skills.GetEnumerator();
        while(e.MoveNext())
        {
            ls.Add(e.Key as string);
        }
        names = ls.ToArray();
    }

    public static void reset(bool clearSkill)
    {
        if (clearSkill)
        {
            clear();
            genNames();
        }
        selecteds.Clear();
    }
}
#endif
