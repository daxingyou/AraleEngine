/*using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class UpdateView : MonoBehaviour {
	public static string _updateURL;
    public Text _infoLB;
    public Text _sizeLB;
    public Text _progressLB;
    public Slider _progressBAR;
    public Button _cancelBT;
    public Text _cancelLB;

    // Use this for initialization
    void Start () {
        _update.init("", "", 0);
        _cancelBT.onClick.AddListener(OnCancel);

		MessageBox.show("有新的资源包v"+_updateVersion+",是否现在更新", () => {//cancel
            _cancelLB.text = "继续";
            _cancelBT.gameObject.SetActive(true);
        }, () => {//comfirm
            BeginUpdate();
        });
    }

    void OnDestroy()
    {
        _action = null;
        _update.cancel();
        _update.deinit();
    }

    // Update is called once per frame
    void Update () {
        EventManager.single.update();
    }

    public static void StartUpdate(Transform mount, int newVersion, System.Action action=null)
    {
		Log.I ("StartUpdate newVersion="+newVersion, Log.Tag.Update);
		if (newVersion < 1) 
		{
			Log.E ("newVersion can't be zero", Log.Tag.Update);
			return;
		}
		if (newVersion<= ResLoad.GetResVersion())
        {
            if(action!=null) action();
            return;
        }

        GameObject go = ResLoad.Get("UI/Update/UpdateView").GetGameObject();
        go.transform.parent = mount;
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;
        RectTransform rt = go.transform as RectTransform;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        UpdateView uv = go.GetComponent<UpdateView>();
		uv._updateVersion = newVersion;
        uv._action = action;
    }

    #region 更新
    ZUpdate _update = new ZUpdate();
    System.Action _action;
    bool _cannel = true;
    int _updateVersion = 0;
    int _updateSize = 0;
    void BeginUpdate()
    {
        if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
        {
            DoBeginUpdate();
        }
        else
        {
            MessageBox.show("非Wifi联网模式下载将产生流量费用，是否继续下载", () => {//cancel
                _cancelLB.text = "继续";
                _cancelBT.gameObject.SetActive(true);
            }, () => {//comfirm
                DoBeginUpdate();
            });
        }
    }

    void DoBeginUpdate()
    {
        _cannel = false;
        _updateSize = 0;
        _infoLB.text = "开始更新";
        _cancelLB.text = "暂停";
        _cancelBT.gameObject.SetActive(false);
		_update.updateAssetBundle(string.IsNullOrEmpty(_updateURL)?GameDataManager.Instance.assetsBunfleInfo.ab_update_url:_updateURL, OnUpdate, _updateVersion);
    }

    void OnCancel()
    {
        if (_cannel)
        {
            BeginUpdate();
        }
        else
        {
            _cannel = true;
            _update.cancel();
        }
    }

    void OnUpdate(ZUpdate.UpdateEventValue v)
    {
        _progressBAR.value = v.progress;
        if(!string.IsNullOrEmpty(v.info))_infoLB.text = v.info;

        switch (v.evt)
        {
            case ZUpdate.Event.DownConfig:
                break;
            case ZUpdate.Event.CheckFile:
                break;
            case ZUpdate.Event.CalcSize:
                _cancelBT.gameObject.SetActive(true);
                _updateSize = v.resSize;
                _progressLB.text = string.Format("{0}%", (int)(100*v.progress));
                _sizeLB.text = string.Format("{0}/{1}", ToSize((int)(v.progress * _updateSize)), ToSize(_updateSize));
                break;
            case ZUpdate.Event.DownFile:
                _progressLB.text = string.Format("{0}%", (int)(100 *v.progress));
                _sizeLB.text = string.Format("{0}/{1}", ToSize((int)(v.progress * _updateSize)), ToSize(_updateSize));
                break;
			case ZUpdate.Event.Success:
				_progressLB.text = "100%";
				_sizeLB.text = string.Format ("{0}/{1}", ToSize ((int)(_updateSize)), ToSize (_updateSize));
				ResMgr.Single.Reset();
                if (_action != null) _action();
				Destroy (gameObject);
                break;
            case ZUpdate.Event.Failure:
                _cancelLB.text = _cannel ? "继续" : "重试";
                _cancelBT.gameObject.SetActive(true);
                _cannel = true;
                _update.cancel();
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
    #endregion
}
*/