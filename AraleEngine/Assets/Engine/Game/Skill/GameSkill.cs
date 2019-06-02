using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class GameSkill
{
    List<Action> actions = new List<Action>();
    public abstract class Node
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
            if (action != null)action.bullets.Remove(this);
            action = a;
            a.bullets.Add(this);
        }
        public abstract void draw(Vector3 pos);
        public abstract void drawGUI();
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
    }

    public class Action : Node
    {
        public float  time;
        public float  loopInterval=1;
        public int    loopTimes;
        public string anim="";
        public int    state=UnitState.ALL;
        public bool   breakable;
        public List<Node> bullets = new List<Node>();
        public virtual void setAction(Action a){}
        public void mergeTo(Action a)
        {
            for (int i = bullets.Count-1; i>=0; --i)bullets[i].setAction(a);
        }
        public override void draw(Vector3 pos)
        {
            drawNode(pos, Color.yellow);
            pos.y += 35;
            for (int i = 0; i<bullets.Count; ++i)
            {
                bullets[i].draw(pos);pos.y += 20;
            }
        }

        public override void drawGUI()
        {
            loopTimes = EditorGUILayout.IntField("循环次数(0不循环)", loopTimes);
            loopInterval = EditorGUILayout.FloatField("循环间隔(s))", loopInterval);
            breakable = EditorGUILayout.Toggle("打断", breakable);
            anim = EditorGUILayout.TextField("动画", anim);
            state = EditorGUILayout.Toggle("生",(state & UnitState.Alive)!=0)?state|UnitState.Alive:state&(~UnitState.Alive);
            state = EditorGUILayout.Toggle("移",(state & UnitState.Move)!=0)?state|UnitState.Move:state&(~UnitState.Move);
            state = EditorGUILayout.Toggle("动",(state & UnitState.Anim)!=0)?state|UnitState.Anim:state&(~UnitState.Anim);
            state = EditorGUILayout.Toggle("技",(state & UnitState.Skill)!=0)?state|UnitState.Skill:state&(~UnitState.Skill);
            state = EditorGUILayout.Toggle("显",(state & UnitState.Show)!=0)?state|UnitState.Show:state&(~UnitState.Show);
            state = EditorGUILayout.Toggle("伤",(state & UnitState.Harm)!=0)?state|UnitState.Harm:state&(~UnitState.Harm);
        }
    }

    public class Bullet : Node
    {
        public enum Mode
        {
            None,
            Scatter,//散射
            Chain,  //链式
        }

        public int id;
        public int harm;
        public int buffId;
        public int moveId;
        public Mode mode;
        public List<Target> targets=new List<Target>();
        public override void draw(Vector3 pos)
        {
            drawNode(pos, Color.green);
        }

        public override void drawGUI()
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
        }

        public void addTarget()
        {
        }

        public void removeTarget()
        {
        }
    }

    public class Target
    {
        enum Find
        {
            Pos,
            Dir,
            Unit,
            InArea,
            Nearst,
            MinHp,
            MaxHp,
            MinDF,
            MaxDF,
        }

        public int unitTypeMask;
        public int maxTarget=1;
        public string area;
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
    #if UNITY_EDITOR
    static List<Node> selecteds = new List<Node>();
    public static bool isDrag{get{return Event.current.alt;}}
    public static void drawGUI()
    {
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
        return act !=null?act:createAction(time);
    }

    public void draw(Rect rc, float unitWidth)
    {
        Color clr = Handles.color;
        Vector3 v = new Vector3(rc.xMin, rc.center.y,0);
        for (int i = 0; i < actions.Count; ++i)
        {
            Action act = actions[i];
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
                dragAction = a == null ? n.action : a;
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
                n.action.bullets.Remove(n);
            }
            else
            {
                actions.Remove(n as Action);
            }
        }
        selecteds.Clear();
    }
    #endif
}
