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
        public override void setAction(GameSkill.Action a){}
        public void mergeTo(GameSkill.Action a)
        {
            for (int i = bullets.Count-1; i>=0; --i)bullets[i].setAction(a);
        }
        public override void draw(Vector3 pos)
        {
            drawNode(pos, Color.yellow);
            drawMark(0, loopTimes>0);
            drawMark(1, breakable);
            drawMark(2, end);
            drawMark(3, !string.IsNullOrEmpty(anim));
            drawMark(4, state!=UnitState.ALL);
            pos.y += 35;
            for (int i = 0; i<bullets.Count; ++i)
            {
                bullets[i].draw(pos);pos.y += 20;
            }
        }

        bool fold=true;
        public override bool drawGUI()
        {
            fold = EditorGUILayout.Foldout(fold,"行为属性");
            if (fold)
            {
                loopTimes = EditorGUILayout.IntField("循环次数", loopTimes);
                if (loopTimes > 0)loopInterval = EditorGUILayout.FloatField("循环间隔(s)", loopInterval);
                mask = EditorGUILayout.Toggle("结束", end) ? mask | 0x0001 : mask & ~0x0001;
                mask = EditorGUILayout.Toggle("可打断", breakable) ? mask | 0x0002 : mask & ~0x0002;
                mask = EditorGUILayout.Toggle("可取消", cancelable) ? mask | 0x0004 : mask & ~0x0004;
                anim = EditorGUILayout.TextField("动画", anim);
                state = EditorGUILayout.Toggle("生", (state & UnitState.Alive) != 0) ? state | UnitState.Alive : state & (~UnitState.Alive);
                state = EditorGUILayout.Toggle("移", (state & UnitState.Move) != 0) ? state | UnitState.Move : state & (~UnitState.Move);
                state = EditorGUILayout.Toggle("动", (state & UnitState.Anim) != 0) ? state | UnitState.Anim : state & (~UnitState.Anim);
                state = EditorGUILayout.Toggle("技", (state & UnitState.Skill) != 0) ? state | UnitState.Skill : state & (~UnitState.Skill);
                state = EditorGUILayout.Toggle("显", (state & UnitState.Show) != 0) ? state | UnitState.Show : state & (~UnitState.Show);
                state = EditorGUILayout.Toggle("伤", (state & UnitState.Harm) != 0) ? state | UnitState.Harm : state & (~UnitState.Harm);
            }
            //子弹列表
            int delIdx = -1;
            for (int i = 0; i < bullets.Count; ++i)
            {
                if (!bullets[i].drawGUI())delIdx = i;
            }
            if (delIdx >= 0)
            {
                selecteds.Remove(bullets[delIdx]);
                bullets.RemoveAt(delIdx);
            }
            return true;
        }
    }

    public partial class Bullet : Node
    {
        public override void draw(Vector3 pos)
        {
            drawNode(pos, Color.green);
        }
        bool fold=true;
        public override bool drawGUI()
        {
            GUILayout.BeginHorizontal();
            fold = EditorGUILayout.Foldout(fold,"子弹属性");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("X", GUI.skin.label))return false;
            GUILayout.EndHorizontal();
            if (fold)
            {
                id = EditorGUILayout.IntField("子弹ID", id);
                harm = EditorGUILayout.IntField("伤害", harm);
                buffId = EditorGUILayout.IntField("BuffID", buffId);
                moveId = EditorGUILayout.IntField("MoveID", moveId);
                mode = (Mode)EditorGUILayout.EnumPopup("模式", mode);
                GUILayout.Box("目标",GUILayout.ExpandWidth(true));//撑满
                Target.Type t = (Target.Type)EditorGUILayout.EnumPopup("类型:", target.type);
                if (t != target.type)target = Target.newType(t);
                target.drawGUI();
            }
            return true;
        }
    }

    public partial class Target : Node
    {
        public override bool drawGUI()
        {
            relation = (UnitRelation)EditorGUILayout.EnumMaskPopup(new GUIContent("目标关系"),relation);
            selector = (Selector)EditorGUILayout.EnumPopup("选择器",selector);
            if (type == Type.None)
            {
                noneType = (NoneType)EditorGUILayout.EnumPopup("指向", noneType);
            }
            return true;
        }
    }

    public partial class VecctorTarget : Target
    {
        public override bool drawGUI()
        {
            base.drawGUI();
            local  = EditorGUILayout.Toggle(local?"本地坐标":"世界坐标",local);
            vct = EditorGUILayout.Vector3Field(type==Type.Dir?"方向":"位置", vct);
            return true;
        }
    }

    public partial class AreaTarget : Target
    {
        public override bool drawGUI()
        {
            base.drawGUI();
            area = EditorGUILayout.TextField("区域", area);
            return true;
        }
    }

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
            initState = EditorGUILayout.Toggle("生", (initState & UnitState.Alive) != 0) ? initState | UnitState.Alive : initState & (~UnitState.Alive);
            initState = EditorGUILayout.Toggle("移", (initState & UnitState.Move) != 0) ? initState | UnitState.Move : initState & (~UnitState.Move);
            initState = EditorGUILayout.Toggle("动", (initState & UnitState.Anim) != 0) ? initState | UnitState.Anim : initState & (~UnitState.Anim);
            initState = EditorGUILayout.Toggle("技", (initState & UnitState.Skill) != 0) ? initState | UnitState.Skill : initState & (~UnitState.Skill);
            initState = EditorGUILayout.Toggle("显", (initState & UnitState.Show) != 0) ? initState | UnitState.Show : initState & (~UnitState.Show);
            initState = EditorGUILayout.Toggle("伤", (initState & UnitState.Harm) != 0) ? initState | UnitState.Harm : initState & (~UnitState.Harm);
        }
        //=======
        switch (GUILayout.Toolbar(-1, new string[]{"添加行为","添加子弹"}))
        {
            case 0:
                getAction(tickLine);
                break;
            case 1:
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
        act.state = initState;
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
