#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;

public partial class SkillTarget : SkillNode
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

public partial class SkillVecctorTarget : SkillTarget
{
    public override bool drawGUI()
    {
        base.drawGUI();
        local  = EditorGUILayout.Toggle(local?"本地坐标":"世界坐标",local);
        vct = EditorGUILayout.Vector3Field(type==Type.Dir?"方向":"位置", vct);
        return true;
    }
}

public partial class SkillAreaTarget : SkillTarget
{
    public override bool drawGUI()
    {
        base.drawGUI();
        area = EditorGUILayout.TextField("区域", area);
        return true;
    }
}
#endif