using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.EventSystems;

//脚本绑定在抽屉按钮上,_contents节点与DrawerButton为兄弟节点
public class UIDrawer : MonoBehaviour , IPointerClickHandler{
    public Transform[] _contents;//如果设置1个以上，则为切换型抽屉
    public Vector3   _offset;
    public Vector2   _cellSize;
    public float     _speed=0.5f;
    public bool      _autoExpand;
    public float     _autoContraceTime;
    bool _expand;
    bool _tweening;
    bool _needAdjust;
    Transform _content;
    CanvasGroup _group;
    int _flipIdx;
	// Use this for initialization
    void Awake()
    {
        _content = _contents[_flipIdx];
    }

	void Start ()
    {
        _group = _content.GetComponent<CanvasGroup>();
        for (int m = 0; m < _contents.Length; ++m)
        {
            Transform c = _contents[m];
            int count = c.childCount;
            for (int i = 0; i < count; ++i)
            {
                Transform t = c.GetChild(i);
                t.localPosition = Vector3.zero;
                t.localScale = Vector3.zero;
            }
        }

        if (_autoExpand)
        {
            Expand();
            if(_autoContraceTime>0)Invoke("Contrace", _autoContraceTime);
        }
	}

   public void Expand()
    {
        if (_expand || _tweening)return;
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

    public void Contrace()
    {
        if (!_expand || _tweening)return;
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
               if(_contents.Length>1)
                {//可以切换显示的抽屉
                    _flipIdx=++_flipIdx%_contents.Length;
                    _content = _contents[_flipIdx];
                    _content.gameObject.SetActive(true);
                    Expand();
                }
            });
        seq.Play();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        CancelInvoke("Contrace");
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
