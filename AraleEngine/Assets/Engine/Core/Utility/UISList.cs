using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Arale.Engine
{

    //挂list根节点上
    public class UISList : MonoBehaviour
    {
		List<UISListItem> mCach  = new List<UISListItem>();
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
					it.selected = object.ReferenceEquals (it, sel);
                }
            }
            if(null!=onSelectedChange)onSelectedChange(sel);
        }

        public UISListItem addItem(object data, bool sort=false)
        {
            UISListItem it=null;
			if (mCach.Count > 0)
			{
				it = mCach[0];
				mCach.RemoveAt (0);
				mItems.Add (it);
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
			it.transform.SetAsLastSibling ();
            return it;
        }

        public void delItem(object data)
        {
            for (int i = mItems.Count-1; i>=0; --i)
            {
                UISListItem it = mItems[i];
				if (!it.isSame (data))continue;
                it.gameObject.SetActive(false);
                mItems.RemoveAt(i);
				mCach.Add(it);
                return;
            }
        }

		public UISListItem getItem(int idx)
		{
			return mItems[idx];
		}

		public UISListItem getItem(object data)
		{
			for (int i = 0,max=mItems.Count; i < max; ++i)
			{
				UISListItem it = mItems[i];
				if (it.isSame (data))return it;
			}
			return null;
		}

        public void clearItem()
        {
            for (int i = 0,max=mItems.Count; i < max; ++i)
            {
                UISListItem it = mItems[i];
                it.selected = false;
                it.gameObject.SetActive(false);
            }
			mCach.AddRange (mItems);
			mItems.Clear ();
        }
    }

}
