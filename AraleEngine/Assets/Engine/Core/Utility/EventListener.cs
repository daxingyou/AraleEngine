using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace Arale.Engine
{
        
    public class EventListener : EventTrigger
    {
    	public enum EventType
    	{
    		PointerClick,
    		PointerDown,
    		PointerUp,
    		PointerEnter,
    		PointerExit,
    		Select,
    		Deselect,
    		UpdateSelected,
    		BeginDrag,
    		EndDrag,
    		Drag,
    		Drop,
    		Scroll,
    		Cancel,
    	}

    	public delegate void VoidDelegate (EventType et, BaseEventData eventData);
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
        public VoidDelegate onScroll;
        public VoidDelegate onCancel;
        public VoidDelegate onDeselect;
        public VoidDelegate onDrag;
        public VoidDelegate onDrop;
        public VoidDelegate onEndDrag;
    	public override void OnPointerClick(PointerEventData eventData)
        {
            if(onClick != null) onClick(EventType.PointerClick, eventData);
    	}
    	public override void OnPointerDown (PointerEventData eventData)
    	{
            if(onPointDown != null) onPointDown(EventType.PointerDown, eventData);
    	}
    	public override void OnPointerEnter (PointerEventData eventData)
    	{
            if(onPointerEnter != null) onPointerEnter(EventType.PointerEnter, eventData);
    	}
    	public override void OnPointerExit (PointerEventData eventData)
    	{
            if(onPointerExit != null) onPointerExit(EventType.PointerExit, eventData);
    	}
    	public override void OnPointerUp (PointerEventData eventData)
    	{
            if(onPointerUp != null) onPointerUp(EventType.PointerUp, eventData);
    	}
    	public override void OnSelect (BaseEventData eventData)
    	{
            if(onSelect != null) onSelect(EventType.Select, eventData);
    	}
    	public override void OnUpdateSelected (BaseEventData eventData)
    	{
            if(onUpdateSelected != null) onUpdateSelected(EventType.UpdateSelected, eventData);
    	}
    	public override void OnBeginDrag (PointerEventData eventData)
    	{
            if(onBeginDrag != null) onBeginDrag(EventType.BeginDrag, eventData);
    	}
    	public override void OnScroll (PointerEventData eventData){
            if(onScroll != null) onScroll(EventType.Scroll, eventData);
    	}
    	public override void OnCancel (BaseEventData eventData)
    	{
            if(onCancel != null) onCancel(EventType.Cancel, eventData);
    	}
    	public override void OnDeselect (BaseEventData eventData)
    	{
            if(onDeselect != null) onDeselect(EventType.Deselect, eventData);
    	}
    	public override void OnDrag (PointerEventData eventData)
    	{
            if(onDrag != null) onDrag(EventType.Drag, eventData);
    	}
    	public override void OnDrop (PointerEventData eventData)
    	{
            if(onDrop != null) onDrop(EventType.Drop, eventData);
    	}
    	public override void OnEndDrag (PointerEventData eventData)
    	{
            if(onEndDrag != null) onEndDrag(EventType.EndDrag, eventData);
    	}
    }

}
