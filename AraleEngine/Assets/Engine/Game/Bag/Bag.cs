using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Arale.Engine;

public class Bag
{
	public class Item
	{
		public int  id;
		public uint count;
		public TBItem table;
		public object itemData;//实例数据
		public Item(int id)
		{
			this.id = id;
			table = TableMgr.single.GetData<TBItem>(id);
		}
		public virtual void use(uint num){}
	}

	public int bagSize = 32;
	public bool isFull{get{return mItems.Count >= bagSize;}}
	List<Item> mItems = new List<Item>();
	public Item addItem(int id, uint num)
	{
		Item it = getItem (id, true);
		if (it == null)return null;
		it.count += num;
		return it;
	}

	public Item DelItem(int id, uint num)
	{
		Item it = getItem (id);
		if (it == null)return null;
		it.count = it.count > num?it.count-num:0;
		return it;
	}

	public Item UseItem(int id, uint num)
	{
		Item it = getItem (id);
		if (it == null)return null;
		uint useNum = it.count > num?num:it.count;
		it.use (useNum);
		return it;
	}

	public Item SetItem(int id, uint num)
	{
		Item it = getItem (id, num > 0);
		if (it == null)return null;
		it.count = num;
		if (num == 0)mItems.Remove (it);
		return it;
	}

	public uint getItemCount(int id)
	{
		Item it = getItem (id, false);
		return it == null ? 0 : it.count;
	}

	public Item getItem(int id, bool create=false)
	{
		Item it = mItems.Find(delegate(Item o) {return o.id == id;});
		if(!create)return it;
		if (it != null)return it;
		if (isFull)return null;
		it = new Item (id);
		mItems.Add (it);
		return it;
	}

	public List<Item> getItems(int type=0)
	{
		if (type == 0)return mItems;
		List<Item> ls = new List<Item> ();
		for(int i=0,max=mItems.Count;i<max;++i)
		{
			Item it= mItems[i];
			if (it.table.type != type || it.count<=0)continue;
			ls.Add (it);
		}
		return ls;
	}
}