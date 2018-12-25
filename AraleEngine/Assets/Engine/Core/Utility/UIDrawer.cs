using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.EventSystems;

//脚本绑定在抽屉按钮上
public class UIDrawer : MonoBehaviour , IPointerClickHandler{
    public Transform _content;
    public Vector3   _offset;
    public Vector2   _cellSize;
    public float     _speed=0.5f;
    public bool      _autoExpand;
    bool _expand;
    bool _tweening;
    bool _needAdjust;
    CanvasGroup _group;
	// Use this for initialization
	void Start ()
    {
        _group = _content.GetComponent<CanvasGroup>();
        int count = _content.childCount;
        for (int i = 0; i < count; ++i)
        {
            Transform c = _content.GetChild(i);
            c.localPosition = Vector3.zero;
            c.localScale    = Vector3.zero;
        }

        if (_autoExpand)Expand();
	}

   void Expand()
    {
        if (_expand)return;
        _expand = true;
        _tweening = true;
        Sequence seq = DOTween.Sequence ();
        int count = _content.childCount;
        int n = 0;
        for (int i = 0; i < count; ++i)
        {
            Transform c = _content.GetChild(i);
            if (!c.gameObject.activeSelf)continue;
            Vector3 v = _offset;
            v.y += n*_cellSize.y;
            v.x += n*_cellSize.x;
            seq.Insert(0, c.transform.DOLocalMove(v, _speed).OnStart(delegate
                {
                    c.localScale = Vector3.one;
                }).SetEase(Ease.OutBack));
            ++n;
        }
        if(_group!=null)seq.Insert(0, _group.DOFade(1f, 0.1f));
        seq.AppendCallback(delegate
            {
                _tweening = false;
                _needAdjust=true;
            });
        seq.Play();
    }

    void Contrace()
    {
        if (!_expand)return;
        _expand = false;
        _tweening = true;
        Sequence seq = DOTween.Sequence ();
        int count = _content.childCount;
        for (int i = 0; i < count; ++i)
        {
            Transform c = _content.GetChild(i);
            seq.Insert(0, c.DOLocalMove(Vector3.zero, _speed).OnComplete(delegate
                    {
                        c.localScale = Vector3.zero;
                    }));
        }
        if(_group!=null)seq.Insert(0.4f, _group.DOFade(0f, 0.1f));
        seq.AppendCallback(delegate
            {
               _tweening = false;
            });
        seq.Play();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_tweening)return;
        if (_expand)
            Contrace();
        else
            Expand();
    }

    void Update ()
    {
        if (_tweening || !_needAdjust || !_expand)return;
        int count = _content.childCount;
        int n = 0;
        for (int i = 0; i < count; ++i)
        {
            Transform c = _content.GetChild(i);
            if (!c.gameObject.activeSelf)continue;
            Vector3 v = _offset;
            v.y += n*_cellSize.y;
            v.x += n*_cellSize.x;
            c.localPosition = v;
            c.localScale = Vector3.one;
            ++n;
        }
        _needAdjust = false;
    }

    void OnEnable()
    {
        _content.gameObject.SetActive(true);
    }

    void OnDisable()
    {
        _content.gameObject.SetActive(false);
    }

    public bool dirty{
        set{_needAdjust = true;}
    }
}
