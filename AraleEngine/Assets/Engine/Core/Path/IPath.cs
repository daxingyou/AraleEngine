using UnityEngine;

public interface IPath  {
	bool Track (float ctime);
	Vector3 TrackPos {
		get;
	}
	float  Duration {
		get;
	}

}

