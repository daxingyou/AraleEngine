using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
//http://docs.manew.com/Script/index.htm
public class SkillEditorWindow : EditorWindow{
	static SkillEditorWindow mThis;
	const int TimeUnitWidth = 100;//1s像素宽度
	Rect winRect;           //窗口大小
	Rect leftRect;          //左侧编辑栏大小
	Rect rightRect;         //右侧编辑栏大小
    int      skillIdx;
    GameSkill gameSkill;   //技能对象
	GameObject  mDebugGameobject;
	public static SkillEditorWindow single{get{ return mThis; }}
	[MenuItem("Assets/Create/SkillWindow")]
	static void createSkillAsset()
    {
        GameSkill.reset(true);
		string path = Application.dataPath; 
		path = NGUIEditorTools.GetSelectionFolder ();
		openSkillEditorWindow ();
		mThis.mSavePath = newFilePath(path);
		AssetDatabase.Refresh ();
	}

	static string newFilePath(string path)
	{
		int i = 1;
		string s = path + "NewSkill.skill.txt";
		while(File.Exists (s))
		{
			s = path + string.Format ("NewSkill{0}.skill.txt", i++);
		}
		return s;
	}

	[OnOpenAssetAttribute(1)]
	public static bool openSkilAsset(int instanceID, int line)
	{//选中打开asset资源文件
		string path = AssetDatabase.GetAssetPath(EditorUtility.InstanceIDToObject(instanceID));
        byte[] ctx = File.ReadAllBytes(path);
        //文件必须放在Resources目录下
        Debug.Assert(path.Contains("Resources"));
        Debug.Assert(path.EndsWith(".skill.txt") && GameSkill.isSkillFile(ctx));
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
		leftRect  = new Rect(0, 0, 300,winRect.height);
		rightRect = new Rect (300, 0, winRect.width - 300, winRect.height);
	}

	void Update()
	{
		Repaint ();
	}

	void OnDestroy()
	{
		mThis = null;
	}

