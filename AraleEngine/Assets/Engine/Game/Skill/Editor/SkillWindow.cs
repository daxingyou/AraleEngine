using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
//http://docs.manew.com/Script/index.htm
public class SkillEditorWindow : EditorWindow{
	static SkillEditorWindow mThis;
	const int TimeUnitWidth = 100;
	Rect winRect;           //窗口大小
	Rect leftRect;          //左侧编辑栏大小
	Rect rightRect;         //右侧编辑栏大小
	GameObject  mDebugGameobject;
	public static SkillEditorWindow single{get{ return mThis; }}
	[MenuItem("Assets/Create/SkillWindow")]
	static void createSkillAsset()
	{
		string path = Application.dataPath; 
		path = NGUIEditorTools.GetSelectionFolder ();
		openSkillEditorWindow ();
		mThis.mSavePath = newFilePath(path);
		mThis.save ();
		AssetDatabase.Refresh ();
	}

	static string newFilePath(string path)
	{
		int i = 1;
		string s = path + "NewSkill.skill";
		while(File.Exists (s))
		{
			s = path + string.Format ("NewSkill{0}.skill", i++);
		}
		return s;
	}

	[OnOpenAssetAttribute(1)]
	public static bool openSkilAsset(int instanceID, int line)
	{//选中打开asset资源文件
		string path = AssetDatabase.GetAssetPath(EditorUtility.InstanceIDToObject(instanceID));
		if(!path.EndsWith(".skill"))return false;
		openSkillEditorWindow().open (path);
		mThis.Focus ();
		return true;
	}

	public static SkillEditorWindow openSkillEditorWindow()
	{
		if (mThis == null)EditorWindow.GetWindow<SkillEditorWindow>();
		mThis.ShowUtility();
		return mThis;
	}

	public SkillEditorWindow()
	{
		mThis = this;
		winRect   = mThis.position;
		leftRect  = new Rect(0, 0, 240,winRect.height);
		rightRect = new Rect (240, 0, winRect.width - 240, winRect.height);
	}

	void Update()
	{
		Repaint ();
	}

	void OnDestroy()
	{
		mThis = null;
	}


	public static float drawTimeCtrl(Rect rc, float unitWidth, float second, float tickLine, ref bool drag)
	{
		Rect timeBar = new Rect (rc.x, rc.y, rc.width, 50);
		GUI.Box (timeBar,"");
		Handles.BeginGUI ();
		int len = (int)(second + 0.99f);
		for(int i=0;i<=len;++i)
		{
			float one  = i*unitWidth;
			float half = (i + 0.5f) * unitWidth;
			Handles.color = Color.gray;
			if (i < len)
			{
				for (int j = 0; j < 10; ++j)
				{
					float tenth = one + 0.1f * j * unitWidth;
					Handles.DrawLine (new Vector3 (tenth, 0, 0), new Vector3 (tenth, 10, 0));
				}
				Handles.DrawLine (new Vector3 (half, 0, 0), new Vector3 (half, 20, 0));
			}
			Handles.DrawLine (new Vector3 (one, 0, 0), new Vector3 (one, rc.height, 0));
			GUI.Label (new Rect (one, 10, unitWidth, 20), i+":00");
		}
		Handles.color = Color.red;
		Handles.DrawLine (new Vector3 (tickLine, 0, 0), new Vector3 (tickLine, rc.height, 0));
		Handles.EndGUI ();

		if (Event.current != null)
		{
			switch (Event.current.rawType)
			{
			case EventType.MouseDown:
				if (!timeBar.Contains (Event.current.mousePosition))break;
				drag=true;
				tickLine = Event.current.mousePosition.x;
				break;
			case EventType.MouseDrag:
				if (!drag)break;
				tickLine = Event.current.mousePosition.x;
				break;
			case EventType.MouseUp:
				drag=false;
				break;
			}
		}
		return tickLine;
	}

	public static Rect drawSplitCtrl(Rect rc, bool portrait, float min, float max, ref bool drag)
	{
		Handles.color = Color.black;
		if (portrait)
		{
			Handles.DrawLine (new Vector3 (rc.center.x, 0, 0), new Vector3 (rc.center.x, rc.height, 0));
			EditorGUIUtility.AddCursorRect (rc, MouseCursor.SplitResizeLeftRight);
		}
		else
		{
			Handles.DrawLine (new Vector3 (0, rc.center.y, 0), new Vector3 (rc.width, rc.center.y, 0));
			EditorGUIUtility.AddCursorRect (rc, MouseCursor.SplitResizeUpDown);
		}
		if (Event.current != null)
		{
			switch (Event.current.rawType)
			{
			case EventType.MouseDown:
				if(rc.Contains(Event.current.mousePosition))drag=true;
				break;
			case EventType.MouseDrag:
				if (!drag)break;
				if (portrait)
				{
					rc.x += Event.current.delta.x;
					if (rc.x < min)rc.x = min;
					else if (rc.x > max)rc.x = max;
				}
				else
				{
					rc.y += Event.current.delta.y;
					if (rc.y < min)rc.y = min;
					else if (rc.y > max)rc.y = max;
				}
				break;
			case EventType.MouseUp:
				drag=false;
				break;
			}
		}
		return rc;
	}

	Rect  mSplitRect;
	bool  mSplitDrag;
	float mTickLine;
	void OnGUI ()
	{
		winRect = mThis.position;
		leftRect.height = winRect.height;
		rightRect.height = winRect.height;
		if(mSplitRect.width==0)mSplitRect=new Rect (leftRect.xMax-3, 0, 5, winRect.height);
		Event e = new Event(Event.current);//需要放在dragwindow调用前，否则鼠标事件被截获了
		BeginWindows();
		drawLeftPanel();
		//绘制分割栏
		mSplitRect = drawSplitCtrl (mSplitRect, true, 0, winRect.width - mSplitRect.width, ref mSplitDrag);
		drawRightPanle ();
		EndWindows ();
		onKeyMouseEvent (e);
		deleteNode ();
		Repaint();
		leftRect.width = mSplitRect.x - mSplitRect.width/2;
		rightRect.x = mSplitRect.x + mSplitRect.width;
		rightRect.width = winRect.width - rightRect.x;
	}

	Vector2 mLeftScroll;
	void drawLeftPanel()
	{
		GUILayout.BeginArea (leftRect);
		mLeftScroll = GUILayout.BeginScrollView (mLeftScroll);

		GUILayout.EndScrollView ();
		GUILayout.EndArea ();
	}

	bool mTimeDrag;
	void drawRightPanle()
	{
		GUILayout.BeginArea (rightRect);
		mTickLine = drawTimeCtrl (new Rect (0, 0, rightRect.width, rightRect.height), TimeUnitWidth, 10, mTickLine, ref mTimeDrag);
		GUILayout.EndArea ();
	}

	void deleteNode()
	{
	}

	#region 键盘鼠标事件
	void onKeyMouseEvent(Event e)
	{
		if (e.isKey && e.type == EventType.MouseUp)
		{
			onKeyUpEvent (e.keyCode);
		}
		else if (e.isMouse && e.type == EventType.MouseUp)
		{
			onMouseKeyUp (e);
		}
	}

	public void onKeyUpEvent(KeyCode kc)
	{
	}

	public void onMouseKeyUp(Event e)
	{
	}
	#endregion


	#region 保存工程
	string mSavePath="";
	void save()
	{
		
	}

	void open(string path)
	{
	}
	#endregion
}
