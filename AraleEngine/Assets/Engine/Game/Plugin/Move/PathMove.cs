using UnityEngine;
using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using DG.Tweening.Plugins.Core.PathCore;


public class PathMove : Move
{
    TweenerCore<Vector3, Path, PathOptions> mPathTween;
    protected override void start(Unit unit)
    {
        mSpeed = unit.speed;
    }

    void onWaypointChange(int pointIdx)
    {
    }

    protected override void update(Unit unit)
    {
        mPathTween.timeScale = unit.scale;
    }

    protected override void stop(Unit unit, bool arrived)
    {
        unit.move.moveState = State.None;
        mPathTween.Pause();
    }

    public void play(Unit unit, string pathName, bool autoPaly, bool inverse)
    {
        start(unit);
        Vector3[] path = iTweenPath.GetPath(pathName);
        if (inverse)System.Array.Reverse(path);
        Vector3 d = path[0] - vTarget;
        for (int i = 0; i < path.Length; ++i)path[i] = path[i] - d;
        mPathTween = unit.transform.DOPath(path, mSpeed, PathType.CatmullRom, PathMode.Full3D);
        if(!autoPaly)mPathTween.Pause();
        mPathTween.SetSpeedBased(true);
        mPathTween.timeScale = unit.scale;
        mPathTween.SetLookAt(0).SetEase(Ease.Linear).OnWaypointChange(onWaypointChange).OnComplete(delegate {mPathTween = null;  });
    }

    public void forward()
    {
        if (mPathTween == null)return;
        mPathTween.PlayForward();
    }

    public void backward()
    {
        if (mPathTween == null)return;
        mPathTween.PlayBackwards();
    }
}
