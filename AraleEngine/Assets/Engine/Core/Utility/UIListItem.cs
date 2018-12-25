using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIListItem : MonoBehaviour , IPointerClickHandler {
    public Text _idx;
    public Text _data;
    public GameObject _del;
    public int _id;
    public void setData(int idx, object data)
    {
        _idx.text = idx.ToString();
        _data.text = ((int)data).ToString();
        _id = idx;
        name = idx.ToString();
    }
	// Use this for initialization
	void Start () {
	
	}

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        Debug.LogError("OnPointerClick="+_id);
    }
}
