/// <summary>
/// 该类主要用于UI动画,动画使用非缩放时间,每个AnimAction组件可以添加多个动画
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Arale.Engine
{
	public class AnimAction : MonoBehaviour
    {
		public enum ActionType
		{
			None,
			Event,
			Move,
			Scale,
			Rotate,
			Alpha,
            Anim,
		}

		public enum ActionMask
		{
			Everything = int.MaxValue,
			Nothing    = 0,
			WindowShow = 1<<1,
			WindowHide = 1<<2,
			WindowClose= 1<<3,

			//自定义
			User0      = 1<<4,
			User1      = 1<<5,
			User2      = 1<<6,
			User3      = 1<<7,
			User4      = 1<<8,
			User5      = 1<<9,
			User6      = 1<<10,
			User7      = 1<<11,
		}

		[System.Serializable]
		public class Action
		{
			public ActionType mType;
			public ActionMask mMask=ActionMask.Nothing;
			public bool       mEnable=true;
			public Transform  mTarget;
			public bool       mLocal=true;
			public Vector3    mFrom;
			public Vector3    mTo;
			public float      mStart;
			public float      mDuration;
			public AnimationCurve mCurve = new AnimationCurve();

			[System.NonSerialized]public bool	mPlay;
			[System.NonSerialized]public float	mElapse;
			IAction mAction;
			public bool Init()
			{
				switch(mType)
				{
				case ActionType.Event:
					if(mAction==null)mAction = new EventAction();
					break;
				case ActionType.Move:
					if(mAction==null)mAction = new MoveAction();
					break;
				case ActionType.Scale:
					if(mAction==null)mAction = new ScaleAction();
					break;
				case ActionType.Rotate:
					if(mAction==null)mAction = new RotateAction();
					break;
				case ActionType.Alpha:
					if(mAction==null)mAction = new AlphaAction();
					break;
				default:
					if(mAction==null)mAction = new IAction();
					break;
				}
				mElapse = 0;
				mAction.mData = this;
				return mAction.Init();
			}

			public void Update()
			{
				mElapse += Time.unscaledDeltaTime;
				if (mElapse < mStart)return;
				mAction.Update ();
			}
		}

		public class IAction
		{
			public Action mData;
			public virtual void Update(){}
			public virtual bool Init(){return false;}
		}

		public string mActionName="";
		[SerializeField]List<Action> mActions = new List<Action>();
		public List<Action> actions{get{return mActions;}}
		int mRunCount;
		// Update is called once per frame
		void Update ()
		{
			for(int i=0,max=mActions.Count;i<max;++i)
			{
				Action a = mActions[i];
				if(!a.mPlay)continue;
				a.Update();

			}
		}


		void Start()
		{
            EventMgr.single.AddListener (mActionName, onAnimActionMessage);
		}

		void OnDestroy()
		{
            EventMgr.single.UnAddListener (mActionName, onAnimActionMessage);
		}

        void onAnimActionMessage(EventMgr.EventData eb)
		{
            ActionMask am = (ActionMask)eb.data;
			play (am);
		}

		public static void sendActionMessage(string actionName, ActionMask mask)
		{
            EventMgr.single.PostEvent (actionName, mask);
		}

		public bool play(ActionMask mask=ActionMask.Everything)
		{
			bool hasAnim = false;
			for(int i=0,max=mActions.Count;i<max;++i)
			{
				Action a = mActions[i];
				if(((a.mMask&mask)!=0) && a.Init())
				{
					hasAnim=true;
					a.mPlay=true;
				}
			}
			return hasAnim;
		}

		public void stop(ActionMask mask=ActionMask.Everything)
		{
			for(int i=0,max=mActions.Count;i<max;++i)
			{
				Action a = mActions[i];
				if(!a.mPlay)continue;
				if((a.mMask&mask)!=0)a.mPlay=false;
			}
		}

		#region ActionItem
		public class EventAction:IAction
		{
			public override bool Init()
			{
				return true;
			}

			public override void Update()
			{
				mData.mTarget.SendMessage ("onActionEvent", (int)mData.mFrom.x);
				mData.mPlay = false;
			}
		}

		public class MoveAction:IAction
		{
			public override bool Init()
			{
				return true;
			}
			
			public override void Update()
			{
				float k = Mathf.Clamp01 (mData.mElapse / mData.mDuration);
				if(mData.mLocal)
					mData.mTarget.localPosition = mData.mFrom+mData.mCurve.Evaluate (k)*(mData.mTo-mData.mFrom);
				else
					mData.mTarget.position = mData.mFrom+mData.mCurve.Evaluate (k)*(mData.mTo-mData.mFrom);
				if (k >= 1)
				{
					mData.mPlay = false;
				}
			}
		}

		public class ScaleAction:IAction
		{
			public override bool Init()
			{
				return true;
			}
			
			public override void Update()
			{
				float k = Mathf.Clamp01 (mData.mElapse / mData.mDuration);
				mData.mTarget.localScale = mData.mFrom+mData.mCurve.Evaluate (k)*(mData.mTo-mData.mFrom);
				if (k >= 1)
				{
					mData.mPlay = false;
				}
			}
		}

		public class RotateAction:IAction
		{

		}
		
		public class AlphaAction:IAction
		{
	#if USE_NGUI
			UIPanel mPanel;
	#else
			CanvasGroup mPanel;
	#endif
			public override bool Init()
			{
				mData.mElapse = 0;
	#if USE_NGUI
				mPanel = mData.mTarget.GetComponent<UIPanel>();
	#else
				mPanel = mData.mTarget.GetComponent<CanvasGroup> ();
	#endif
				mPanel.alpha = mData.mFrom.x;
				return true;
			}

			public override void Update()
			{
				float k = Mathf.Clamp01 (mData.mElapse / mData.mDuration);
				mPanel.alpha = mData.mFrom.x+mData.mCurve.Evaluate (k)*(mData.mTo.x-mData.mFrom.x);
				if (k >= 1)
				{
					mData.mPlay = false;
				}
			}
		}

        public class AnimationAction:IAction
        {
        }
		#endregion
	}
}