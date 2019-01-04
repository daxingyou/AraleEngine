using UnityEngine;

public class StraightPath : IPath {
	private  Vector3 _trackPos;
	private  Vector3 _startPos;
	private  Vector3 _controlPos;
	private  Vector3 _targetPos;
	private  float _duration = 0;

	public StraightPath(Vector3 startPos,Vector3 targetPos,float duration)
	{
		_startPos = startPos;	
		_targetPos = targetPos;	
		_duration = duration;
		_trackPos = startPos;
	}
	public bool Track (float ctime)
	{

		float ratio = ctime / _duration;
		if (ratio >= 0.99f) {
			_trackPos = _targetPos;
		} else {
			_trackPos = Vector3.Lerp (_startPos,_targetPos,ratio);
		}
		if (ctime > _duration) {
			return false;
		}
		return true;

	}

	public Vector3 TrackPos {
		get{ 
			return _trackPos;
		}
	}
	public float Duration
	{
		get{ 
			return _duration;
		}
	}
}
