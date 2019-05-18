using UnityEngine;
using System.Collections;
using taskproto;
using System.Collections.Generic;
using System.Linq;
using XLua;
using System.IO;
using Scripts.CoreScripts.Core;
using Scripts.CoreScripts.NetWrok;
using gameproto;
using Scripts.CoreScripts.Core.Notice;
using LitJson;
using System;
using Scripts.CoreScripts.GameLogic.Data;

[LuaCallCSharp]
public class MissionManager : IData{
	public const int MissionStateChange = 1;
	static MissionManager _this;
	public static MissionManager Instance
	{
		get
		{
			if (_this == null)
			{
				_this = new MissionManager();
			}
			return _this;
		}
	}
		
	public bool _taskSort;
	private PlayerTaskListNtf _taskList;
	public PlayerTaskListNtf TaskList
	{
		get { return _taskList; }
		set
		{
			_taskList = value;
			_taskSort = false;
            UpdateRedPoint();
		}
	}

	public void ReSort()
	{
		if (TaskList != null)
		{
			if (_taskSort)return;
			IEnumerable<PlayerTaskInfo> query = null;
			query = from task in TaskList.tasks orderby task.task_state descending, task.sort ascending select task;

			List<PlayerTaskInfo> tasks = new List<PlayerTaskInfo>();
			List<PlayerTaskInfo> completeTasks = new List<PlayerTaskInfo>();
			foreach (var item in query)
			{
				if ((PlayerTaskState)item.task_state == PlayerTaskState.TaskGetRewardState)
				{
					completeTasks.Add(item);
				}
				else
				{
					tasks.Add(item);
				}

			}

			TaskList.tasks.Clear();
			for (int i = 0; i < tasks.Count; i++)
			{
				TaskList.tasks.Add(tasks[i]);
			}

			for (int i = 0; i < completeTasks.Count; i++)
			{
				TaskList.tasks.Add(completeTasks[i]);
			}
			_taskSort = true;
		}
	}

	public void DelTask(uint taskId)
	{
		if (TaskList != null)
		{
			for (int i = 0; i < TaskList.tasks.Count; i++)
			{
				if (TaskList.tasks[i].task_id == taskId)
				{
					TaskList.tasks.RemoveAt(i);
					break;
				}
			}
			_taskSort = false;
            UpdateRedPoint();
		}
	}

	public void AddTask(PlayerTaskInfo info)
	{
		if (TaskList != null)
		{
			TaskList.tasks.Add(info);
			_taskSort = false;
            UpdateRedPoint();
		}
	}

	public void TaskUpdate(uint taskid, uint conditionCount, uint state)
	{
		if (TaskList != null)
		{
			for (int i = 0; i < TaskList.tasks.Count; i++)
			{
				if (TaskList.tasks[i].task_id == taskid)
				{
					TaskList.tasks[i].cur_condition_count = conditionCount;
					TaskList.tasks[i].task_state = state;
					notify (MissionStateChange, taskid);
					break;
				}
			}
			_taskSort = false;
            UpdateRedPoint();
		}
	}

	public void GainReward(uint taskid)
	{
		GetTaskRewardReq gtr = new GetTaskRewardReq();
		gtr.task_id = taskid;
		GameNetManager.Instance.Client.sendMessage<GetTaskRewardReq>(gtr, MSGID.CMSG_GET_TASK_REWARD_REQ);
		MessageLoading.Instance.show ();
	}

	public static bool IsDownloadMission(PlayerTaskInfo task)
	{
		return task.event_type==19||task.event_type==20;
	}

    void UpdateRedPoint()
    {
        if (TaskList == null)return;
        int count = 0;
        for (int i = 0; i < TaskList.tasks.Count; i++)
        {
            PlayerTaskInfo taskinfo = TaskList.tasks [i];
            if ((PlayerTaskState)taskinfo.task_state == PlayerTaskState.TaskFinishState)
            {
                ++count;
            }
        }
        RedPointManager.Single.set("MISSION", count);
    }

	#region 下载任务数据
	public class DownloadMission : IData
	{
		public const int CaleSizeDoing    = 1;
		public const int CaleSizeComplete = 2;
		public const int CaleSizeFailed   = 3;
		public const int DownloadDoing    = 4;
		public const int DownloadComplete = 5;
		public const int DownloadFailed   = 6;

		int    _downTag;
		uint   _taskId;
		string _appid;
		string _package;
		public uint taskId{get{return _taskId;}}
		ZUpdate.FileDownLoad _fdl;
		public ZUpdate.FileDownLoad fdl{get{return _fdl;}}

		string getDownInfo(string taskExtra)
		{
			string url = "";
			if (string.IsNullOrEmpty (taskExtra))return url;
			try
			{
				JsonData data = JsonMapper.ToObject (taskExtra);
				#if !UNITY_IPHONE && !UNITY_IOS
				JsonData android = data["android"] as JsonData;
				url = android["url"].ToString();
				_package =  android["package"].ToString();
				#else
				JsonData ios = data["ios"] as JsonData;
				url = ios["url"].ToString();
				_appid =  ios["appid"].ToString();
				#endif
				return url;

			}
			catch(Exception e)
			{
				Log.E ("taskExtra is invalid json,"+e.Message,Log.Tag.Task);
				return url;
			}
		}

