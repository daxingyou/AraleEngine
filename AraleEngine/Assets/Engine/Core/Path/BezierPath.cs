using UnityEngine;

public class BezierPath : IPath {
	private  Vector3 _trackPos;
	private  Vector3 _startPos;
	private  Vector3 _controlPos;
	private  Vector3 _targetPos;
	private  float _duration=0;

	public BezierPath(Vector3 startPos,Vector3 controlPos,Vector3 targetPos,float duration)
	{
		_startPos = startPos;
		_trackPos = startPos;
		_controlPos = controlPos;
		_targetPos = targetPos;
		_duration = duration;
		
	}
	public bool Track (float ctime)
	{	
		float ratio = ctime / _duration;
		if (ratio >= 0.99f) {
			_trackPos = _targetPos;
			
		} else {
			_trackPos=BezierMultiplier(_startPos,_controlPos, _targetPos,ratio );				
		}
		if (ctime > _duration) {
			return false;
		}
		return true;
		
	}

	private Vector3 BezierMultiplier(Vector3 p0,Vector3 p1,Vector3 p2,float t)
	{
		return Mathf.Pow(1.0f - t, 2)*p0+2 * t * (1.0f - t) * p1 + Mathf.Pow(t, 2) * p2;

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
