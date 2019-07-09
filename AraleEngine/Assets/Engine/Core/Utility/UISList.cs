using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Arale.Engine
{

    //挂list根节点上
    public class UISList : MonoBehaviour
    {
		public delegate void OnSelectChange(UISListItem sel);
		List<UISListItem> mCach  = new List<UISListItem>();
        List<UISListItem> mItems = new List<UISListItem>();
		public int count{get{return mItems.Count;}}
		public UISListItem this[int idx]{get{return mItems[idx];}}

        UISListItem mPrefab;
		public OnSelectChange onSelectedChange=null;
        public bool mMultiSelect;
        public bool mReverse;
        void Awake()
        {
            mPrefab = GetComponentInChildren<UISListItem>();
            mPrefab.gameObject.SetActive(false);
        }

    	void Start () {
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

		public UISListItem addItem(object data, int id=0)
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
            it.setData(data,id);
            it.gameObject.SetActive(true);
            if (mReverse)
                it.transform.SetAsFirstSibling();
            else
			    it.transform.SetAsLastSibling ();
            return it;
        }

		public UISListItem getItem(int id)
		{
			return mItems.Find (delegate(UISListItem it) {return it.id == id;});
		}

		public void delItem(UISListItem it)
        {
			if (!mItems.Remove (it))return;
			it.gameObject.SetActive(false);
			mCach.Add(it);
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

		public void sort(System.Comparison<UISListItem> comp)
        {
			mItems.Sort(comp);
            if (mReverse)
            {
                for (int i = 0,max=mItems.Count; i < max; ++i)mItems[i].transform.SetAsFirstSibling();
            }
            else
            {
                for (int i = 0,max=mItems.Count; i < max; ++i)mItems[i].transform.SetAsLastSibling();
            }
		}
    }

}
