#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using Arale.Engine;
using System;

public partial class GameSkill : AraleSerizlize
{
    public static List<SkillNode> selecteds = new List<SkillNode>();
    public static bool isDrag{get{return Event.current.alt;}}
    bool fold=true;
    public void drawGUI(float tickLine)
    {
        //=======
        fold = EditorGUILayout.Foldout(fold,"skill属性");
        if (fold)
        {
            string newName = EditorGUILayout.TextField("技能名称", name);
            if (!skills.ContainsKey(newName))
            {
                skills.Remove(name);
                name = newName;
                skills[name] = this;
                genNames();
            }
            initAnim = EditorGUILayout.TextField("动画", initAnim);
            GUILayout.BeginHorizontal();
            GUILayoutOption width = GUILayout.Width(32);
            initState = EditorGUILayout.ToggleLeft("生", (initState & UnitState.Alive) != 0, width) ? initState | UnitState.Alive : initState & (~UnitState.Alive);
            initState = EditorGUILayout.ToggleLeft("移", (initState & UnitState.Move) != 0, width) ? initState | UnitState.Move : initState & (~UnitState.Move);
            initState = EditorGUILayout.ToggleLeft("动", (initState & UnitState.Anim) != 0, width) ? initState | UnitState.Anim : initState & (~UnitState.Anim);
            initState = EditorGUILayout.ToggleLeft("技", (initState & UnitState.Skill) != 0, width) ? initState | UnitState.Skill : initState & (~UnitState.Skill);
            initState = EditorGUILayout.ToggleLeft("显", (initState & UnitState.Show) != 0, width) ? initState | UnitState.Show : initState & (~UnitState.Show);
            initState = EditorGUILayout.ToggleLeft("伤", (initState & UnitState.Harm) != 0, width) ? initState | UnitState.Harm : initState & (~UnitState.Harm);
            GUILayout.EndHorizontal();
        }
        //=======
        switch (GUILayout.Toolbar(-1, new string[]{"添加行为","添加子弹"}))
        {
            case 0:
                getAction(tickLine);
                break;
            case 1:
                new SkillBullet().setAction(getAction(tickLine));
                break;
        }

        if (selecteds.Count < 1)return;
        SkillNode n = selecteds[selecteds.Count - 1];
        n.drawGUI();
    }

    SkillAction createAction(float time)
    {
        SkillAction act = new SkillAction();
        actions.Add(act);
        act.time = time;
        act.state = initState;
        return act;
    }

    public SkillAction getAction(float time)
    {
        SkillAction act = actions.Find(delegate(SkillAction a){return a.time == time;});
        return act !=null?act as SkillAction:createAction(time);
    }

    public void draw(Rect rc, float unitWidth)
    {
        Color clr = Handles.color;
        Vector3 v = new Vector3(rc.xMin, rc.center.y,0);
        for (int i = 0; i < actions.Count; ++i)
        {
            SkillAction act = actions[i] as SkillAction;
            act.draw(v + new Vector3(act.time * unitWidth, 0, 0));
        }
        Handles.color = clr;
    }

    SkillAction dragAction;
    public void drag(float timeLine, bool draging)
    {
        if (selecteds.Count < 1)return;
        if (draging && isDrag)
        {
            if (dragAction == null)
            {
                SkillNode n = selecteds[0];
                SkillAction a = n as SkillAction;
                dragAction = a == null ? n.action as SkillAction : a;
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
            SkillAction act = actions.Find(delegate(SkillAction b){return b.time == dragAction.time&&!object.ReferenceEquals(b,dragAction);});
            if (act != null)
            {
                dragAction.mergeTo(act);
                actions.Remove(dragAction);
                selecteds.Remove(dragAction);
            }
            dragAction = null;
        }
    }

    public bool hasEndAction()
    {
        for (int i = 0; i < actions.Count; ++i)
        {
            if (actions[i].end)return true; 
        }
        return false;
    }

    public void deleteSelected()
    {
        for (int i = 0; i < selecteds.Count; ++i)
        {
            SkillNode n = selecteds[i];
            if (n.action != null)
            {
                n.action.bullets.Remove(n as SkillBullet);
            }
            else
            {
                actions.Remove(n as SkillAction);
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
