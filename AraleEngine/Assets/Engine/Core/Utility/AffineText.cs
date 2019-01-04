//斜切变换Text
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
[ExecuteInEditMode]
#endif
public class AffineText: Text
{
    public float ang =30f;
    protected override void OnPopulateMesh(VertexHelper toFill) 
    {
        base.OnPopulateMesh(toFill);
        int vCount = toFill.currentVertCount;
        //文本的每个字符是一个正方形网格且高度与字符有关
        //每个字符框的y坐标都是相对中心的局部坐标
        int charCount = vCount / 4;
        float r = Mathf.Tan(Mathf.Deg2Rad*ang);
        for (int i = 0; i < charCount; ++i)
        {
            UIVertex pos0 = new UIVertex(); 
            UIVertex pos1 = new UIVertex(); 
            UIVertex pos2 = new UIVertex(); 
            UIVertex pos3 = new UIVertex(); 
            toFill.PopulateUIVertex(ref pos0, i*4); 
            toFill.PopulateUIVertex(ref pos1, i*4+1);
            toFill.PopulateUIVertex(ref pos2, i*4+2); 
            toFill.PopulateUIVertex(ref pos3, i*4+3);
            //Debug.LogError("h="+h+",y"+pos0.position.y+",y"+pos2.position.y);
            pos0.position+=new Vector3(r*pos0.position.y, 0, 0);
            pos1.position+=new Vector3(r*pos1.position.y, 0, 0);
            pos2.position+=new Vector3(r*pos2.position.y, 0, 0);
            pos3.position+=new Vector3(r*pos3.position.y, 0, 0);
            toFill.SetUIVertex(pos0, i*4); 
            toFill.SetUIVertex(pos1, i*4+1);
            toFill.SetUIVertex(pos2, i*4+2); 
            toFill.SetUIVertex(pos3, i*4+3);
        }
    } 
}