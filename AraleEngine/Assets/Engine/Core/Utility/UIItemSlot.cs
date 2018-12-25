using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Arale.Engine;

public class UIItemSlot : MonoBehaviour {
    public Image mIcon;
    public Text mName;
    public Text mNum;
    public void SetData(string icon, string name, int count)
    {
        AssetRef.setImage(mIcon, icon);
        mNum.text = count.ToString();   
        if (mName != null)mName.text = name.ToString();
    }
}
