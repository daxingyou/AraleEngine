using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Arale.Engine
{
        
	public class UIStick : MonoBehaviour,IDragHandler,IBeginDragHandler,IEndDragHandler,IPointerDownHandler
    {
		public static bool isHit;
    	[System.NonSerialized]public Vector2 mDir;
    	Vector2 mBeginDragPos;
    	public Image mStick;
    	public Image mStickBack;
    	public float mRadius=30f;
    	void Start()
    	{
    		mStick.gameObject.SetActive (false);
    		mStickBack.gameObject.SetActive (false);
    		//image用于接收事件,该区域内才能处理stick
    		Image i = gameObject.AddComponent<Image> ();
    		i.color = new Color (0, 0, 0, 0);
    	}

    	void FixedUpdate()
    	{
			isHit = false;
            if (mStick.gameObject.activeSelf)
                return;

    		if (Input.GetKey (KeyCode.A))
    			mDir = Vector2.left;
    		else if (Input.GetKey (KeyCode.S))
    			mDir = Vector2.down;
    		else if (Input.GetKey (KeyCode.D))
    			mDir = Vector2.right;
    		else if (Input.GetKey (KeyCode.W))
    			mDir = Vector2.up;
            else
                mDir = Vector2.zero;
    	}

		public void OnPointerDown (PointerEventData eventData)
		{
			isHit = true;
		}

    	public void OnBeginDrag (PointerEventData eventData)
    	{
    		mDir = Vector2.zero;
    		mStick.gameObject.SetActive (true);
    		mStickBack.gameObject.SetActive (true);
    		RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, eventData.position, eventData.pressEventCamera, out mBeginDragPos);
    		Vector3 v = mStickBack.transform.localPosition;
    		v.x = mBeginDragPos.x;
    		v.y = mBeginDragPos.y;
    		mStick.transform.localPosition = mStickBack.transform.localPosition = v;
    	}

    	public void OnEndDrag (PointerEventData eventData)
    	{
    		mDir = Vector2.zero;
    		mStick.gameObject.SetActive (false);
    		mStickBack.gameObject.SetActive (false);
    	}

    	public void OnDrag(PointerEventData eventData)
    	{
    		Vector2 dragPos;
    		RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, eventData.position, eventData.pressEventCamera, out dragPos);
    		mDir = dragPos - mBeginDragPos;
    		if (mDir.sqrMagnitude > mRadius * mRadius)dragPos = mBeginDragPos + mRadius * mDir.normalized;
    		Vector3 v = mStick.transform.localPosition;
    		v.x = dragPos.x;
    		v.y = dragPos.y;
    		mStick.transform.localPosition = v;
    	}
    }

}
