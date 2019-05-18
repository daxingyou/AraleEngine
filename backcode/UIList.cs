using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIList : MonoBehaviour
{
    #region 滑动窗口
    UIListItem[] mCach;
    int mCachSize;
    int mShowSize;
    int mBegin;
    int mCount;
    void initWindow(int cachSize, int showSize, UIListItem prefab)
    {
        Debug.LogError(""+cachSize+","+showSize);
        mCachSize = cachSize;
        mShowSize = showSize;
        mBegin = 0;
        mCach = new UIListItem[mCachSize];
    }

    UIListItem getCachItem(int idx)
    {
        int midx = idx % mCachSize;
        UIListItem it = mCach[midx];
        if (it == null)
        {
            it = GameObject.Instantiate<UIListItem>(prefab);
            it.transform.SetParent(transform, false);
            mCach[midx] = it;
        }
        it.gameObject.SetActive(true);
        return it;
    }

    void updateView(int begin)
    {
        mBegin = begin;
        for (int i = 0; i < mShowSize; ++i)
        {
            int idx = mBegin + i;
            UIListItem it = getCachItem(idx);
            if (idx >= datas.Count)
            {
                it.gameObject.SetActive(false);
            }
            else
            {
                it.setData(idx, datas[idx]);
                updateItemPos(idx, it.transform);
                it.gameObject.SetActive(true);
            }
        }
    }

    void updateItemPos(int idx, Transform it)
    {
        Vector3 v = it.localPosition;
        v.y = idx * ItemHeight;
        it.localPosition = v;
    }
    #endregion
    
    public UIListItem prefab;
    public float ItemHeight=1;
    float startY = 0;
    List<object> datas = new List<object>();
    RectTransform trans;
    void Awake()
    {
        trans = transform as RectTransform;
        Rect rc = (trans.parent as RectTransform).rect;
        int showCount = (int)Mathf.Abs((rc.height + Mathf.Abs(ItemHeight) - 0.1f) / ItemHeight);
        prefab.gameObject.SetActive(false);
        initWindow(10, showCount, prefab);
        trans = transform as RectTransform;
    }

    void resizeContent()
    {
        Vector2 sz =trans.sizeDelta;
        sz.y = Mathf.Abs(datas.Count * ItemHeight);
        trans.sizeDelta = sz;
    }

    void Start()
    {
        startY = trans.localPosition.y;
    }

    public void add(object data)
    {
        datas.Add(data);
        resizeContent();
        updateView(mBegin);
        Debug.LogError("add count=" + datas.Count);
    }

    public void del(object data)
    {
        int idx = datas.IndexOf(data);
        if (idx < 0)return;
        datas.RemoveAt(idx);
        resizeContent();
        focusItem(mBegin, getOffset(mBegin));
        Debug.LogError("del count=" + datas.Count);
    }

    public void del(int idx)
    {
        if (idx < 0||idx>=datas.Count)return;
        datas.RemoveAt(idx);
        resizeContent();
        focusItem(mBegin, getOffset(mBegin));
        Debug.LogError("del count=" + datas.Count);
    }

    public void clear()
    {
        datas.Clear();
        resizeContent();
        updateView(0);
    }

    public object getItem(int idx)
    {
        return datas[idx];
    }

    float getOffset(int idx)
    {
        Vector3 v = trans.localPosition;
        return (v.y - startY) - Mathf.Abs(mBegin*ItemHeight);
    }

    public void focusItem(int idx, float yoffset=0)
    {
        Debug.LogError("focusItem=" + idx);
        Vector3 v = trans.localPosition;
        v.y = (startY - idx * ItemHeight)+yoffset;
        trans.localPosition = v;
        updateView((int)Mathf.Abs((v.y - startY) / ItemHeight));
    }

    public void findItem()
    {
    }

    public void sortItem()
    {
    }

	// Update is called once per frame
	void Update ()
    {
        float yOffset = trans.localPosition.y - startY;
        updateView((int)Mathf.Abs(yOffset / ItemHeight));
	}

    public bool isVisible(Transform item)
    {
        Rect rc = (trans.parent as RectTransform).rect;
        if (item.localPosition.y + trans.localPosition.y < rc.yMin || item.localPosition.y + trans.localPosition.y > rc.yMax)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    #region 测试
    int TestIdx=0;
    void OnGUI()
    {
        float y = 0;
        if (GUI.Button(new Rect(0, 0, 100, 50), "Add"))
        {
            add(TestIdx++);
        }
        if (GUI.Button(new Rect(0,y+=50, 100, 50), "Del Idx=8"))
        {
            del(8);
        }
        if (GUI.Button(new Rect(0,y+=50, 100, 50), "Clear"))
        {
            TestIdx = 0;
            clear();
        }
    }
    #endregion
}
