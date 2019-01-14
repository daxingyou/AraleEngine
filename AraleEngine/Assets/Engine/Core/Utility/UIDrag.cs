//挂在拖拽物体根节点上，这样可以避免每个拖拽物体都绑定一个DragItem,影响性能
//需要拖动的物体上绑定脚本UIDragItem,需要接收拖动物体的槽上绑定脚本UIDragReceiver
//如果想拖动自身节点，也可以在自身节点上同时绑定UIDrag和UIDragItem
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;

public class UIDrag : MonoBehaviour,IDragHandler,IBeginDragHandler,IEndDragHandler{
	public delegate void OnDragReceived(UIDragItem item, UIDragReceiver receiver);
	public OnDragReceived onDragReceived;
	public bool mClone=true;
	public bool mFallback=true;
	UIDragItem mTarget;
	Vector2 mLocalPointerPos;
	Vector3 mLocalBeginlPos;
	RectTransform mTargetParent;
	void Start()
	{
		//添加CanvasGroup组件用于在拖拽时忽略拖拽物体自身,以便检测下层物体
		//mCanvasGroup = GetComponent<CanvasGroup>();
		//mCanvasGroup.blocksRaycasts = false;
		//Canvas已经启到相同效果
	}
		
	public void OnBeginDrag(PointerEventData eventData)
	{
		GameObject go = eventData.pointerEnter;
		if (go == null)return;
		Debug.LogError (go.name);
		//获取子节点,这样可以拖动UIDragReceiver内的UIDragItem,UIDragReceiver的子物体不能设置响应事件,否则检测不到UIDragReceiver物体
		UIDragItem di = go.GetComponentInChildren<UIDragItem> ();
		if (di == null || di.enabled == false)return;
		mLocalBeginlPos = di.transform.localPosition;
		mTarget = mClone ? GameObject.Instantiate<UIDragItem> (di) : di;
		if (mClone)mTarget.transform.SetParent (di.transform.parent, false);

		mTargetParent = mTarget.transform.parent as RectTransform;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(mTargetParent, eventData.position, eventData.pressEventCamera, out mLocalPointerPos);
		bringToTop ();
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (mTarget == null)return;
		Vector2 localPorinterPos;
		if(RectTransformUtility.ScreenPointToLocalPointInRectangle(mTargetParent, eventData.position, eventData.pressEventCamera, out localPorinterPos))
		{
			Vector3 t = localPorinterPos - mLocalPointerPos;
			mTarget.transform.localPosition = mLocalBeginlPos + t;
			ClampToScreen (eventData.pressEventCamera);
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		
		if (mTarget == null)return;
		GameObject go = eventData.pointerEnter;//鼠标下的物体
		if (go == null)return;
		UIDragReceiver dr = go.GetComponent<UIDragReceiver> ();
		if (dr == null || dr.enabled == false || (dr.recvMask&mTarget.typeMask)==0 || object.ReferenceEquals(mTarget.GetComponentInParent<UIDragReceiver> (),dr))
		{
			if(mFallback)StartCoroutine (Fallback (mTarget.transform, mLocalBeginlPos));
		}
		else
		{
			if (onDragReceived != null)onDragReceived (mTarget, dr);
			recoverDepth (mTarget.transform);
		}
		mTarget = null;
	}

	IEnumerator Fallback(Transform target, Vector3 backPos)
	{
		float t = 0;
		while (t < 0.3f)
		{
			t += Time.deltaTime;
			target.localPosition = Vector3.Lerp (target.localPosition, backPos, t / 0.3f);
			yield return null;
		}
		target.localPosition = backPos;
		recoverDepth (target);
		if (mClone)GameObject.Destroy (target.gameObject);
	}

	void recoverDepth(Transform target)
	{
		Canvas cav = target.gameObject.GetComponent<Canvas> ();
		if (cav != null)GameObject.Destroy (cav);
	}

	void bringToTop()
	{
		Canvas cav = mTarget.gameObject.AddComponent<Canvas> ();
		cav.overrideSorting = true;
		cav.sortingOrder = 100;
	}

	void ClampToScreen(Camera cam)
	{/*
		Vector2 max = new Vector2 (0.5f * 1920, 0.5f * 1080);
		Vector2 min = -max;
		Vector2 rmin, rmax;
		if (!RectTransformUtility.ScreenPointToLocalPointInRectangle (mTargetParent, min, cam, out rmin))
			return;
		if (!RectTransformUtility.ScreenPointToLocalPointInRectangle (mTargetParent, max, cam, out rmax))
			return;
		Vector3 v = mTarget.localPosition;
		v.x = Mathf.Clamp (v.x, min.x, max.x);
		v.y = Mathf.Clamp (v.y, min.y, max.y);
		mTarget.localPosition = v;*/
	}
}