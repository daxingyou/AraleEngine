using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace Arale.Engine
{
        
    public class EventListener : EventTrigger
    {
    	public delegate void VoidDelegate (BaseEventData eventData);
    	public static EventListener Get (GameObject go)
        {
    		EventListener listener = go.GetComponent<EventListener>();
    		if (listener == null) listener = go.AddComponent<EventListener>();
    		return listener;
    	}

        public VoidDelegate onClick;
        public void AddOnClick(VoidDelegate callback){onClick += callback;}
        public void RemoveOnClick(VoidDelegate callback){onClick -= callback;}
        public VoidDelegate onPointDown;
        public VoidDelegate onPointerEnter;
        public VoidDelegate onPointerExit;
        public VoidDelegate onPointerUp;
        public VoidDelegate onSelect;
        public VoidDelegate onUpdateSelected;
        public VoidDelegate onBeginDrag;
		public void AddOnBeginDrag(VoidDelegate callback){onBeginDrag += callback;}
		public void RemoveOnBeginDrag(VoidDelegate callback){onBeginDrag -= callback;}
        public VoidDelegate onScroll;
        public VoidDelegate onCancel;
        public VoidDelegate onDeselect;
        public VoidDelegate onDrag;
        public VoidDelegate onDrop;
        public VoidDelegate onEndDrag;
    	public override void OnPointerClick(PointerEventData eventData)
        {
            if(onClick != null) onClick(eventData);
    	}
    	public override void OnPointerDown (PointerEventData eventData)
    	{
            if(onPointDown != null) onPointDown(eventData);
    	}
    	public override void OnPointerEnter (PointerEventData eventData)
    	{
            if(onPointerEnter != null) onPointerEnter(eventData);
    	}
    	public override void OnPointerExit (PointerEventData eventData)
    	{
            if(onPointerExit != null) onPointerExit(eventData);
    	}
    	public override void OnPointerUp (PointerEventData eventData)
    	{
            if(onPointerUp != null) onPointerUp(eventData);
    	}
    	public override void OnSelect (BaseEventData eventData)
    	{
            if(onSelect != null) onSelect(eventData);
    	}
    	public override void OnUpdateSelected (BaseEventData eventData)
    	{
            if(onUpdateSelected != null) onUpdateSelected(eventData);
    	}
    	public override void OnBeginDrag (PointerEventData eventData)
    	{
            if(onBeginDrag != null) onBeginDrag(eventData);
    	}
    	public override void OnScroll (PointerEventData eventData){
            if(onScroll != null) onScroll(eventData);
    	}
    	public override void OnCancel (BaseEventData eventData)
    	{
            if(onCancel != null) onCancel(eventData);
    	}
    	public override void OnDeselect (BaseEventData eventData)
    	{
            if(onDeselect != null) onDeselect(eventData);
    	}
    	public override void OnDrag (PointerEventData eventData)
    	{
            if(onDrag != null) onDrag(eventData);
    	}
    	public override void OnDrop (PointerEventData eventData)
    	{
            if(onDrop != null) onDrop(eventData);
    	}
    	public override void OnEndDrag (PointerEventData eventData)
    	{
            if(onEndDrag != null) onEndDrag(eventData);
    	}
    }

}
