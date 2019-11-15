#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;

public partial class SkillAction : SkillNode
{
    public float length;
    public override void setAction(SkillAction a){}
    public void mergeTo(SkillAction a)
    {
        for (int i = bullets.Count-1; i>=0; --i)bullets[i].setAction(a);
    }
    public override void draw(Vector3 pos)
    {
        drawNode(pos, Color.yellow);
        GameSkill.drawMark(pos, 0, end);
        GameSkill.drawMark(pos, 1, loopTimes > 0);
        state = GameSkill.drawStates(ref pos,length,state);
        pos.y += 25;
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
            anim = EditorGUILayout.TextField("动画", anim);
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