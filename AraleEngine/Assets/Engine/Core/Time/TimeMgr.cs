using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Arale.Engine
{
    #region 服务器相关时间
    public class RTime : IData
    {
        public static RTime R = new RTime ();
        static long UTC_TICK = new System.DateTime (1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc).Ticks;
        int   _frameCount;
        float _begin;
        long  _utcTick;
        long  _diffTick;
        DateTime _utcTime;

        public RTime()
        {
            _utcTime = System.DateTime.UtcNow;
            _diffTick= 0;
            _utcTick = _utcTime.Ticks;
            _begin = Time.realtimeSinceStartup;
            ResetValue();
        }

        //同步服务器时间后时间是服务器时间 utcTickNs服务器utc时间戳 tickNs服务器本地时间戳 单位为纳秒
        public void syncServerTime(long utcTickNs, long tickNs, bool begin1970=true)
        {
            //java tick是毫秒，c#tick是100纳秒,c#从0001年开始,而java从1970年算起
            _utcTime = new System.DateTime(utcTickNs + (begin1970?UTC_TICK:0), System.DateTimeKind.Utc);
            _diffTick= tickNs - utcTickNs;
            _utcTick = _utcTime.Ticks;
            _begin = Time.realtimeSinceStartup;
            ResetValue();
        }

        public System.DateTime utcTime
        {//返回服务器utc时间
            get
            {
                return _utcTime.AddSeconds (Time.realtimeSinceStartup - _begin);
            }
        }

        public System.DateTime time
        {//返回服务器当地时间(通常活动配表时间使用)
            get
            {
                //return _utcTime.AddSeconds (Time.realtimeSinceStartup - _begin + _diffTickMS*MS);
                return _utcTime.AddTicks ((long)((Time.realtimeSinceStartup - _begin)*TimeMgr.S2NS) + _diffTick);
            }
        }

        public System.DateTime localTime
        {//返回服务器时间对应的本机时间
            get
            {
                return _utcTime.AddSeconds (Time.realtimeSinceStartup - _begin).ToLocalTime();
            }
        }

        public long utcTickMs
        {
            get
            {
                return (long)(_utcTick*TimeMgr.NS2MS + 1000*(Time.realtimeSinceStartup - _begin));
            }
        }

        public long utcTickNs
        {
            get
            {
                return _utcTick + (long)((Time.realtimeSinceStartup - _begin) * TimeMgr.S2NS);
            }
        }

        #region 整时变换通知
        int _oldDay;
        int _oldHour;
        int _oldMin;
        int _oldSec;
        void ResetValue()
        {
            _oldDay  = -1;
            _oldHour = -1;
            _oldMin  = -1;
            _oldSec  = -1;
        }
        public void Update()
        {
            System.DateTime dt = time;
            if (Time.frameCount - _frameCount < 10)return;
            if (dt.Second == _oldSec)return;
            Notify (0, _oldSec=dt.Second);
            if (dt.Minute == _oldMin)return;
            Notify (1, _oldMin=dt.Minute);
            if (dt.Hour == _oldHour)return;
            Notify (2, _oldHour=dt.Hour);
            if (dt.Day == _oldDay)return;
            Notify (3, _oldDay=dt.Day);
        }
        #endregion
    }
    #endregion


    public class TimeMgr : MgrBase<TimeMgr>
	{
		public const double MS= 0.001;
		public const long  S = 1;
		public const long  M = 60;
		public const long  H = 3600;
		public const long  D = 24 * 3600;
		public const long  W = 7 * 24 * 3600;
		public const long  MS2NS = 10000;
		public const double NS2MS = 0.0001;
		public const long  S2NS  = 10000000;
		public const double NS2S  = 0.0000001;
	
		Dictionary<string, TimeAxis> mMap = new Dictionary<string, TimeAxis>();
		TimeAxis mRealTimeAxis  = new TimeAxis(null);
		public TimeAxis realTimeAxis{get{return mRealTimeAxis;}}
		TimeAxis mScaleTimeAxis = new TimeAxis(null);
		public TimeAxis scaleTimeAxis{get{return mScaleTimeAxis;}}
		TimeAxis mDateTimeAxis = new TimeAxis(null);
		public TimeAxis dateTimeAxis{get{return mDateTimeAxis;}}

		public TimeAxis GetTimeAxis(string name, bool create=false)
		{
			if (name == null)return null;
			TimeAxis a = null;
			mMap.TryGetValue (name, out a);
			if (a == null && create)
			{
				a = new TimeAxis (name);
				mMap [name] = a;
			}
			return a;
		}

        float mScaleTime;
        public override void Update()
		{
            mDateTimeAxis.Update(RTime.R.time);
			mRealTimeAxis.Update (Time.realtimeSinceStartup);
			mScaleTimeAxis.Update(mScaleTime+=Time.deltaTime);
			DoDelayAction ();
		}

		#region 延迟调用
        static List<Action> sDelayAction = new List<Action> (5);
		void DoDelayAction()
		{
			for (int i = 0, max = sDelayAction.Count; i < max; ++i)
			{
                Action a = sDelayAction[i];
                if (a.onAction == null)continue;
                a.onAction(a);
			}
			sDelayAction.Clear ();
		}
		#endregion


		#region 时间轴事件
		public class Action
		{
            public delegate void  OnAction(Action self);
            public OnAction onAction;
            public float  doTime;   //相对时间轴时间
			public bool   hasDone;  //是否已执行
            public bool   delayCall;//如果onAction调用栈中有对TimeManager的的操作，都应该使用delaycall,否则会有异想不到的错误
            public object userData; //自定义数据
            public void  Loop(float interval)//间隔多少秒时间再执行一次，在OnAction中调用实现循环
            {
                doTime += interval;
                hasDone = false;
            }

            internal void  DoAction()
            {
                hasDone = true;
                if (delayCall)
                {
                    sDelayAction.Add (this);
                }
                else
                {
                    if (onAction != null)onAction(this);
                }
            }
		}

		public class DateAction : Action
		{
			public short y=-1;//年
            public short M=-1;//月
            public short d=-1;//日
            public short w=-1;//星期
            public short h=-1;//时
            public short m=-1;//分
            public short s=-1;//秒
			public bool IsTimeOk(DateTime dt)
			{
				if ((y<0||y==dt.Year) && (M<0||M==dt.Month) && (d<0||d==dt.Day) && (w<0||w==(int)dt.DayOfWeek) && (h<0||h==dt.Hour) && (m<0||m==dt.Minute) && (s<0||s==dt.Second))
				{
					if(!hasDone)
					{
						hasDone = true;
						return true;
					}
					return false;
				}
				hasDone = false;
				return false;
			}
		}
		#endregion


		#region 时间轴
		public class TimeAxis
		{
            string mName;
            public string name{get{return mName == null ? "" : mName;}}
			List<TimeAxis> mChilds = new List<TimeAxis> ();
            public List<TimeAxis> childs{get{ return mChilds; }}
			List<Action> mActions = new List<Action> ();
            public List<Action> actions{get{ return mActions; }}

			TimeAxis mParent;
			public TimeAxis parent{get{return mParent;}}
			float  mRelativeTime=0; //相对父时间轴的时间
			public float relativeTime{get{return mRelativeTime;}}
			float  mTime=0;         //时间轴当前时间(相对自己)
			public float time{get{return mTime;}}
			float  mScale=1;        //时间轴缩放系数
			int    mEventIdx;
			bool   mEnable=true;
			public bool enable{ set{ mEnable = value;} get{return mEnable;}}
			bool   mDirty=true;

            public TimeAxis(){}
            internal TimeAxis(string name)
			{
                if(TimeMgr.single.mMap.ContainsKey(name))
                {
                    throw new Exception("TimeAxis named "+name+" has existed");
                }
                mName = name;
                TimeMgr.single.mMap.Add(name, this);
			}

			public Action AddAction(Action te)
			{
				mDirty = true;
                mActions.Add (te);
				return te;
			}

			public void RemoveAction(Action te)
			{
				mDirty = true;
                mActions.Remove (te);
			}

			public void ClearAction()
			{
                mActions.Clear ();
			}

			void Reset()
			{
				mTime  = 0;
				mDirty = true;
                for (int i = 0, max = mActions.Count; i < max; ++i)
				{
                    mActions [i].hasDone = false;
				}
			}

            public void Remove()
            {
                if (mName != null)TimeMgr.single.mMap.Remove (mName);
                if (mParent == null)return;
                mParent.mChilds.Remove (this);
                mParent = null;
            }

            //循环时间轴
            public void Loop()
            {
                mRelativeTime = mRelativeTime + mTime;
                Reset();
            }

			public void SetParent(TimeAxis parent, float relativeTime=0)
			{
                if (mParent != null)mParent.mChilds.Remove (this);
				mRelativeTime = relativeTime;
				mParent = parent;
                if (mParent != null)mParent.mChilds.Add (this);
				Reset ();
			}

			public float startTime{
				get
				{//绝对时间
					return mParent==null?mRelativeTime:mRelativeTime+mParent.startTime;
				}
			}

			public float endTime{
				get
				{//绝对时间
                    return mActions.Count > 0?startTime + mActions [mActions.Count-1].doTime:startTime;
				}
			}

			public void Update(float t)
			{
				mTime = t - mRelativeTime;
				if (!mEnable)return;
				if (mDirty)
				{
					mEventIdx = 0;
                    mActions.Sort(delegate(Action a, Action b){return a.doTime.CompareTo(b.doTime);});
					mDirty = false;
				}

                for (int max = mActions.Count; mEventIdx < max; ++mEventIdx)
				{
                    Action te = mActions [mEventIdx];
					if (te.hasDone)continue;
					if (te.doTime > mTime)break;
                    te.DoAction();
                    if (!te.hasDone)
                    {//循环时hasDone被置为false,需要重排序
                        mDirty = true;
                    }
				}

                for(int i=0,max=mChilds.Count; i<max; ++i)
				{
                    mChilds[i].Update(mTime);
				}
			}

			//相对时间无效
			public void Update(DateTime dt)
			{
				if (!mEnable)return;
                for (int i = mActions.Count-1; i >= 0; --i)
				{
                    DateAction te = mActions [i] as DateAction;
					if (!te.IsTimeOk(dt))continue;
                    te.DoAction();
				}

                for(int i=0,max=mChilds.Count; i<max; ++i)
				{
                    mChilds[i].Update(dt);
				}
			}
		}
		#endregion
	}
}