		int getDownloadTag()
		{
			if (_downTag > 0)return _downTag;
			ulong  playerId = GameDataManager.Instance.PlayerAccId;
			string playerIdAndMissionId = playerId.ToString () + _taskId.ToString ();
			_downTag = UnityEngine.PlayerPrefs.GetInt ("mission"+playerIdAndMissionId, 0);
			if (_downTag > 0)return _downTag;
			if (fdl.downSize!= fdl.totalSize)return _downTag;
			_downTag = 1;
			UnityEngine.PlayerPrefs.SetInt ("mission" + playerIdAndMissionId, 1);
			return _downTag;
		}

		public DownloadMission(uint id, string taskExt)
		{
			string url = getDownInfo(taskExt);
			_taskId = id;
			string apkName = System.IO.Path.GetFileName(url);
			ulong  playerId = GameDataManager.Instance.PlayerAccId;
			string missionTag ="mission"+Scripts.CoreScripts.Core.Util.Utils.getMD5(playerId.ToString()+_taskId.ToString());
			string path = Application.persistentDataPath+"/"+missionTag+apkName;
			_fdl = new ZUpdate.FileDownLoad(url, path, null);
			_fdl.Start(CaleSizeCallBack, ZUpdate.FileDownLoad.Action.CalcSize);
		}

		void clearOldMissionFile()
		{
			string apkName = System.IO.Path.GetFileName(_fdl.path);
			ulong  playerId = GameDataManager.Instance.PlayerAccId;
			string missionTag ="mission"+Scripts.CoreScripts.Core.Util.Utils.getMD5(playerId.ToString()+_taskId.ToString());
			DirectoryInfo di = new DirectoryInfo(Application.persistentDataPath);
			FileInfo[] fis = di.GetFiles ("*.apk");
			for (int i = 0, max = fis.Length; i < max; ++i) 
			{
				if (!fis [i].Name.StartsWith (missionTag))continue;
				if (!fis [i].Name.EndsWith(apkName))fis [i].Delete ();
			}
		}

		void CaleSizeCallBack(ZUpdate.FileDownLoad fdl)
		{
			if (fdl.state == ZUpdate.FileDownLoad.State.Doing) {
				notify (CaleSizeDoing);
			} else if (fdl.state == ZUpdate.FileDownLoad.State.Completed) {
				notify (CaleSizeComplete);
			} else if (fdl.state == ZUpdate.FileDownLoad.State.Failed) {
				notify (CaleSizeFailed);
			}
		}

		void DownLoadCallBack(ZUpdate.FileDownLoad fdl)
		{
			if (fdl.state == ZUpdate.FileDownLoad.State.Doing) {
				notify (DownloadDoing);
			} else if (fdl.state == ZUpdate.FileDownLoad.State.Completed) {
				notify (DownloadComplete);
			} else if (fdl.state == ZUpdate.FileDownLoad.State.Failed) {
				notify (DownloadFailed);
			}
		}

		public void Start()
		{
			#if !UNITY_IPHONE && !UNITY_IOS
			clearOldMissionFile();
			#endif
			_fdl.Start (DownLoadCallBack);
		}

		public void Stop()
		{
			_fdl.Stop ();
		}

		public void Cancel()
		{
			_fdl.Stop ();
			File.Delete (_fdl.path);
			_fdl.Start(CaleSizeCallBack, ZUpdate.FileDownLoad.Action.CalcSize);
		}

		public void InstallApp()
		{//用户点击安装
			string path = _fdl.path;
			getDownloadTag ();
			#if UNITY_ANDROID
			AndroidJavaClass updateManagerClass = new AndroidJavaClass("com.baina.goldshark.update.UpdateManager");
			AndroidJavaObject updateInstance = updateManagerClass.CallStatic<AndroidJavaObject>("Instance");
			updateInstance.Call("installApk", path);
			#endif
		}

		public void CheckAppInstalled()
		{//检测用户是否安装
			if(string.IsNullOrEmpty(_package))return;
			if (getDownloadTag () < 1)return;//玩家没有下载过
			bool hasInstall = false;
			#if UNITY_ANDROID
			using (AndroidJavaClass cls = new AndroidJavaClass ("com.baina.goldshark.update.Helper"))
			{
			hasInstall = cls.CallStatic<bool>("isAppInstalled", _package);
			}
			#endif
			if (!hasInstall)return;
			FinishTaskNotice ftn = new FinishTaskNotice ();
			ftn.task_id = (int)_taskId;
			GameNetManager.Instance.Client.sendMessage<FinishTaskNotice> (ftn, MSGID.C2S_FINISH_TAKS_NOTIFY);
		}

		public void Dispose()
		{
			_fdl.Dispose ();
		}
	}
	List<DownloadMission> _downloadMission = new List<DownloadMission>();
	public DownloadMission GetDownloadMission(uint id, PlayerTaskInfo task)
	{
		DownloadMission dm;
		for (int i = 0; i < _downloadMission.Count; ++i) {
			dm = _downloadMission [i];
			if (dm.taskId == id)return dm;
		}

		/*JsonData jd = new JsonData ();
		jd ["android"] = new JsonData ();
		jd["android"]["url"] = "";
		jd["android"]["package"]="";
		jd ["ios"] = new JsonData ();
		jd["ios"]["url"] = "";
		jd["ios"]["appid"]="";
		Debug.LogError (jd.ToJson());*/
		dm = new DownloadMission (id, task.extra);//"http://dn.cdn.dolphin-game.com/res/downloads/dn/ttdn-4.5.0/BYDL-ttdn-4.5.0.apk"
		_downloadMission.Add (dm);
		return dm;
	}

	public void Clear()
	{
		for (int i = 0; i < _downloadMission.Count; ++i) 
		{
			_downloadMission [i].Dispose();
		}
		_downloadMission.Clear ();
	}
	#endregion
}
