using UnityEngine;
using System.Collections;
using Arale.Engine;

//StartWindow最好不要Lua化,防止更新了错误资源无法重更新恢复
public class StartWindow : Window
{
    public float splashTime=2;
    public CanvasGroup mSplashView;
    public GameObject mStartView;
    public UnzipView  mUnzipView;
    public UpdateView mUpdateView;
    public override void OnWindowEvent(Window.Event eventId)
    {
        if (eventId == Event.Create)
        {
            EventMgr.single.AddListener(GRoot.EventResUnzip,OnResUnzipCallback);
            EventMgr.single.AddListener(GRoot.EventResUpdate,OnResUpdateCallback);
        }
        else if (eventId == Event.Destroy)
        {
            EventMgr.single.RemoveListener(GRoot.EventResUnzip,OnResUnzipCallback);
            EventMgr.single.RemoveListener(GRoot.EventResUpdate,OnResUpdateCallback);
        }
    }

    void OnResUnzipCallback(EventMgr.EventData ed)
    {
        if (!(bool)ed.data)return;
        mUnzipView.gameObject.SetActive(false);
        mUpdateView.gameObject.SetActive(true);
    }

    void OnResUpdateCallback(EventMgr.EventData ed)
    {
        if (!(bool)ed.data)return;
        mUpdateView.gameObject.SetActive(false);
        GRoot.single.ReloadResource();
        mStartView.gameObject.SetActive(true);
    }
}
