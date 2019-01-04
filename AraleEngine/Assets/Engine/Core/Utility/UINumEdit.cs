//实现购买数量控件,左右按钮点击+-,长按快速+-,直接数字输入编辑 
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Arale.Engine;
using UnityEngine.EventSystems;

public class UINumEdit : MonoBehaviour {
    public InputField _buyNum;
    public GameObject _addBt;
    public GameObject _subBt;
    public GameObject _maxBt;
    public uint _min=0;
    public uint _max=1000000;
    uint _itemCount=1;
    float _pressTime=0;
    void Start()
    {
        _buyNum.text = _itemCount.ToString();
        EventListener.Get (_maxBt).onClick = OnMaxClick;
		EventListener.Get (_addBt).onClick = OnAddClick;
		EventListener.Get (_subBt).onClick = OnSubClick;
		EventListener.Get (_addBt).onPointDown  = OnAddDown;
		EventListener.Get (_addBt).onPointerUp  = OnAddUp;
		EventListener.Get (_addBt).onPointerExit= OnAddExit;
		EventListener.Get (_subBt).onPointDown  = OnSubDown;
		EventListener.Get (_subBt).onPointerUp  = OnSubUp;
		EventListener.Get (_subBt).onPointerExit= OnSubExit;
        _buyNum.onEndEdit.AddListener(OnTextFieldChangeEnd);
    }

    void updatePrice()
    {
        //跟新UI
    }

    void showEdit(bool show)
    {
        _buyNum.transform.parent.gameObject.SetActive(show);
    }

    private void OnMaxClick(BaseEventData eventData)
    {
        _itemCount = _max;
        _buyNum.text = _itemCount.ToString();
        updatePrice();
    }

    private void OnAddClick(BaseEventData eventData)
    {
        if (_itemCount >= _max)
            return;
        ++_itemCount;
        _buyNum.text = _itemCount.ToString();
        updatePrice();
    }

    private void OnSubClick(BaseEventData eventData)
    {
        if (_itemCount <= _min)
            return;
        --_itemCount;
        _buyNum.text = _itemCount.ToString();
        updatePrice();
    }

    private void OnAddDown(BaseEventData eventData)
    {
        _pressTime = Time.realtimeSinceStartup;
        CancelInvoke ("subHold");
        InvokeRepeating ("addHold", 0.5f, 0.1f);
    }

    private void OnAddUp(BaseEventData eventData)
    {
        CancelInvoke ("addHold");
    }

    private void OnAddExit(BaseEventData eventData)
    {
        CancelInvoke ("addHold");
    }

    private void OnSubDown(BaseEventData eventData)
    {
        _pressTime = Time.realtimeSinceStartup;
        CancelInvoke ("addHold");
        InvokeRepeating ("subHold", 0.5f, 0.1f);
    }

    private void OnSubUp(BaseEventData eventData)
    {
        CancelInvoke ("subHold");
    }

    private void OnSubExit(BaseEventData eventData)
    {
        CancelInvoke ("subHold");
    }

    uint getAddValue(float t)
    {
        t = t-_pressTime;
        if (t < 5)
            return 1;
        else if (t < 10)
            return 5;
        else
            return 10;
    }

    void addHold()
    {
        if (_itemCount >= _max)return;
        uint val = getAddValue (Time.realtimeSinceStartup);
        _itemCount+=val;
        if (_itemCount > _max)_itemCount = _max;
        _buyNum.text = _itemCount.ToString();
        updatePrice();
    }

    void subHold()
    {
        if (_itemCount <= _min)return;
        uint val = getAddValue (Time.realtimeSinceStartup);
        if (_itemCount > val)
            _itemCount -= val;
        else
            _itemCount = 0;
        if (_itemCount < _min)_itemCount = _min;
        _buyNum.text = _itemCount.ToString();
        updatePrice();
    }

    public void OnTextFieldChangeEnd(string s)
    {
        if (!uint.TryParse (_buyNum.text, out _itemCount))_itemCount = _min;
        if (_itemCount < _min)
            _itemCount = _min;
        if (_itemCount > _max)
            _itemCount = _max;
        _buyNum.text = _itemCount.ToString();
        updatePrice();
    }
}
