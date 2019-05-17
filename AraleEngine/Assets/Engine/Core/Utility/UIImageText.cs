using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using Arale.Engine;

public class UIImageText : Text, IPointerClickHandler{
    List<Image> cach = new List<Image>();
    public class Item
    {
        public int begin; //相对文本起始偏移
        public int end;   //结束字符偏移
        public int type;  //0文本1表情2文本链接3图片链接
        public string ctx;//内容
        public Vector2 pos;
        public Vector2 size;
        public Image   img;
        public Item(int begin, int end, int type, string ctx)
        {
            this.begin  = begin;
            this.end    = end;
            this.type   = type;
            this.ctx    = ctx;
            this.size   = type==2?Vector2.zero:new Vector2(32,32);
        }

        List<Rect> box;//点击区域
        public virtual bool isHit(Vector2 pos)
        {
            if (box == null)return false;
            for (int i = 0; i < box.Count; ++i)
            {
                if (box[i].Contains(pos))return true;
            }
            return false;
        }

        public Item setBox()
        {
            if (type < 2)return null;
            box = new List<Rect>(){new Rect(pos.x, pos.y-size.y/2, size.x, size.y)};
            return type==2?this:null;
        }

        public Item addTextLinkBox(int chr, float xmin, float ymin, float xmax, float ymax)
        {
            if (type != 2 || chr>end)return null;
            if (xmax < box[0].xMin)
            {
                box.Insert(0, new Rect(xmin, ymin, xmax - xmin, ymax - ymin));
            }
            else
            {
                Rect rc = box[0];
                rc.xMax = xmax;
                if(rc.yMax<ymax)rc.yMax = ymax;
                if(rc.yMin>ymin)rc.yMin = ymin;
                box[0] = rc;
            }
            return this;
        }
    }

    public int imageSize;
    List<Item> mItems = new List<Item>();
    float mHeight;
    public override float preferredHeight{ get{ return mHeight; } }

	// Use this for initialization
	void Start ()
    {
        imageSize = 32;
        //text = "123456789.1234567890http:\\www.arale.com123456789.1234567890";
        text = "⊕1a⊕0123456789.123456789012345⊕1a⊕06789012345⊕2http:\\www.arale.com";
        parseText();
        //字体大小必须用fontSize,而不是font.fontSize,因为前者是需要显示的大小，而后者是加载的字体大小
        this.font.RequestCharactersInTexture(m_Text, fontSize);
	}

    protected virtual void parseText()
    {
        mItems.Clear();
        string[] txs = text.Split(new char[]{'⊕'}, StringSplitOptions.RemoveEmptyEntries);
        text = "";
        for (int i = 0; i < txs.Length; ++i)
        {
            string s = txs[i];
            switch (s[0])
            {
                case '0':
                    text += s.Substring(1);
                    break;
                case '1':
                    mItems.Add(new Item(text.Length, 0, 1, s.Substring(1)));
                    break;
                case '2':
                    string ctx = s.Substring(1);
                    int begin = text.Length;
                    text += "<color=#0000ff>"+ctx+"</color>";
                    mItems.Add(new Item(begin, text.Length-1, 2, ctx));
                    break;
                case '3':
                    mItems.Add(new Item(text.Length, 0, 3, s.Substring(1)));
                    break; 
            }
        }
    }

    struct LineInfo
    {
        public int     ret;
        public Vector2 start;
        public LineInfo(int ret, Vector2 start)
        {
            this.ret   = ret;
            this.start = start;
        }
    };

    Vector2 getLineStart(float lineWidth, float lineHeight)
    {
        Rect rc = rectTransform.rect;
        switch (this.alignment)
        {   //用xMin,xMax,yMin,yMax计算原点可以正确处理pivot设置
            case TextAnchor.UpperLeft:
            case TextAnchor.MiddleLeft:
            case TextAnchor.LowerLeft:
                return new Vector2(rc.xMin, rc.yMax - mHeight - lineHeight/ 2);
            case TextAnchor.UpperCenter:
            case TextAnchor.MiddleCenter:
            case TextAnchor.LowerCenter:
                return new Vector2(rc.center.x - lineWidth/2, rc.yMax - mHeight - lineHeight/ 2);
            case TextAnchor.UpperRight:
            case TextAnchor.MiddleRight:
            case TextAnchor.LowerRight:
                return new Vector2(rc.xMax - lineWidth, rc.yMax - mHeight - lineHeight / 2);
        }
        return new Vector2(rc.xMin, rc.yMax - mHeight - lineHeight/ 2);
    }
	
    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        base.OnPopulateMesh(toFill);
        #if UNITY_EDITOR
        if (!Application.isPlaying)return;
        #endif
        IList<UICharInfo> chs = cachedTextGenerator.characters;
        float fk   = 1.0f * fontSize / font.fontSize;//显示字体高度与实际字体高度比值
        float unit = 1.0f / this.pixelsPerUnit;
        float textHight = lineSpacing*fontSize;
        //Debug.LogError("bein"+fontSize+","+font.fontSize+","+font.lineHeight+","+font.ascent+","+unit+","+fk);

