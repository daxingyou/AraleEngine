using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Arale.Engine
{

    //挂list根节点上
    public class UISList : MonoBehaviour
    {
        List<UISListItem> mItems = new List<UISListItem>();
        UISListItem mPrefab;
		public UISListItem.OnSelectChange onSelectedChange=null;
        public bool mMultiSelect;
        void Awake()
        {
            mPrefab = GetComponentInChildren<UISListItem>();
            mPrefab.gameObject.SetActive(false);
        }

    	void Start () {
    	
    	}
    	
    	void Update () {
    	
    	}

        public List<UISListItem> getSelected()
        {
            List<UISListItem> sels = new List<UISListItem>();
            for (int i = 0,max=mItems.Count; i < max; ++i)
            {
                UISListItem it = mItems[i];
                if (it.selected)
                    sels.Add(it);
            }
            return sels;
        }

        public UISListItem getFirstSelected()
        {
            for (int i = 0,max=mItems.Count; i < max; ++i)
            {
                UISListItem it = mItems[i];
                if (it.selected)return it;
            }
            return null;
        }

        void onSelected(UISListItem sel)
        {
            if (mMultiSelect)
            {
                sel.selected = !sel.selected;
            }
            else
            {
                for (int i = 0,max=mItems.Count; i < max; ++i)
                {
                    UISListItem it = mItems[i];
                    if (!it.gameObject.activeSelf)
                        continue;
                    it.selected = false;
                }
                sel.selected = true;
            }
            if(null!=onSelectedChange)onSelectedChange(sel);
        }

        public UISListItem addItem(object data, bool sort=false)
        {
            UISListItem it=null;
            for (int i = mItems.Count-1; i>=0; --i)
            {
                if (!mItems[i].gameObject.activeSelf)
                {
                    it = mItems[i];
                    mItems.RemoveAt(i);
                    mItems.Insert(0, it);
                    break;
                }
            }

            if (it == null)
            {
                GameObject go = GameObject.Instantiate(mPrefab.gameObject) as GameObject;
                go.transform.SetParent(transform,false);
                it = go.GetComponent<UISListItem>();
                it.onSelectChange = onSelected;
                mItems.Add(it);
            }
            it.setData(data);
            it.gameObject.SetActive(true);
            return it;
        }

        public void delItem(object data)
        {
            for (int i = mItems.Count-1; i>=0; --i)
            {
                UISListItem it = mItems[i];
                if(it.isSame(data))
                {
                    it.gameObject.SetActive(false);
                    mItems.RemoveAt(i);
                    mItems.Add(it);
                    return;
                }
            }
        }

        public void clearItem()
        {
            for (int i = 0,max=mItems.Count; i < max; ++i)
            {
                UISListItem it = mItems[i];
                it.selected = false;
                it.gameObject.SetActive(false);
            }
        }
    }

}
