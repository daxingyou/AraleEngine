using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

public class DragItem : MonoBehaviour,IPointerDownHandler,IDragHandler,IBeginDragHandler,IEndDragHandler{
	public bool mBringToTop=false;
	Vector2 mLocalPointerPos;
	Vector3 mLocalPanelPos;
	public RectTransform mTargetObject;
	RectTransform mParentRectTransform;
	RectTransform mTargetRectTransform;
	CanvasGroup mCanvasGroup;
	void Start()
	{
		if (mTargetObject == null)
			mTargetObject = transform as RectTransform;
		mCanvasGroup = GetComponent<CanvasGroup>();
		if (!mCanvasGroup)
			mCanvasGroup = gameObject.AddComponent<CanvasGroup>();
		mParentRectTransform = mTargetObject.parent as RectTransform;
		mTargetRectTransform = mTargetObject as RectTransform;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		mLocalPanelPos = mTargetRectTransform.localPosition;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(mParentRectTransform, eventData.position, eventData.pressEventCamera, out mLocalPointerPos);
		if(mBringToTop)//拖拽时置顶层显示
			mTargetObject.gameObject.transform.SetAsLastSibling();
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		mCanvasGroup.blocksRaycasts = false;
	}

	public void OnDrag(PointerEventData eventData)
	{
		Vector2 localPorinterPos;
		if(RectTransformUtility.ScreenPointToLocalPointInRectangle(mParentRectTransform, eventData.position, eventData.pressEventCamera, out localPorinterPos))
		{
			Vector3 t = localPorinterPos - mLocalPointerPos;
			mTargetObject.localPosition = mLocalPanelPos + t;
			ClampToScreen (eventData.pressEventCamera);
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		mCanvasGroup.blocksRaycasts = true;
	}

	void ClampToScreen(Camera cam)
	{

		Vector2 max = new Vector2 (0.5f * 1920, 0.5f * 1080);
		Vector2 min = -max;
		Vector2 rmin, rmax;
		if (!RectTransformUtility.ScreenPointToLocalPointInRectangle (mParentRectTransform, min, cam, out rmin))
			return;
		if (!RectTransformUtility.ScreenPointToLocalPointInRectangle (mParentRectTransform, max, cam, out rmax))
			return;
		Vector3 v = mTargetObject.localPosition;
		v.x = Mathf.Clamp (v.x, min.x, max.x);
		v.y = Mathf.Clamp (v.y, min.y, max.y);
		mTargetObject.localPosition = v;
	}
}