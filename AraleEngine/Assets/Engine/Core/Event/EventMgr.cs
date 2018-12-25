using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace Arale.Engine
{
    public class EventMgr : MgrBase<EventMgr>
    {
		public class EventData
        {
			public string eventID;
            public object data;
            public EventData(string id, object data)
			{
                this.eventID = id;
                this.data    = data;
			}
		}
        public delegate void EventCallback(EventData ed);
		
		private Dictionary<string, List<EventCallback>> mCallbacks = new Dictionary<string, List<EventCallback>>();
        public void AddListener(string id, EventCallback callback)
		{
			lock (this)
			{
                List<EventCallback> callbacks;
                if(!mCallbacks.TryGetValue(id, out callbacks))
                {
                    mCallbacks.Add(id, callbacks = new List<EventCallback>());
                }
                callbacks.Add(callback);
			} 
		}
		
        public void UnAddListener(string id, EventCallback callback)
		{
			lock (this)
			{
                List<EventCallback> callbacks;
                if(mCallbacks.TryGetValue(id, out callbacks))
                {
                    callbacks.Remove(callback);
                }
			}
		}
		
		bool mIsEnuming = false;
        List<EventData> mEvents = new List<EventData>();
        List<EventData> mPendingEvents = new List<EventData>();
		public void PostEvent(string id, object data=null)
		{
			lock (this)
			{
                if (!HasCallback(id))
				{
					return;
				}

                if (mIsEnuming)
                {
                    mPendingEvents.Add(new EventData(id, data));
                }
                else
                {
                    mEvents.Add(new EventData(id, data));
                }
			}
		}

        public void SendEvent(string id, object data=null)
        {
            lock (this)
            {
                if (!HasCallback(id))
                {
                    return;
                }

                if (mIsEnuming)
                {
                    mPendingEvents.Add(new EventData(id, data));
                }
                else
                {
                    mIsEnuming = true;
                    EventData ed = new EventData(id, data);
                    DoCallback(new EventData(id, data));
                    mIsEnuming = false;
                }
            }
        }

        public override void Update()
		{
			lock (this)
			{
                if (mEvents.Count == 0)
				{
                    mEvents.AddRange(mPendingEvents);
                    mPendingEvents.Clear();
					return;
				} 

                mIsEnuming = true;
                for(int i=0,imax=mEvents.Count; i<imax; ++i)
				{
                    DoCallback(mEvents[i]);
				}
                mEvents.Clear();
                mIsEnuming = false;
			}
		}

        void DoCallback(EventData ed)
        {
            List<EventCallback> lsCallback = mCallbacks[ed.eventID];
            for(int i=lsCallback.Count-1; i>=0; --i)
            {
                EventCallback ecb = lsCallback[i];
                if (ecb != null)
                {
                    ecb(ed);
                }
            }
        }

        bool HasCallback(string id)
        {
            List<EventCallback> callbacks;
            if(mCallbacks.TryGetValue(id, out callbacks))
            {
                return callbacks.Count > 0;
            }
            return false;
        }
	}
}