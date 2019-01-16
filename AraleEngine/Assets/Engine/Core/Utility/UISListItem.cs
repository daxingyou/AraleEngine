using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Arale.Engine;

//list中的子item,配合UIList使用
public class UISListItem : LuaMono, IPointerClickHandler
{
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
	public UISList.OnSelectChange onSelectChange{ set; get;}
    public void OnPointerClick(PointerEventData eventData)
    {
        if (null != onSelectChange)onSelectChange(this);
    }

    protected object mData;
	public object data{get{return mData;}}
	protected int    mID;
	public int id{get{return mID;}}
	public virtual void setData(object data,int id)
    {
		mID = id;
        mData = data;
    }
}
