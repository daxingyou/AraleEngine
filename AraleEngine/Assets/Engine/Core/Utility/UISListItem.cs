using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Arale.Engine;

//list中的子item,配合UIList使用
public class UISListItem : LuaMono, IPointerClickHandler
{
    public delegate void OnSelectChange(UISListItem sel);
    public Image mOn;
    public bool selected
    {
        set
        {
			if (null != mOn)mOn.gameObject.SetActive (value);
        }
        get
        {
            return null==mOn?false:mOn.gameObject.activeSelf;       
        }
    }
    public OnSelectChange onSelectChange{ set; get;}
    public void OnPointerClick(PointerEventData eventData)
    {
        if (null != onSelectChange)onSelectChange(this);
    }

    protected object mData;
    public object data{get{return mData;}}
    // Use this for initialization
    void Start ()
    {
    }

    public virtual void setData(object data)
    {
        mData = data;
    }

    public virtual bool isSame(object data)
    {
        return false;
    }
}
