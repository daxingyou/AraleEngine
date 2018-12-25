using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Arale.Engine
{

    [CustomEditor(typeof(AnimAction))]
    public class AnimActionInspector : Editor {
    	AnimAction mAnimAction;

    	void OnEnable()
    	{
    		mAnimAction = target as AnimAction;
    	}
    	
    	// Update is called once per frame
    	public override void OnInspectorGUI()
    	{
    		mAnimAction.mActionName = EditorGUILayout.TextField ("ActionName", mAnimAction.mActionName);
    		if (NGUIEditorTools.DrawHeader ("action-info", "ACTION-INFO"))
    		{
    			NGUIEditorTools.BeginContents ();
    			for (int i=0; i<mAnimAction.actions.Count; ++i)
    			{
    				drawActionInspector (i);
    			}
    			GUILayout.Space (10);
    			if (GUILayout.Button ("添加", GUILayout.Width (80)))onAddClick ();
    			NGUIEditorTools.EndContents ();
    		}
    	}

    	void drawActionInspector(int idx)
    	{
    		AnimAction.Action action = mAnimAction.actions[idx];
    		if (NGUIEditorTools.DrawHeader (""+idx+":"+action.mType, "ACTION"+idx))
    		{
    			NGUIEditorTools.BeginContents ();
    			action.mType = (AnimAction.ActionType)EditorGUILayout.EnumPopup("类型", action.mType);
    			action.mMask = (AnimAction.ActionMask)EditorGUILayout.EnumMaskField("过滤器", action.mMask);
    			action.mEnable = EditorGUILayout.Toggle("是否可用", action.mEnable);
    			switch(action.mType)
    			{
    			case AnimAction.ActionType.Event:
    				drawEventAction(action);
    				break;
    			case AnimAction.ActionType.Move:
    				drawMoveAction(action);
    				break;
    			case AnimAction.ActionType.Scale:
    				drawScaleAction(action);
    				break;
    			case AnimAction.ActionType.Rotate:
    				drawRotateAction(action);
    				break;
    			case AnimAction.ActionType.Alpha:
    				drawAlphaAction(action);
    				break;
    			default:
    				break;
    			}
    			if (GUILayout.Button ("删除", GUILayout.Width (80)))onDelClick (idx);
    			NGUIEditorTools.EndContents ();
    		}
    	}

    	#region 绘制ActionItem
    	void drawEventAction(AnimAction.Action action)
    	{
    		action.mTarget = EditorGUILayout.ObjectField("目标", action.mTarget, typeof(Transform), true) as Transform;
    		action.mFrom.x = EditorGUILayout.FloatField ("消息ID", action.mFrom.x);
    		action.mFrom.y = EditorGUILayout.FloatField ("重复次数", action.mFrom.y);
    		action.mFrom.z = EditorGUILayout.FloatField ("重复间隔", action.mFrom.z);
    		action.mStart = EditorGUILayout.FloatField("开始时间", action.mStart);
    	}

    	void drawMoveAction(AnimAction.Action action)
    	{
    		action.mTarget = EditorGUILayout.ObjectField("目标", action.mTarget, typeof(Transform), true) as Transform;
    		EditorGUILayout.BeginHorizontal ();
    		action.mFrom = EditorGUILayout.Vector3Field("From", action.mFrom);
    		if (GUILayout.Button ("snap", GUILayout.Width(60)))action.mFrom = action.mLocal?action.mTarget.transform.localPosition:action.mTarget.transform.position;
    		EditorGUILayout.EndHorizontal ();
    		EditorGUILayout.BeginHorizontal ();
    		action.mTo = EditorGUILayout.Vector3Field("To", action.mTo);
    		if (GUILayout.Button ("snap", GUILayout.Width(60)))action.mTo = action.mLocal?action.mTarget.transform.localPosition:action.mTarget.transform.position;
    		EditorGUILayout.EndHorizontal ();
    		action.mLocal = EditorGUILayout.Toggle("本地坐标", action.mLocal);
    		EditorGUILayout.BeginHorizontal ();
    		action.mStart = EditorGUILayout.FloatField("开始时间", action.mStart);
    		action.mDuration = EditorGUILayout.FloatField("持续时间", action.mDuration);
    		EditorGUILayout.EndHorizontal ();
    		action.mCurve = EditorGUILayout.CurveField("动画曲线", action.mCurve);
    	}

    	void drawScaleAction(AnimAction.Action action)
    	{
    		action.mTarget = EditorGUILayout.ObjectField("目标", action.mTarget, typeof(Transform), true) as Transform;
    		action.mFrom = EditorGUILayout.Vector3Field("From", action.mFrom);
    		action.mTo = EditorGUILayout.Vector3Field("To", action.mTo);
    		EditorGUILayout.BeginHorizontal ();
    		action.mStart = EditorGUILayout.FloatField("开始时间", action.mStart);
    		action.mDuration = EditorGUILayout.FloatField("持续时间", action.mDuration);
    		EditorGUILayout.EndHorizontal ();
    		action.mCurve = EditorGUILayout.CurveField("动画曲线", action.mCurve);
    	}

    	void drawRotateAction(AnimAction.Action action)
    	{
    	}

    	void drawAlphaAction(AnimAction.Action action)
    	{
    		action.mTarget = EditorGUILayout.ObjectField("目标", action.mTarget, typeof(Transform), true) as Transform;
    		action.mFrom.x = EditorGUILayout.FloatField("From(0~1)", action.mFrom.x);
    		action.mTo.x = EditorGUILayout.FloatField("To(0~1)", action.mTo.x);
    		EditorGUILayout.BeginHorizontal ();
    		action.mStart = EditorGUILayout.FloatField("开始时间", action.mStart);
    		action.mDuration = EditorGUILayout.FloatField("持续时间", action.mDuration);
    		EditorGUILayout.EndHorizontal ();
    		action.mCurve = EditorGUILayout.CurveField("动画曲线", action.mCurve);
    	}
    	#endregion

    	void onAddClick()
    	{
    		mAnimAction.actions.Add (new AnimAction.Action());
    	}

    	void onDelClick(int idx)
    	{
    		mAnimAction.actions.RemoveAt (idx);
    	}
    }

}
