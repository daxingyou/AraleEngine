using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Scripts.CoreScripts.Core;
using Scripts.BehaviourScripts;

public class ItemSlot : MonoBehaviour {
	public bool _omit;
	public Image _icon;
	public Text _itemNum;
	public void SetInfo(string icon, int num)
	{
		if (_omit) {
			if (num < 1000000) {
				_itemNum.text = string.Format ("{0:D}", num);
				_itemNum.fontSize = 36;
			} else {
				_itemNum.text = string.Format ("{0:D}万", num / 10000);
				_itemNum.fontSize = 32;
			}

		} else {
			_itemNum.text = string.Format ("{0:D}", num);
		}

		AssetRef.SetImage(_icon, icon);
	}

	public void SetInfo(int itemId, int itemNum)
	{
		GameDataCfg.Item item = GameDataCfg.Instance.getItemById(itemId);
		if (item == null)return;
		SetInfo (item._small_icon, itemNum);
	}
}
