using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

public class UIIndicator : MonoBehaviour,IPointerEnterHandler,IDragHandler,IEndDragHandler
{
    public float radius=100;
    RectTransform rc;
    void Start()
    {
        Debug.Assert(radius > 0);
        rc = transform as RectTransform;
        rc.sizeDelta = new Vector2(2 * radius, 2 * radius);
    }
    //dir是方向向量,disPercent是鼠标距离中心点距离除radius,end是否操作结束
    public delegate void OnIndicator(Vector2 dir, float disPercent, bool end);
    public OnIndicator onEvent;
    public void OnPointerEnter(PointerEventData eventData)
    {//Event will sendto then PointerDown gameObject,so change then event receiver
        eventData.dragging=true;
        eventData.pointerDrag = gameObject;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (null==onEvent)return;
        Vector2 localPos;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rc, eventData.position, eventData.pressEventCamera, out localPos))return;
        if (null==onEvent)return;
        float disPercent = localPos.magnitude/radius;
        onEvent(localPos.normalized, disPercent>1?1:disPercent, false);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        gameObject.SetActive(false);
        if (null==onEvent)return;
        Vector2 localPos;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rc, eventData.position, eventData.pressEventCamera, out localPos))return;
        float disPercent = localPos.magnitude/radius;
        onEvent(localPos.normalized, disPercent>1?1:disPercent, true);
    }
}
