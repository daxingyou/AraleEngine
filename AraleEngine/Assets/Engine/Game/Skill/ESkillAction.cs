#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;

public partial class SkillAction : SkillNode
{
    public override void setAction(SkillAction a){}
    public void mergeTo(SkillAction a)
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
            GUILayout.BeginHorizontal();
            GUILayoutOption width = GUILayout.Width(32);
            state = EditorGUILayout.ToggleLeft("生", (state & UnitState.Alive) != 0, width) ? state | UnitState.Alive : state & (~UnitState.Alive);
            state = EditorGUILayout.ToggleLeft("移", (state & UnitState.Move) != 0, width) ? state | UnitState.Move : state & (~UnitState.Move);
            state = EditorGUILayout.ToggleLeft("动", (state & UnitState.Anim) != 0, width) ? state | UnitState.Anim : state & (~UnitState.Anim);
            state = EditorGUILayout.ToggleLeft("技", (state & UnitState.Skill) != 0, width) ? state | UnitState.Skill : state & (~UnitState.Skill);
            state = EditorGUILayout.ToggleLeft("显", (state & UnitState.Show) != 0, width) ? state | UnitState.Show : state & (~UnitState.Show);
            state = EditorGUILayout.ToggleLeft("伤", (state & UnitState.Harm) != 0, width) ? state | UnitState.Harm : state & (~UnitState.Harm);
            GUILayout.EndHorizontal();
            GUILayout.Space(16);
        }
        //子弹列表
        int delIdx = -1;
        for (int i = 0; i < bullets.Count; ++i)
        {
            if (!bullets[i].drawGUI())delIdx = i;
        }
        if (delIdx >= 0)
        {
            GameSkill.selecteds.Remove(bullets[delIdx]);
            bullets.RemoveAt(delIdx);
        }
        return true;
    }
}
#endif