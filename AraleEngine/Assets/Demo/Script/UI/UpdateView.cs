using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Arale.Engine;


public class UpdateView : MonoBehaviour {
	public static string _updateURL;
    public Text _infoLB;
    public Slider _progressBAR;
    public Button _cancelBT;

    ResUpdate.PatchTask mTask;
    // Use this for initialization
    void Start ()
    {
        mTask = ResUpdate.single.checkVersion(1, GRoot.single.mResServer);
        if (mTask == null)
        {
            EventMgr.single.SendEvent(GRoot.EventResUpdate, true);
        }
        else
        {
            _cancelBT.onClick.AddListener(OnCancel);
            mTask.addListern(OnPatchTaskCallback);
            StartUpdate();
        }
    }

    void OnDestroy()
    {
        if (mTask == null)return;
        mTask.stop();
        mTask.removeListern(OnPatchTaskCallback);
    }

    void StartUpdate()
    {
        if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
        {
            mTask.start();
        }
        else
        {
            /*MessageBox.show("非Wifi联网模式下载将产生流量费用，是否继续下载", () => {//cancel
                _cancelLB.text = "继续";
                _cancelBT.gameObject.SetActive(true);
            }, () => {//comfirm
                DoBeginUpdate();
            });*/
        }
    }

    bool _cancel;
    void DoBeginUpdate()
    {
        _cancel = false;
        _infoLB.text = "开始更新";
        _cancelBT.GetComponentInChildren<Text>().text = "暂停";
        _cancelBT.gameObject.SetActive(false);
        mTask.start();
    }

    void OnCancel()
    {
        if (_cancel)
        {
            StartUpdate();
        }
        else
        {
            _cancel = true;
            mTask.stop();
        }
    }

    void OnPatchTaskCallback(ResUpdate.PatchTask task)
    {
        _progressBAR.value = task.mProgress;
        switch(task.mState)
        {
            case ResUpdate.State.Doing:
                break;
            case ResUpdate.State.Completed:
                EventMgr.single.SendEvent(GRoot.EventResUpdate, true);
                break;
            case ResUpdate.State.Failed:
                _cancelBT.GetComponentInChildren<Text>().text = _cancel ? "继续" : "重试";
                _cancelBT.gameObject.SetActive(true);
                _cancel = true;
                break;
        }
    }

    string ToSize(int bytes)
    {
        if (bytes > 1024 * 1024 * 1024)
        {
            return (1.0f * bytes / (1024 * 1024 * 1024)).ToString("f2") + "GB";
        }
        else if (bytes > 1024 * 1024)
        {
            return (1.0f * bytes / (1024 * 1024)).ToString("f2") + "MB";
        }
        else if (bytes > 1024)
        {
            return (1.0f * bytes / 1024).ToString("f2") + "KB";
        }
        else
        {
            return (float)bytes + "B";
        }
    }
}