        #region 构造行信息
        Rect rc = rectTransform.rect;
        float lineHight = textHight;
        mHeight = 0;
        float w = 0;
        List<LineInfo> lines = new List<LineInfo>();
        for (int i = 0, itemIdx = 0; i < m_Text.Length; ++i)
        {
            
            while (itemIdx < mItems.Count && mItems[itemIdx].begin <= i)
            {
                Item it = mItems[itemIdx++];
                if (w + it.size.x > rc.width)
                {//item转行
                    lines.Add(new LineInfo(i,getLineStart(w,lineHight)));
                    w = 0; mHeight += lineHight; lineHight = textHight;
                }
                w += it.size.x;
                if (lineHight < it.size.y)lineHight = it.size.y;
            }
            float charWidth = chs[i].charWidth * unit;
            if (w + charWidth > rc.width)
            {//text转行
                lines.Add(new LineInfo(i,getLineStart(w,lineHight)));
                w = 0; mHeight += lineHight; lineHight = textHight;
            }
            w += charWidth;
        }
        lines.Add(new LineInfo(m_Text.Length,getLineStart(w,lineHight)));
        mHeight += lineHight; lineHight = textHight;
        #endregion
       
        #region 逐行调整位置
        Item textLink = null;
        for (int i = 0, chr=0, itemIdx=0; i < lines.Count; ++i)
        {
            LineInfo li = lines[i];
            Vector2 pos = li.start;
            float charYOffset = textHight / 2 - font.ascent * fk;
            while (chr < li.ret)
            {
                while(itemIdx<mItems.Count && mItems[itemIdx].begin<=chr)
                {
                    Item it = mItems[itemIdx++];
                    it.pos = pos;
                    textLink = it.setBox();
                    pos.x += it.size.x;
                }

                CharacterInfo ci;
                this.font.GetCharacterInfo(m_Text[chr], out ci, fontSize);
                float charWidth = chs[chr].charWidth * unit;
                UIVertex pos0 = new UIVertex(); 
                UIVertex pos1 = new UIVertex(); 
                UIVertex pos2 = new UIVertex(); 
                UIVertex pos3 = new UIVertex(); 
                int vIdx = chr << 2;
                toFill.PopulateUIVertex(ref pos0, vIdx);  //左上点
                toFill.PopulateUIVertex(ref pos1, vIdx+1);//右上点
                toFill.PopulateUIVertex(ref pos2, vIdx+2);//右下点
                toFill.PopulateUIVertex(ref pos3, vIdx+3);//左下点
                float chw = pos1.position.x - pos0.position.x;
                float chh = pos1.position.y - pos2.position.y;
                float xCharoffset = chw > ci.glyphWidth ? -ci.bearing * unit : 0;
                //pos0.position = new Vector3(pos.x+ci.minX*unit, pos.y+ci.maxY*unit, 0);
                //pos1.position = new Vector3(pos.x+ci.maxX*unit, pos.y+ci.maxY*unit, 0);
                //pos2.position = new Vector3(pos.x+ci.maxX*unit, pos.y+ci.minY*unit, 0);
                //pos3.position = new Vector3(pos.x+ci.minX*unit, pos.y+ci.minY*unit, 0);
                pos0.position = new Vector3(pos.x+xCharoffset+ci.minX*unit,     pos.y+charYOffset+ci.minY*unit+chh, 0);
                pos1.position = new Vector3(pos.x+xCharoffset+ci.minX*unit+chw, pos.y+charYOffset+ci.minY*unit+chh, 0);
                pos2.position = new Vector3(pos.x+xCharoffset+ci.minX*unit+chw, pos.y+charYOffset+ci.minY*unit,     0);
                pos3.position = new Vector3(pos.x+xCharoffset+ci.minX*unit,     pos.y+charYOffset+ci.minY*unit,     0);
                toFill.SetUIVertex(pos0, vIdx); 
                toFill.SetUIVertex(pos1, vIdx+1);
                toFill.SetUIVertex(pos2, vIdx+2); 
                toFill.SetUIVertex(pos3, vIdx+3);
                pos.x += charWidth;
                if(textLink!=null)textLink = textLink.addTextLinkBox(chr,pos3.position.x, pos3.position.y, pos1.position.x, pos1.position.y);
                ++chr;
            }
        }
        #endregion
        updateItems = true;
        //Debug.LogError("end");
    }

    bool updateItems;
    void Update()
    {
        if (!updateItems)return;
        updateItems = false;
        Vector2 size = rectTransform.sizeDelta;
        size.y = mHeight;
        rectTransform.sizeDelta = size;
        for (int i = 0; i < mItems.Count; ++i)
        {
            showItem(mItems[i]);
        }
    }

    void showItem(Item item)
    {
        if (item.type==2)return;
        if (item.img == null)
        {
            item.img = allocImage();
        }
        Image img = item.img;
        RectTransform t = img.rectTransform;
        t.sizeDelta = new Vector2(imageSize, imageSize);
        if (AssetRef.setImage(img, "UI/Icon/Shop/jinbi") && imageSize <= 0)img.SetNativeSize();
        Vector3 lp = t.localPosition;
        lp.x = item.pos.x+t.sizeDelta.x/2;lp.y=item.pos.y;
        t.localPosition = lp;
    }

    Image allocImage()
    {
        Image img = null;
        if (cach.Count > 0)
        {
            img = cach[0];
            cach.RemoveAt(0);
            img.transform.SetParent(transform, false);
            img.gameObject.SetActive(true);
        }
        else
        {
            GameObject go = new GameObject("I");
            go.transform.SetParent(transform, false);
            img = go.AddComponent<Image>();
            img.preserveAspect = true;
        }
        return img;
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
        Debug.LogError("click="+item.ctx);
    }
}