	Rect  mSplitRect;
	bool  mSplitDrag;
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
        GUILayout.BeginHorizontal();
        GUILayout.Label("文件:" + Path.GetFileName(mThis.mSavePath));
        drawTestGUI();
        GUILayout.EndHorizontal();
        drawSkillSelector();
        if(gameSkill!=null)gameSkill.drawGUI(mTickLine);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("保存"))save();
		GUILayout.EndScrollView ();
		GUILayout.EndArea ();
	}

	bool  mTimeDrag;
    float mTickLine;
	void drawRightPanle()
	{
		GUILayout.BeginArea (rightRect);
        Rect rc = new Rect(32, 0, rightRect.width, rightRect.height);
        mTickLine = drawTimeCtrl (rc, TimeUnitWidth, 10, mTickLine, ref mTimeDrag, GameSkill.isDrag);
        rc.height = 50;
        if (gameSkill != null)
        {
            gameSkill.draw(rc, TimeUnitWidth);
            gameSkill.drag(mTickLine, mTimeDrag);
        }
        GUILayout.Space(rc.height);
        GUILayout.Label("生");
        GUILayout.Label("移");
        GUILayout.Label("动");
        GUILayout.Label("技");
        GUILayout.Label("显");
        GUILayout.Label("伤");
		GUILayout.EndArea ();
	}

    void drawSkillSelector()
    {
        if (GameSkill.names!=null && GameSkill.names.Length > 0)
        {
            int idx = EditorGUILayout.Popup(skillIdx, GameSkill.names);
            if (idx != skillIdx && idx >= 0)
            {
                noEndActionTip();
                skillIdx = idx;
                gameSkill = GameSkill.get(GameSkill.names[skillIdx]);
            }
        }
        GUILayout.BeginHorizontal();
        switch (GUILayout.Toolbar(-1, new string[]{"删除技能","添加技能"}))
        {
            case 0:
                if(gameSkill == null)return;
                if (!EditorUtility.DisplayDialog("提示", "确定删除当前技能吗", "确定", "取消"))return;
                GameSkill.delete(gameSkill.name);
                gameSkill = null;
                skillIdx  = -1;
                break;
            case 1:
                GameSkill newskill = GameSkill.create("new skill");
                if (newskill == null)
                {
                    Debug.LogError("new skill技能已存在,请先改名称后再新建");
                }
                else
                {
                    noEndActionTip();
                    gameSkill = newskill;
                    skillIdx = ArrayUtility.IndexOf<string>(GameSkill.names, gameSkill.name);
                }
                break;
        }
        GUILayout.EndHorizontal();
    }

    void noEndActionTip()
    {
        if (gameSkill == null||gameSkill.hasEndAction())return;
        Debug.LogError("skill has no end action!!! name="+gameSkill.name);
    }

	#region 键盘鼠标事件
	void onKeyMouseEvent(Event e)
	{
        if (e.isKey && e.type == EventType.keyUp)
		{
			onKeyUpEvent (e.keyCode);
		}
		else if (e.isMouse && e.type == EventType.MouseUp)
		{
			onMouseKeyUp (e);
		}
	}

	void onKeyUpEvent(KeyCode kc)
	{
        if(kc == KeyCode.Delete&&gameSkill!=null)gameSkill.deleteSelected();
	}

	void onMouseKeyUp(Event e)
	{
	}
	#endregion

    #region 绘制接口
    public static float drawTimeCtrl(Rect rc, float unitWidth, float second, float tickLine, ref bool drag, bool dragItem=false)
    {
        Rect timeBar = new Rect (rc.x, rc.y, rc.width, 50);
        Rect dragArea = dragItem ? rc : timeBar;
        GUI.Box (timeBar,"");
        GUI.Label(new Rect(rc.x, rc.y+30, unitWidth, 20),tickLine.ToString("F2"));
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
                    Handles.color = Color.gray;
                    Handles.DrawLine (new Vector3 (rc.x+tenth, 0, 0), new Vector3 (rc.x+tenth, 10, 0));
                    Handles.color = new Color(0f, 0f, 0f, 0.2f);
                    Handles.DrawLine (new Vector3 (rc.x+tenth, 50, 0), new Vector3 (rc.x+tenth, rc.height, 0));
                }
                Handles.color = Color.gray;
                Handles.DrawLine (new Vector3 (rc.x+half, 0, 0), new Vector3 (rc.x+half, 20, 0));
            }
            Handles.DrawLine (new Vector3 (rc.x+one, 0, 0), new Vector3 (rc.x+one, rc.height, 0));
            GUI.Label (new Rect (rc.x+one, 10, unitWidth, 20), i+":00");
        }
        Handles.color = Color.red;
        Handles.DrawLine (new Vector3 (rc.x+tickLine*unitWidth, 0, 0), new Vector3 (rc.x+tickLine*unitWidth, rc.height, 0));
        Handles.EndGUI ();

        if (Event.current != null)
        {
            Event e = Event.current;
            switch (e.rawType)
            {
                case EventType.MouseDown:
                    if (!dragArea.Contains(e.mousePosition))break;
                    drag=true;
                    tickLine = (e.mousePosition.x-rc.x)/unitWidth;
                    break;
                case EventType.MouseDrag:
                    if (!drag)break;
                    tickLine = (e.mousePosition.x-rc.x)/unitWidth;
                    break;
                case EventType.MouseUp:
                    drag=false;
                    break;
            }
        }
        return toTick(tickLine, 0.1f);
    }

    static float toTick(float tickLine, float timeUnit)
    {//转换到整刻度时间
        return timeUnit*((int)(tickLine/timeUnit+0.5f));
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
    #endregion

	#region 保存工程
	string mSavePath="";
	void save()
	{
        noEndActionTip();
        if (File.Exists(mSavePath))
        {
            if (!EditorUtility.DisplayDialog("提示", "文件已存在,确定覆盖吗", "确定", "取消"))return;
        }

        if (!GameSkill.saveSkill(mSavePath))
        {
            Debug.LogError("保存文件失败 path="+mSavePath);
        }
        else
        {
            AssetDatabase.Refresh();
        }
	}

	void open(string path)
	{
        GameSkill.reset(true);
        if (GameSkill.loadSkill(path))
        {
            GameSkill.genNames();
            mThis.mSavePath = path;
            mThis.gameSkill = null;
            if (GameSkill.names.Length < 1)return;
            mThis.gameSkill = GameSkill.get(GameSkill.names[mThis.skillIdx=0]);
        }
        else
        {
            mThis.mSavePath = "";
            Debug.LogError("打开文件失败 path="+path);
        }
	}
	#endregion

    #region 测试
    Unit testUnit;
    bool testing;
    void drawTestGUI()
    {
        testUnit = (Unit)EditorGUILayout.ObjectField(testUnit, typeof(Unit), true);
        if (testUnit!=null&&gameSkill!=null&&GUILayout.Button(testing?"停止测试":"开始测试"))
        {
            //testUnit.buff.addBuff();
            testing=!testing;
        }
    }
    #endregion
}