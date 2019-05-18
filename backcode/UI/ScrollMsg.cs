using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Scripts.CoreScripts.GameLogic.Billboard;
using DG.Tweening;

public class ScrollMsg : MonoBehaviour
{
    public int   _msgType = 0; 
    public int   _interval= 5;
    public float _offsetY =100;
    public Text[] _text;
    int _cur;
    bool _hide;
	void Start ()
    {
        showNotice();
	}

    void showNotice()
    {
        string s = BillboardManager.Instance.GetCurMsg(_msgType);
        if (s == null)
        {
            _hide = true;
            Invoke("showNotice", 0.5f);
            return;
        }
        gameObject.SetActive(true);
        _hide = false;
        Text text = _text[_cur];
        text.text = s;
        Transform n = text.transform.parent; 
        Vector3 v = n.localPosition;
        v.y = -70;
        n.localPosition = v;
        n.DOLocalMoveY(0, 0.8f);
        n.GetComponent<CanvasGroup>().DOFade(1, 0.8f);
        n.DOLocalMoveY(_offsetY, 1f).SetDelay(_interval).OnStart(delegate() {
            n.GetComponent<CanvasGroup>().DOFade(0, 1f);
            Invoke("showNotice", 0.1f);
        }).OnComplete(delegate(){
            if(_hide)gameObject.SetActive(false);
        });
        _cur = ++_cur % _text.Length;
    }
}
