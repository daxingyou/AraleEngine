using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Arale.Engine;

public class PlayerHeader : MonoBehaviour {
	public Text mNick;
	public Text mLevel;
	public Image mIcon;
	public void SetData(string nick, string head, int lv)
	{
		mNick.text = nick;
		mLevel.text = lv.ToString ();
		AssetRef.setImage (mIcon, head);
	}
}
