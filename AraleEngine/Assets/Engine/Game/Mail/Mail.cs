using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Mail
{
	public class Item : IData
	{
		public int id;      //以发送时间戳为ID
		public int senderID;//邮件发送者ID
		public string senderNick;//发送者昵称
		public int state;   //邮件状态0未读 1已读 2已领取
		public string title;//邮件标题
		public int[] reward;//奖励
		public string Json; //邮件扩展内容
		public Item(int id)
		{
			this.id = id;
		}
	}

	public int size = 64;
	public bool isFull{get{return mItems.Count >= size;}}
	List<Item> mItems = new List<Item>();
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

	public void dellItem(int id)
	{
		Item it = mItems.Find(delegate(Item o) {return o.id == id;});
		if(it!=null)mItems.Remove (it);
	}
}
