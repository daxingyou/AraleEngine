using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class SwitchButton : MonoBehaviour,IPointerClickHandler {
	public int _groupId;
	public int _userData;
	public GameObject _on;
	public GameObject _off;
	static List<SwitchButton> _switchs = new List<SwitchButton>();
	public static List<SwitchButton> getGroupSwitch(int groupId)
	{
		List<SwitchButton> ls = new List<SwitchButton> ();
		for (int i = 0, max = _switchs.Count; i < max; ++i)
		{
			SwitchButton sb = _switchs [i];
			if (sb._groupId != groupId)continue;
			ls.Add (sb);
		}
		return ls;
	}
	bool _isOn;
	public delegate void OnValueChange(SwitchButton sb);
	public OnValueChange onValueChange; 
	// Use this for initialization
	void Awake()
	{
		_switchs.Add (this);
	}

	void Start ()
	{
		updateState ();
	}

	void OnDestroy()
	{
		_switchs.Remove (this);
		onValueChange = null;
	}

	void updateState()
	{
		if (_on != null)_on.SetActive (_isOn);
		if (_off != null)_off.SetActive (!_isOn);
	}
	
	public bool isOn
	{
		set
		{
			if (_isOn == value)return;
			_isOn = value;
			updateState ();
			if(onValueChange!=null)onValueChange (this);
			if (_groupId==0||_isOn==false)return;

			for (int i = 0, max = _switchs.Count; i < max; ++i)
			{
				SwitchButton sb = _switchs [i];
				if (sb._groupId != _groupId)continue;
				if(object.ReferenceEquals(sb, this))continue;
				sb._isOn = false;
				sb.updateState ();
				if(sb.onValueChange!=null)sb.onValueChange (sb);
			}
		}
		get
		{
			return _isOn;
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		isOn = true;
	}
}
