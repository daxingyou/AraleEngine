using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;

public class UIImageText : Text, IPointerClickHandler{
    public class Item
    {
        int begin;
        int typeId;
        int id;
        string ctx;
        List<Rect> box;
        public virtual bool isHit(Vector2 pos)
        {
            if (box == null)return false;
            for (int i = 0; i < box.Count; ++i)
            {
                if (box[i].Contains(pos))return true;
            }
            return false;
        }
    }
    List<Item> mItems = new List<Item>();
	// Use this for initialization
	void Start () {
        this.font = Font.CreateDynamicFontFromOSFont(font.name, fontSize); 
	}

    protected virtual void parseText()
    {
        string[] txs = text.Split(new char[]{'⊕'}, StringSplitOptions.RemoveEmptyEntries);
    }
	
    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        base.OnPopulateMesh(toFill);
        this.font.RequestCharactersInTexture(m_Text, fontSize);
        Rect rc = rectTransform.rect;
        Debug.LogError("bein"+fontSize+","+font.fontSize);
        float unit = 1.0f / this.pixelsPerUnit;
        float lineHight = fontSize * unit * lineSpacing;
        Vector2 org = new Vector2(-rc.width / 2, rc.height / 2);
        float maxX = org.x + rc.width;
        CharacterInfo ci;
        for (int i = 0; i < m_Text.Length; ++i)
        {
            this.font.GetCharacterInfo(m_Text[i], out ci, fontSize, fontStyle);
            if (org.x + ci.advance * unit > maxX)
            {
                org.x = -rc.width / 2;
                org.y -= lineHight;
            }
            Debug.LogError(ci.advance);
            UIVertex pos0 = new UIVertex(); 
            UIVertex pos1 = new UIVertex(); 
            UIVertex pos2 = new UIVertex(); 
            UIVertex pos3 = new UIVertex(); 
            toFill.PopulateUIVertex(ref pos0, i*4); 
            toFill.PopulateUIVertex(ref pos1, i*4+1);
            toFill.PopulateUIVertex(ref pos2, i*4+2); 
            toFill.PopulateUIVertex(ref pos3, i*4+3);
            pos0.position = new Vector3(org.x+ci.minX*unit, org.y+ci.maxY*unit, 0);
            pos1.position = new Vector3(org.x+ci.maxX*unit, org.y+ci.maxY*unit, 0);
            pos2.position = new Vector3(org.x+ci.maxX*unit, org.y+ci.minY*unit, 0);
            pos3.position = new Vector3(org.x+ci.minX*unit, org.y+ci.minY*unit, 0);
            toFill.SetUIVertex(pos0, i*4); 
            toFill.SetUIVertex(pos1, i*4+1);
            toFill.SetUIVertex(pos2, i*4+2); 
            toFill.SetUIVertex(pos3, i*4+3);
            org.x += ci.advance * unit;

        }
        Debug.LogError("end");
    }

    public void OnPointerClick(PointerEventData ed)
    {
        Vector2 localPos;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, ed.position, ed.pressEventCamera, out localPos))return;
        for (int i = 0; i < mItems.Count; ++i)
        {
            if (mItems[i].isHit(localPos))
            {
                onItemClick(mItems[i]);
                break;
            }
        }
    }

    public virtual void onItemClick(Item item)
    {
    }
}
