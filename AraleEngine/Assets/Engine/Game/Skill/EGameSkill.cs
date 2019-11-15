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
    public static Color[] markClrs = new Color[]{Color.cyan,Color.blue,Color.magenta,Color.green,Color.red};
    public const float StateHeight=18;
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
        act.state = state;
        actions.Sort(delegate(SkillAction x, SkillAction y){return x.time.CompareTo(y.time);});
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
        int max = actions.Count;
        for (int i = 0; i < max; ++i)
        {
            SkillAction act = actions[i] as SkillAction;
            Vector3 offset = new Vector3(act.time * unitWidth, 0, 0);
            act.length = i < max - 1 ? actions[i + 1].time * unitWidth - offset.x : 1000-offset.x+StateHeight;
            act.draw(v + offset);
        }
        Handles.color = clr;
        v.x -= StateHeight;
        state = drawStates(ref v, max>0?actions[0].time * unitWidth+StateHeight:1000+2*StateHeight, state);
    }

    public static int drawStates(ref Vector3 pos, float length, int state)
    {
        pos.y += 25;
        int i = 0;
        state = drawState(ref pos, length, i++, (state & UnitState.Alive) != 0) ? state | UnitState.Alive : state & (~UnitState.Alive);
        state = drawState(ref pos, length, i++, (state & UnitState.Move) != 0) ? state | UnitState.Move : state & (~UnitState.Move);
        state = drawState(ref pos, length, i++, (state & UnitState.Anim) != 0) ? state | UnitState.Anim : state & (~UnitState.Anim);
        state = drawState(ref pos, length, i++, (state & UnitState.Skill) != 0) ? state | UnitState.Skill : state & (~UnitState.Skill);
        state = drawState(ref pos, length, i++, (state & UnitState.Show) != 0) ? state | UnitState.Show : state & (~UnitState.Show);
        state = drawState(ref pos, length, i++, (state & UnitState.Harm) != 0) ? state | UnitState.Harm : state & (~UnitState.Harm);
        return state;
    }

    static bool drawState(ref Vector3 pos, float length, int idx, bool light)
    {
        Rect rc = new Rect(pos.x, pos.y, length, StateHeight);pos.y += StateHeight;
        Color clr = markClrs[idx%markClrs.Length];
        Handles.color = clr;
        Handles.DrawSolidRectangleWithOutline(rc, light?clr:Color.clear, Color.black);
        if (Event.current == null || Event.current.rawType != EventType.MouseDown || !rc.Contains(Event.current.mousePosition))return light;
        return !light;
    }

    public static void drawMark(Vector3 pos, int idx, bool light)
    {
        if (!light)return;
        pos.y += idx * 5+8;
        Vector3[] vs = new Vector3[]{ new Vector3(pos.x-2,pos.y-2,0), new Vector3(pos.x+2,pos.y-2,0), new Vector3(pos.x+2,pos.y+2,0), new Vector3(pos.x-2,pos.y+2,0)};
        Color clr = markClrs[idx];
        Handles.color = clr;
        Handles.DrawSolidRectangleWithOutline(vs, clr, Color.black);
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
