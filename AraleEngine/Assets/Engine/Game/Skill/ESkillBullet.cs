#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;

public partial class SkillBullet : SkillNode
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
            SkillTarget.Type t = (SkillTarget.Type)EditorGUILayout.EnumPopup("类型:", target.type);
            if (t != target.type)target = SkillTarget.newType(t);
            target.drawGUI();
        }
        return true;
    }
}
#endif