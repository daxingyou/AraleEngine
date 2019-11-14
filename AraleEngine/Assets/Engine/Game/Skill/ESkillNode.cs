#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using Arale.Engine;
using UnityEditor;
using System.Collections.Generic;

public abstract partial class SkillNode : AraleSerizlize
{
    protected static Color[] markClrs = new Color[]{Color.cyan,Color.blue,Color.magenta,Color.green,Color.red};

    public SkillAction action{ get; protected set;}
    Rect rc = new Rect(0,0,12,12);
    protected void drawNode(Vector3 pos, Color fillClr)
    {
        rc.center = pos;
        bool sel = GameSkill.selecteds.Contains(this);
        Vector3[] vs = new Vector3[]{ new Vector3(pos.x-6,pos.y,0), new Vector3(pos.x,pos.y+6,0), new Vector3(pos.x+6,pos.y,0), new Vector3(pos.x,pos.y-6,0)};
        Handles.color = sel ? Color.red : fillClr;
        Handles.DrawSolidRectangleWithOutline(vs, Handles.color, Color.black);
        if (Event.current == null || Event.current.rawType != EventType.MouseDown || !rc.Contains(Event.current.mousePosition))
            return;

        if (!Event.current.control)
        {
            GameSkill.selecteds.Clear();
        }

        if (inSameAction())
        {
            if (sel)
                GameSkill.selecteds.Remove(this);
            else
                GameSkill.selecteds.Add(this);
        }   
    }
    public virtual void setAction(SkillAction a)
    {
        if (action != null)action.bullets.Remove(this as SkillBullet);
        action = a;
        a.bullets.Add(this as SkillBullet);
    }
    public virtual void draw(Vector3 pos){}
    public virtual bool drawGUI(){return true;}
    bool inSameAction()
    {
        if (GameSkill.selecteds.Count < 1)return true;
        SkillNode n = GameSkill.selecteds[0];
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
#endif
