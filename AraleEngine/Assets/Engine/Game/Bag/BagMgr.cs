using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EquipItem : BagManager.Item
{
    uint mState;
    uint mType;
    uint mLV;
}

public class BagManager
{
    public class Item
    {
        public uint mTID;//配表ID
        public uint mIID;//实例ID
        public uint mNum;//数量
        public Item(){}

        public Item(uint tid, uint iid, uint num)
        {
            mTID = tid;
            mIID = iid;
            mNum = num;
        }

        public virtual void use()
        {
        }
    }


    int mBagSize;
    List<Item> mItems = new List<Item> ();
    public void useItem(uint tid, uint iid, uint num)
    {
        Item item = mItems.Find(delegate(Item it)
            {
                return it.mTID == tid && it.mIID == iid;
            });
        if(item==null)return;
        item.use();
    }

    public void addItem(uint tid, uint iid, uint num)
    {
        Item item = mItems.Find(delegate(Item it)
            {
                return it.mTID == tid && it.mIID == iid;
            });
        if (item == null)
        {
            mItems.Add(new Item(tid, iid, num));
        }
        else
        {
            item.mNum += num;
        }
    }
}